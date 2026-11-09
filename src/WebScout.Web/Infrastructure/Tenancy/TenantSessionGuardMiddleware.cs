using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Middleware de seguridad que valida que un usuario autenticado mediante cookie
/// pertenezca estrictamente al tenant (subdominio) sobre el que está realizando peticiones HTTP.
/// Previene vulnerabilidades de confusión de sesión cross-tenant (BOLA / Broken Tenant Isolation).
/// El rol SuperAdmin está exento de esta restricción para permitir administración federativa.
/// </summary>
public class TenantSessionGuardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantSessionGuardMiddleware> _logger;

    public TenantSessionGuardMiddleware(
        RequestDelegate next,
        ILogger<TenantSessionGuardMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var user = context.User;

        // Si el usuario está autenticado y NO es SuperAdmin
        if (user?.Identity?.IsAuthenticated == true && !tenantContext.IsSuperAdmin)
        {
            var userOrgClaim = user.FindFirst("OrganizacionId")?.Value;
            if (Guid.TryParse(userOrgClaim, out var userOrgId) && userOrgId != Guid.Empty)
            {
                // Si la petición se dirige a un subdominio de grupo específico
                if (tenantContext.CurrentTenantId != Guid.Empty && tenantContext.CurrentTenantId != userOrgId)
                {
                    _logger.LogWarning(
                        "ALERTA DE SEGURIDAD CROSS-TENANT: El usuario '{UserId}' (OrganizacionId: {UserOrgId}) " +
                        "intentó acceder al tenant '{TargetTenantSlug}' (OrganizacionId: {TargetTenantId}) en la ruta '{Path}'. Acceso bloqueado.",
                        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity.Name,
                        userOrgId,
                        tenantContext.TenantSlug,
                        tenantContext.CurrentTenantId,
                        context.Request.Path);

                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        await context.Response.WriteAsync("{\"error\": \"Acceso denegado: Su cuenta pertenece a otro grupo scout.\"}");
                        return;
                    }

                    // Para peticiones web de navegador, redirigir al login informando del error
                    context.Response.Redirect($"/login?error=wrong_tenant&returnUrl={Uri.EscapeDataString(context.Request.Path)}");
                    return;
                }
            }
        }

        await _next(context);
    }
}
