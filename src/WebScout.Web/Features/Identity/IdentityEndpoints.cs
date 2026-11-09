using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
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
                return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
            }

            return Results.Redirect($"/login?error=invalid_credentials&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }).DisableAntiforgery(); // Permite envío desde formulario estándar de Blazor SSR

        // Inicio de sesión rápido para pruebas de desarrollo
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

            return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        });

        // Cierre de sesión seguro
        group.MapGet("/logout", async (
            SignInManager<UsuarioScout> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/");
        });

        return app;
    }
}
