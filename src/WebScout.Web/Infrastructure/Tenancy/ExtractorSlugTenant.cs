using Microsoft.AspNetCore.Http;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Extrae el slug del grupo scout de una petición HTTP. Lo comparten el middleware de resolución
/// (peticiones HTTP) y <see cref="TenantContext"/> (circuito interactivo de Blazor) para que ambos
/// apliquen exactamente el mismo orden de precedencia.
/// </summary>
public static class ExtractorSlugTenant
{
    public const string DominioBasePorDefecto = "scoutsclm.org";

    /// <summary>
    /// Orden de precedencia:
    /// 1. Cabecera HTTP <c>X-Tenant-Slug</c> (pruebas e integraciones).
    /// 2. Subdominio en el Host (<c>{slug}.localhost</c> o <c>{slug}.{dominioBase}</c>).
    /// 3. Parámetro de consulta <c>?tenant=</c> (pruebas o desarrollo sin selector en UI).
    /// Devuelve <c>null</c> para el dominio raíz y para <c>www</c>.
    /// </summary>
    public static string? Extraer(HttpRequest request, string? dominioBase = null)
    {
        var baseDomain = string.IsNullOrWhiteSpace(dominioBase) ? DominioBasePorDefecto : dominioBase;

        if (request.Headers.TryGetValue("X-Tenant-Slug", out var headerSlug) && !string.IsNullOrWhiteSpace(headerSlug))
        {
            return headerSlug.ToString().Trim().ToLowerInvariant();
        }

        var host = request.Host.Host;

        if (host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
        {
            var parts = host.Split('.');
            if (parts.Length > 1 && !parts[0].Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                return parts[0].ToLowerInvariant();
            }
        }

        if (host.EndsWith(baseDomain, StringComparison.OrdinalIgnoreCase))
        {
            var prefix = host.Substring(0, host.Length - baseDomain.Length).TrimEnd('.');
            if (!string.IsNullOrWhiteSpace(prefix) && !prefix.Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                var parts = prefix.Split('.');
                return parts[^1].ToLowerInvariant();
            }
        }

        if (request.Query.TryGetValue("tenant", out var querySlug) && !string.IsNullOrWhiteSpace(querySlug))
        {
            return querySlug.ToString().Trim().ToLowerInvariant();
        }

        return null;
    }
}
