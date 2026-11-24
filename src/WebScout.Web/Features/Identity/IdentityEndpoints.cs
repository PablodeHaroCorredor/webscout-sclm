using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        // Inicio de sesión convencional con formulario
        group.MapPost("/login", async (
            [FromForm] string email,
            [FromForm] string password,
            [FromForm] string? returnUrl,
            SignInManager<UsuarioScout> signInManager,
            UserManager<UsuarioScout> userManager,
            ITenantContext tenantContext) =>
        {
            var normalizedEmail = email.ToUpperInvariant();
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user == null || !user.Activo)
            {
                return Results.Redirect($"/login?error=invalid_credentials&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

            var esSuperAdmin = await userManager.IsInRoleAsync(user, RolesScout.SuperAdmin);
            if (!esSuperAdmin && tenantContext.CurrentTenantId != Guid.Empty && user.OrganizacionId != tenantContext.CurrentTenantId)
            {
                return Results.Redirect($"/login?error=wrong_tenant&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

            var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                user.UltimoAccesoUtc = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
                var destino = await CalcularRutaDestinoPorRolAsync(userManager, user, returnUrl);
                return Results.Redirect(destino);
            }

            return Results.Redirect($"/login?error=invalid_credentials&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }).DisableAntiforgery(); // Permite envío desde formulario estándar de Blazor SSR

        // El acceso rápido sin contraseña solo existe en Development: en cualquier otro
        // entorno la ruta no se registra (404) y no puede usarse para suplantar cuentas.
        var entorno = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (entorno.IsDevelopment())
        {
            MapAccesoRapidoDesarrollo(group);
        }

        // Cierre de sesión seguro
        group.MapGet("/logout", async (
            SignInManager<UsuarioScout> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/");
        });

        return app;
    }

    private static void MapAccesoRapidoDesarrollo(RouteGroupBuilder group)
    {
        group.MapGet("/quick-login", async (
            [FromQuery] string email,
            [FromQuery] string? returnUrl,
            SignInManager<UsuarioScout> signInManager,
            UserManager<UsuarioScout> userManager,
            ITenantContext tenantContext) =>
        {
            var normalizedEmail = email.ToUpperInvariant();
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user == null || !user.Activo)
            {
                return Results.NotFound($"Usuario '{email}' no encontrado o inactivo.");
            }

            var esSuperAdmin = await userManager.IsInRoleAsync(user, RolesScout.SuperAdmin);
            if (!esSuperAdmin && tenantContext.CurrentTenantId != Guid.Empty && user.OrganizacionId != tenantContext.CurrentTenantId)
            {
                return Results.Redirect($"/login?error=wrong_tenant&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

            await signInManager.SignInAsync(user, isPersistent: true);
            user.UltimoAccesoUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            var destino = await CalcularRutaDestinoPorRolAsync(userManager, user, returnUrl);
            return Results.Redirect(destino);
        });
    }

    /// <summary>
    /// Solo acepta rutas relativas de la propia aplicación ("/familias"), nunca URLs absolutas
    /// ni rutas que el navegador interpreta como otro host ("//evil.example", "/\evil.example").
    /// Evita que un enlace de login manipulado redirija a un sitio externo tras autenticarse.
    /// </summary>
    public static bool EsRutaLocalSegura(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url[0] != '/')
        {
            return false;
        }

        return url.Length == 1 || (url[1] != '/' && url[1] != '\\');
    }

    private static async Task<string> CalcularRutaDestinoPorRolAsync(
        UserManager<UsuarioScout> userManager,
        UsuarioScout user,
        string? returnUrl)
    {
        if (EsRutaLocalSegura(returnUrl) && returnUrl != "/")
        {
            return returnUrl!;
        }

        if (await userManager.IsInRoleAsync(user, RolesScout.Familia))
        {
            return "/familias";
        }

        if (await userManager.IsInRoleAsync(user, RolesScout.Scouter))
        {
            return "/kraal/inscripciones";
        }

        if (await userManager.IsInRoleAsync(user, RolesScout.Coordinacion) || await userManager.IsInRoleAsync(user, RolesScout.Administracion))
        {
            return "/kraal/actividades";
        }

        if (await userManager.IsInRoleAsync(user, RolesScout.SuperAdmin))
        {
            return "/admin/grupos";
        }

        return "/";
    }
}
