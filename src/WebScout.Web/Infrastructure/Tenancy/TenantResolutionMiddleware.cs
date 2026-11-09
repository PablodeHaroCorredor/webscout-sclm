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
            else if (tenantSlug.Equals("admin", StringComparison.OrdinalIgnoreCase) || tenantSlug.Equals("sclm", StringComparison.OrdinalIgnoreCase))
            {
                // Dominio de administración central o federación SCLM
                tenantContext.SetTenant(
                    tenantId: Guid.Empty,
                    slug: "sclm",
                    groupName: "Scouts de Castilla-La Mancha (SCLM)",
                    groupNumber: 0
                );
            }
            else
            {
                _logger.LogWarning("Se solicitó un subdominio no reconocido: {TenantSlug}", tenantSlug);
            }
        }
        else
        {
            // Petición a dominio raíz sin subdominio (portal federativo)
            tenantContext.SetTenant(
                tenantId: Guid.Empty,
                slug: "sclm",
                groupName: "Scouts de Castilla-La Mancha (SCLM)",
                groupNumber: 0
            );
        }

        await _next(context);
    }

    /// <summary>
    /// Algoritmo de extracción de slug según el orden:
    /// 1. Cabecera HTTP X-Tenant-Slug (pruebas e integraciones)
    /// 2. Subdominio en el Host (ej. lestonnac.localhost o {slug}.scoutsclm.org)
    /// 3. Parámetro de consulta '?tenant=' (para pruebas o desarrollo sin selector en UI)
    /// </summary>
    public string? ExtractTenantSlug(HttpContext context)
    {
        // 1. Cabecera de desarrollo / test
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerSlug) && !string.IsNullOrWhiteSpace(headerSlug))
        {
            return headerSlug.ToString().Trim().ToLowerInvariant();
        }

        // 2. Subdominio en Host
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
                var parts = prefix.Split('.');
                return parts[^1].ToLowerInvariant();
            }
        }

        // 3. Parámetro de consulta '?tenant=' para pruebas / enlaces directos
        if (context.Request.Query.TryGetValue("tenant", out var querySlug) && !string.IsNullOrWhiteSpace(querySlug))
        {
            return querySlug.ToString().Trim().ToLowerInvariant();
        }

        return null;
    }
}
