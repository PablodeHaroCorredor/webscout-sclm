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
                .FirstOrDefaultAsync(u => u.OrganizacionId == tenantContext.CurrentTenantId && u.NormalizedEmail == normalizedEmail);

            if (user == null || !user.Activo)
            {
                return Results.Redirect($"/login?error=invalid_credentials&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
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
                .FirstOrDefaultAsync(u => u.OrganizacionId == tenantContext.CurrentTenantId && u.NormalizedEmail == normalizedEmail);

            if (user == null)
            {
                // Si no se encuentra en el tenant activo, buscar en el tenant correspondiente al email
                user = await userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
            }

            if (user != null && user.Activo)
            {
                await signInManager.SignInAsync(user, isPersistent: true);
                user.UltimoAccesoUtc = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
            }

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
