using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Middleware de ASP.NET Core que inspecciona el host de la petición HTTP,
/// extrae el subdominio del grupo scout y establece el ITenantContext activo.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly string _baseDomain;
    private readonly string _fallbackTenant;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _baseDomain = configuration.GetValue<string>("Tenancy:BaseDomain") ?? "scoutsclm.org";
        _fallbackTenant = configuration.GetValue<string>("Tenancy:FallbackTenantSlug") ?? "lestonnac";
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        var tenantSlug = ExtractTenantSlug(context);

        if (!string.IsNullOrWhiteSpace(tenantSlug))
        {
            // Nota: En la fase inicial se resuelve con presets conocidos.
            // Al conectar la base de datos se resuelve contra el caché / WebScoutDbContext.
            if (tenantSlug.Equals("lestonnac", StringComparison.OrdinalIgnoreCase))
            {
                tenantContext.SetTenant(
                    tenantId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    slug: "lestonnac",
                    groupName: "Grupo Scout Lestonnac 330",
                    groupNumber: 330
                );
            }
            else if (tenantSlug.Equals("sanvicente", StringComparison.OrdinalIgnoreCase))
            {
                tenantContext.SetTenant(
                    tenantId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    slug: "sanvicente",
                    groupName: "Grupo Scout San Vicente 412",
                    groupNumber: 412
                );
            }
            else
            {
                _logger.LogWarning("Se solicitó un subdominio no reconocido: {TenantSlug}", tenantSlug);
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Algoritmo de extracción de slug según el orden:
    /// 1. Cabecera HTTP X-Tenant-Slug (pruebas e integraciones)
    /// 2. Query string ?tenant=xxx (facilidad en desarrollo)
    /// 3. Subdominio en el Host (producción: {slug}.scoutsclm.org o local: {slug}.localhost)
    /// 4. Fallback configurado para entorno Development
    /// </summary>
    public string? ExtractTenantSlug(HttpContext context)
    {
        // 1. Cabecera de desarrollo / test
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerSlug) && !string.IsNullOrWhiteSpace(headerSlug))
        {
            return headerSlug.ToString().Trim().ToLowerInvariant();
        }

        // 2. Query parameter
        if (context.Request.Query.TryGetValue("tenant", out var querySlug) && !string.IsNullOrWhiteSpace(querySlug))
        {
            return querySlug.ToString().Trim().ToLowerInvariant();
        }

        // 3. Subdominio en Host
        var host = context.Request.Host.Host;

        // Caso desarrollo: {slug}.localhost
        if (host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
        {
            var parts = host.Split('.');
            if (parts.Length > 1 && !parts[0].Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                return parts[0].ToLowerInvariant();
            }
        }

        // Caso producción: {slug}.scoutsclm.org
        if (host.EndsWith(_baseDomain, StringComparison.OrdinalIgnoreCase))
        {
            var prefix = host.Substring(0, host.Length - _baseDomain.Length).TrimEnd('.');
            if (!string.IsNullOrWhiteSpace(prefix) && !prefix.Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                // Si viene como lestonnac.scoutsclm.org -> prefix = lestonnac
                var parts = prefix.Split('.');
                return parts[^1].ToLowerInvariant();
            }
        }

        // 4. Fallback en desarrollo si se accede a localhost sin subdominio
        if (context.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) is Microsoft.AspNetCore.Hosting.IWebHostEnvironment env && env.EnvironmentName == "Development")
        {
            return _fallbackTenant;
        }

        return null;
    }
}
