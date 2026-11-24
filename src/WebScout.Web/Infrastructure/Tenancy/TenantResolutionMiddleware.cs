using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Middleware de ASP.NET Core que inspecciona el host de la petición HTTP, extrae el subdominio
/// del grupo scout, lo resuelve contra <c>GruposScout</c> (solo grupos activos) y establece el
/// <see cref="ITenantContext"/> con su nombre, número y paleta.
/// El dominio raíz y los subdominios <c>admin</c>/<c>sclm</c> sirven el portal federativo.
/// Un subdominio desconocido o inactivo responde 404: nunca se cae a otro grupo.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly string _baseDomain;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _baseDomain = configuration.GetValue<string>("Tenancy:BaseDomain") ?? ExtractorSlugTenant.DominioBasePorDefecto;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, IResolutorGrupoScout resolutor)
    {
        var tenantSlug = ExtractTenantSlug(context);

        if (string.IsNullOrWhiteSpace(tenantSlug) || PortalFederativo.EsSlugFederativo(tenantSlug))
        {
            tenantContext.SetTenant(Guid.Empty, PortalFederativo.Slug, PortalFederativo.Nombre, 0);
            await _next(context);
            return;
        }

        var grupo = await resolutor.ResolverAsync(tenantSlug, context.RequestAborted);
        if (grupo is null)
        {
            if (context.Features.Get<IStatusCodeReExecuteFeature>() is not null)
            {
                // Re-ejecución de UseStatusCodePagesWithReExecute para pintar /not-found:
                // se sirve sin datos de ningún grupo.
                tenantContext.SetTenant(Guid.Empty, PortalFederativo.Slug, PortalFederativo.Nombre, 0);
                await _next(context);
                return;
            }

            _logger.LogWarning("Subdominio de grupo scout desconocido o inactivo: {TenantSlug}. Se responde 404.", tenantSlug);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        tenantContext.SetTenant(grupo.Id, grupo.Slug, grupo.Nombre, grupo.NumeroGrupo, grupo.Paleta);
        await _next(context);
    }

    /// <summary>
    /// Extrae el slug según el orden definido en <see cref="ExtractorSlugTenant.Extraer"/>.
    /// </summary>
    public string? ExtractTenantSlug(HttpContext context) => ExtractorSlugTenant.Extraer(context.Request, _baseDomain);
}
