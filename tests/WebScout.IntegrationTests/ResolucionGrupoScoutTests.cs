using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

/// <summary>
/// Resolución del tenant por subdominio contra <c>GruposScout</c>: grupos creados en base de datos,
/// subdominios desconocidos o inactivos (404), portal federativo, caché e invalidación al guardar,
/// y aislamiento entre grupos (PA18). Todos los datos son sintéticos.
/// </summary>
public class ResolucionGrupoScoutTests : IDisposable
{
    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OrgInactivo = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly PaletteConfig PaletaSanVicente = new()
    {
        PrimaryHex = "#0D47A1",
        SecondaryHex = "#D32F2F",
        AccentHex = "#F57C00",
        TextOnPrimaryHex = "#FFFFFF",
        TextOnSecondaryHex = "#FFFFFF",
        SurfaceHeaderHex = "#0D47A1"
    };

    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly ServiceProvider _provider;
    private readonly IConfiguration _configuration;

    public ResolucionGrupoScoutTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "Tenancy:BaseDomain", "scoutsclm.org" } })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_configuration);
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddSingleton<IResolutorGrupoScout, ResolutorGrupoScout>();
        services.AddSingleton<InvalidacionCacheGruposInterceptor>();
        services.AddDbContext<WebScoutDbContext>((sp, options) =>
        {
            options.UseNpgsql(_baseDatos.CadenaConexion);
            options.AddInterceptors(sp.GetRequiredService<InvalidacionCacheGruposInterceptor>());
        });
        _provider = services.BuildServiceProvider();

        using var scope = CrearAmbitoFederativo(out var db);

        db.GruposScout.AddRange(
            CrearGrupo(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330, PaletteConfig.DefaultLestonnac),
            CrearGrupo(OrgSanVicente, "sanvicente", "Grupo Scout San Vicente 412", 412, PaletaSanVicente),
            CrearGrupo(OrgInactivo, "dormido", "Grupo Scout Dormido 999", 999, PaletteConfig.DefaultLestonnac, activo: false));

        db.Actividades.AddRange(
            new ActividadScout { OrganizacionId = OrgLestonnac, Titulo = "Acampada sintética Lestonnac", Lugar = "Lugar A" },
            new ActividadScout { OrganizacionId = OrgSanVicente, Titulo = "Acampada sintética San Vicente", Lugar = "Lugar B" });

        db.SaveChanges();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _baseDatos.Dispose();
    }

    // ---------------------------------------------------------------------
    // Resolución por subdominio contra GruposScout
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GrupoCreadoEnBaseDeDatos_SeResuelvePorSubdominio_ConNombreNumeroYPaleta()
    {
        var nuevoId = Guid.NewGuid();
        var paleta = new PaletteConfig
        {
            PrimaryHex = "#4A148C",
            SecondaryHex = "#00695C",
            AccentHex = "#E65100",
            TextOnPrimaryHex = "#FFFFFF",
            TextOnSecondaryHex = "#FFFFFF",
            SurfaceHeaderHex = "#4A148C"
        };

        using (var scope = CrearAmbitoFederativo(out var db))
        {
            db.GruposScout.Add(CrearGrupo(nuevoId, "robledal", "Grupo Scout Robledal 777", 777, paleta));
            await db.SaveChangesAsync();
        }

        var resultado = await EjecutarMiddlewareAsync("robledal.scoutsclm.org");

        Assert.True(resultado.SiguienteInvocado);
        Assert.Equal(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.Equal(nuevoId, resultado.Tenant.CurrentTenantId);
        Assert.Equal("robledal", resultado.Tenant.TenantSlug);
        Assert.Equal("Grupo Scout Robledal 777", resultado.Tenant.GroupName);
        Assert.Equal(777, resultado.Tenant.GroupNumber);
        Assert.Equal(paleta, resultado.Tenant.Palette);

        // La paleta llega al tema de MudBlazor de la gestión (MainLayout usa ScoutMudTheme.CreateTheme(Tenant.Palette))
        var tema = ScoutMudTheme.CreateTheme(resultado.Tenant.Palette);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#4A148C"), tema.PaletteLight.Primary);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#00695C"), tema.PaletteLight.Secondary);
    }

    [Theory]
    [InlineData("SanVicente.scoutsclm.org")]
    [InlineData("sanvicente.localhost")]
    public async Task GrupoExistente_SeResuelveSinDistinguirMayusculas_YEnLocalhost(string host)
    {
        var resultado = await EjecutarMiddlewareAsync(host);

        Assert.True(resultado.SiguienteInvocado);
        Assert.Equal(OrgSanVicente, resultado.Tenant.CurrentTenantId);
        Assert.Equal(412, resultado.Tenant.GroupNumber);
        Assert.Equal("#0D47A1", resultado.Tenant.Palette?.PrimaryHex);
    }

    [Theory]
    [InlineData("fantasma.scoutsclm.org", null)]
    [InlineData("fantasma.localhost", null)]
    [InlineData("localhost", "fantasma")]
    [InlineData("sanvicente.scoutsclm.org", "../lestonnac")]
    public async Task SubdominioDesconocido_Responde404_SinCaerAOtroGrupo(string host, string? cabeceraSlug)
    {
        var resultado = await EjecutarMiddlewareAsync(host, cabeceraSlug);

        Assert.False(resultado.SiguienteInvocado);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.StatusCode);
        Assert.Equal(Guid.Empty, resultado.Tenant.CurrentTenantId);
        Assert.False(resultado.Tenant.IsResolved);
        Assert.Null(resultado.Tenant.Palette);
    }

    [Fact]
    public async Task GrupoInactivo_Responde404()
    {
        var resultado = await EjecutarMiddlewareAsync("dormido.scoutsclm.org");

        Assert.False(resultado.SiguienteInvocado);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.StatusCode);
        Assert.NotEqual(OrgInactivo, resultado.Tenant.CurrentTenantId);
        Assert.False(resultado.Tenant.IsResolved);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("scoutsclm.org")]
    [InlineData("www.scoutsclm.org")]
    [InlineData("admin.localhost")]
    [InlineData("admin.scoutsclm.org")]
    [InlineData("sclm.scoutsclm.org")]
    public async Task DominioRaizYAdminYSclm_SirvenPortalFederativo(string host)
    {
        var resultado = await EjecutarMiddlewareAsync(host);

        Assert.True(resultado.SiguienteInvocado);
        Assert.True(resultado.Tenant.IsResolved);
        Assert.Equal(Guid.Empty, resultado.Tenant.CurrentTenantId);
        Assert.Equal(PortalFederativo.Slug, resultado.Tenant.TenantSlug);
        Assert.Null(resultado.Tenant.Palette);
    }

    // ---------------------------------------------------------------------
    // Caché e invalidación al guardar
    // ---------------------------------------------------------------------

    [Fact]
    public async Task PaletaGuardada_InvalidaCache_YSeAplicaEnLaSiguientePeticion()
    {
        var antes = await EjecutarMiddlewareAsync("lestonnac.scoutsclm.org");
        Assert.Equal("#1B5E20", antes.Tenant.Palette?.PrimaryHex);

        using (var scope = CrearAmbitoFederativo(out var db))
        {
            var grupo = await db.GruposScout.SingleAsync(g => g.Id == OrgLestonnac);
            grupo.Paleta = grupo.Paleta with { PrimaryHex = "#004D40", SurfaceHeaderHex = "#004D40" };
            await db.SaveChangesAsync();
        }

        var despues = await EjecutarMiddlewareAsync("lestonnac.scoutsclm.org");
        Assert.Equal("#004D40", despues.Tenant.Palette?.PrimaryHex);
        Assert.Equal(new MudBlazor.Utilities.MudColor("#004D40"), ScoutMudTheme.CreateTheme(despues.Tenant.Palette).PaletteLight.Primary);
    }

    [Fact]
    public async Task GrupoDesactivado_DejaDeResolverseInmediatamente()
    {
        Assert.True((await EjecutarMiddlewareAsync("sanvicente.scoutsclm.org")).SiguienteInvocado);

        using (var scope = CrearAmbitoFederativo(out var db))
        {
            var grupo = await db.GruposScout.SingleAsync(g => g.Id == OrgSanVicente);
            grupo.Activo = false;
            await db.SaveChangesAsync();
        }

        var resultado = await EjecutarMiddlewareAsync("sanvicente.scoutsclm.org");
        Assert.False(resultado.SiguienteInvocado);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.StatusCode);
    }

    [Fact]
    public async Task GrupoDadoDeAltaTrasUn404_SeResuelveSinEsperarACaducidad()
    {
        Assert.Equal(StatusCodes.Status404NotFound, (await EjecutarMiddlewareAsync("encinar.scoutsclm.org")).StatusCode);

        var id = Guid.NewGuid();
        using (var scope = CrearAmbitoFederativo(out var db))
        {
            db.GruposScout.Add(CrearGrupo(id, "encinar", "Grupo Scout Encinar 501", 501, PaletaSanVicente));
            await db.SaveChangesAsync();
        }

        var resultado = await EjecutarMiddlewareAsync("encinar.scoutsclm.org");
        Assert.True(resultado.SiguienteInvocado);
        Assert.Equal(id, resultado.Tenant.CurrentTenantId);
    }

    [Fact]
    public async Task Resolutor_CacheaPorSlug_HastaQueSeInvalida()
    {
        var resolutor = _provider.GetRequiredService<IResolutorGrupoScout>();
        Assert.Equal("Grupo Scout Lestonnac 330", (await resolutor.ResolverAsync("lestonnac"))?.Nombre);

        // Cambio directo en SQL, sin pasar por EF: la caché sigue sirviendo el valor anterior
        using (var conexion = new Npgsql.NpgsqlConnection(_baseDatos.CadenaConexion))
        {
            conexion.Open();
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = "UPDATE \"GruposScout\" SET \"Nombre\" = 'Nombre cambiado fuera de EF' WHERE \"Slug\" = 'lestonnac'";
            Assert.Equal(1, cmd.ExecuteNonQuery());
        }
        Assert.Equal("Grupo Scout Lestonnac 330", (await resolutor.ResolverAsync("lestonnac"))?.Nombre);

        resolutor.Invalidar("LESTONNAC");
        Assert.Equal("Nombre cambiado fuera de EF", (await resolutor.ResolverAsync("lestonnac"))?.Nombre);
    }

    // ---------------------------------------------------------------------
    // PA18: aislamiento entre grupos con el tenant resuelto desde base de datos
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData("lestonnac.scoutsclm.org", "Acampada sintética Lestonnac")]
    [InlineData("sanvicente.scoutsclm.org", "Acampada sintética San Vicente")]
    public async Task PA18_PeticionAUnGrupo_SoloVeDatosDeEseGrupo(string host, string tituloEsperado)
    {
        using var scope = _provider.CreateScope();
        var tenant = scope.ServiceProvider.GetRequiredService<TenantContext>();
        var resultado = await EjecutarMiddlewareAsync(host, scope: scope);
        Assert.True(resultado.SiguienteInvocado);

        var db = scope.ServiceProvider.GetRequiredService<WebScoutDbContext>();
        var actividades = await db.Actividades.AsNoTracking().ToListAsync();

        var actividad = Assert.Single(actividades);
        Assert.Equal(tituloEsperado, actividad.Titulo);
        Assert.Equal(tenant.CurrentTenantId, actividad.OrganizacionId);
    }

    [Fact]
    public async Task PA18_CircuitoInteractivo_ResuelveElGrupoDelHost_YNoVeOtroGrupo()
    {
        // En el circuito de Blazor no corre el middleware: TenantContext se resuelve de forma perezosa
        using var db = CrearDbContextDeCircuito("sanvicente.scoutsclm.org", out var tenant);

        Assert.True(tenant.IsResolved);
        Assert.Equal(OrgSanVicente, tenant.CurrentTenantId);
        Assert.Equal("#0D47A1", tenant.Palette?.PrimaryHex);

        var actividades = await db.Actividades.AsNoTracking().ToListAsync();
        Assert.All(actividades, a => Assert.Equal(OrgSanVicente, a.OrganizacionId));
        Assert.Single(actividades);
    }

    [Theory]
    [InlineData("fantasma.scoutsclm.org")]
    [InlineData("dormido.scoutsclm.org")]
    public async Task PA18_CircuitoConSubdominioDesconocidoOInactivo_NoCaeAOtroGrupoNiVeDatos(string host)
    {
        using var db = CrearDbContextDeCircuito(host, out var tenant);

        Assert.False(tenant.IsResolved);
        Assert.Equal(Guid.Empty, tenant.CurrentTenantId);
        Assert.Null(tenant.Palette);

        Assert.Empty(await db.Actividades.AsNoTracking().ToListAsync());
    }

    // ---------------------------------------------------------------------
    // Utilidades
    // ---------------------------------------------------------------------

    private sealed record ResultadoMiddleware(bool SiguienteInvocado, int StatusCode, TenantContext Tenant);

    private async Task<ResultadoMiddleware> EjecutarMiddlewareAsync(string host, string? cabeceraSlug = null, IServiceScope? scope = null)
    {
        var ownScope = scope is null ? _provider.CreateScope() : null;
        var activeScope = scope ?? ownScope!;
        try
        {
            var siguienteInvocado = false;
            var middleware = new TenantResolutionMiddleware(
                _ => { siguienteInvocado = true; return Task.CompletedTask; },
                _configuration,
                NullLogger<TenantResolutionMiddleware>.Instance);

            var context = new DefaultHttpContext { RequestServices = activeScope.ServiceProvider };
            context.Request.Host = new HostString(host);
            if (cabeceraSlug is not null)
            {
                context.Request.Headers["X-Tenant-Slug"] = cabeceraSlug;
            }

            var tenant = activeScope.ServiceProvider.GetRequiredService<TenantContext>();
            await middleware.InvokeAsync(context, tenant, activeScope.ServiceProvider.GetRequiredService<IResolutorGrupoScout>());

            return new ResultadoMiddleware(siguienteInvocado, context.Response.StatusCode, tenant);
        }
        finally
        {
            ownScope?.Dispose();
        }
    }

    /// <summary>
    /// Equivalente a un circuito interactivo: TenantContext con HttpContextAccessor y resolutor,
    /// sin pasar por el middleware, y un DbContext que lo usa para sus filtros globales.
    /// </summary>
    private WebScoutDbContext CrearDbContextDeCircuito(string host, out TenantContext tenant)
    {
        var http = new DefaultHttpContext();
        http.Request.Host = new HostString(host);

        tenant = new TenantContext(
            new HttpContextAccessor { HttpContext = http },
            authStateProvider: null,
            resolutor: _provider.GetRequiredService<IResolutorGrupoScout>(),
            configuration: _configuration);

        var options = _baseDatos.Opciones;
        return new WebScoutDbContext(options, tenant);
    }

    private IServiceScope CrearAmbitoFederativo(out WebScoutDbContext db)
    {
        var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<TenantContext>()
            .SetTenant(Guid.Empty, PortalFederativo.Slug, PortalFederativo.Nombre, 0);
        db = scope.ServiceProvider.GetRequiredService<WebScoutDbContext>();
        return scope;
    }

    private static GrupoScout CrearGrupo(Guid id, string slug, string nombre, int numero, PaletteConfig paleta, bool activo = true) => new()
    {
        Id = id,
        Slug = slug,
        Nombre = nombre,
        NumeroGrupo = numero,
        EmailContacto = $"contacto@{slug}.example.org",
        Activo = activo,
        Paleta = paleta
    };
}
