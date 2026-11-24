using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Common.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Datos del grupo scout necesarios para fijar el <see cref="ITenantContext"/> de una petición.
/// </summary>
public sealed record GrupoScoutResuelto(Guid Id, string Slug, string Nombre, int NumeroGrupo, PaletteConfig? Paleta);

/// <summary>
/// Resuelve un slug de subdominio a un grupo activo de <c>GruposScout</c>.
/// </summary>
public interface IResolutorGrupoScout
{
    /// <summary>
    /// Devuelve el grupo activo con ese slug, o <c>null</c> si no existe, está inactivo o el slug no es válido.
    /// </summary>
    ValueTask<GrupoScoutResuelto?> ResolverAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Variante síncrona para los getters de <see cref="TenantContext"/> en el circuito interactivo.
    /// </summary>
    GrupoScoutResuelto? Resolver(string slug);

    /// <summary>
    /// Descarta la entrada en caché del slug para que la siguiente petición lea la base de datos.
    /// </summary>
    void Invalidar(string slug);
}

/// <summary>
/// Implementación singleton con caché corta en memoria por slug (positiva y negativa).
/// Cada fallo de caché abre su propio ámbito de DI con el contexto fijado al portal federativo,
/// de modo que la consulta no depende del tenant de la petición en curso.
/// </summary>
public sealed partial class ResolutorGrupoScout : IResolutorGrupoScout, IDisposable
{
    private const int MaxEntradas = 1024;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = MaxEntradas });
    private readonly TimeSpan _duracionGrupo;
    private readonly TimeSpan _duracionDesconocido;

    public ResolutorGrupoScout(IServiceScopeFactory scopeFactory, IConfiguration? configuration = null)
    {
        _scopeFactory = scopeFactory;
        _duracionGrupo = TimeSpan.FromSeconds(configuration?.GetValue<int?>("Tenancy:CacheSegundos") ?? 60);
        _duracionDesconocido = TimeSpan.FromSeconds(configuration?.GetValue<int?>("Tenancy:CacheDesconocidoSegundos") ?? 15);
    }

    public async ValueTask<GrupoScoutResuelto?> ResolverAsync(string slug, CancellationToken cancellationToken = default)
    {
        var clave = Normalizar(slug);
        if (clave is null) return null;

        if (_cache.TryGetValue<EntradaCache>(clave, out var entrada) && entrada is not null)
        {
            return entrada.Grupo;
        }

        using var scope = CrearAmbitoFederativo(out var db);
        var grupo = await ConsultarActivo(db, clave).FirstOrDefaultAsync(cancellationToken);
        return Guardar(clave, grupo);
    }

    public GrupoScoutResuelto? Resolver(string slug)
    {
        var clave = Normalizar(slug);
        if (clave is null) return null;

        if (_cache.TryGetValue<EntradaCache>(clave, out var entrada) && entrada is not null)
        {
            return entrada.Grupo;
        }

        using var scope = CrearAmbitoFederativo(out var db);
        var grupo = ConsultarActivo(db, clave).FirstOrDefault();
        return Guardar(clave, grupo);
    }

    public void Invalidar(string slug)
    {
        var clave = Normalizar(slug);
        if (clave is not null)
        {
            _cache.Remove(clave);
        }
    }

    public void Dispose() => _cache.Dispose();

    private IServiceScope CrearAmbitoFederativo(out WebScoutDbContext db)
    {
        var scope = _scopeFactory.CreateScope();

        // Se fija el contexto antes de crear el DbContext para que nunca intente resolverse por sí mismo
        // (lo que volvería a llamar a este resolutor).
        scope.ServiceProvider.GetRequiredService<TenantContext>()
            .SetTenant(Guid.Empty, PortalFederativo.Slug, PortalFederativo.Nombre, 0);

        db = scope.ServiceProvider.GetRequiredService<WebScoutDbContext>();
        return scope;
    }

    private static IQueryable<GrupoScout> ConsultarActivo(WebScoutDbContext db, string slug) =>
        db.GruposScout
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(g => g.Activo && g.Slug.ToLower() == slug);

    private GrupoScoutResuelto? Guardar(string clave, GrupoScout? grupo)
    {
        var resuelto = grupo is null
            ? null
            : new GrupoScoutResuelto(grupo.Id, grupo.Slug.ToLowerInvariant(), grupo.Nombre, grupo.NumeroGrupo, grupo.Paleta);

        _cache.Set(clave, new EntradaCache(resuelto), new MemoryCacheEntryOptions
        {
            Size = 1,
            AbsoluteExpirationRelativeToNow = resuelto is null ? _duracionDesconocido : _duracionGrupo
        });

        return resuelto;
    }

    /// <summary>
    /// Devuelve el slug en minúsculas, o <c>null</c> si no tiene forma de etiqueta DNS válida,
    /// para no consultar ni cachear valores arbitrarios procedentes de cabeceras o query string.
    /// </summary>
    private static string? Normalizar(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        var normalizado = slug.Trim().ToLowerInvariant();
        return FormatoSlug().IsMatch(normalizado) ? normalizado : null;
    }

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$")]
    private static partial Regex FormatoSlug();

    private sealed record EntradaCache(GrupoScoutResuelto? Grupo);
}
