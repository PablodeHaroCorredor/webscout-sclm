using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Identity;

/// <summary>
/// Enriquecedor de claims de usuario de Identity.
/// Inyecta OrganizacionId, NombreCompleto y RamaAsignada en la cookie de sesión.
/// </summary>
public class ScoutUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<UsuarioScout, RolScout>
{
    public ScoutUserClaimsPrincipalFactory(
        UserManager<UsuarioScout> userManager,
        RoleManager<RolScout> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(UsuarioScout user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim("OrganizacionId", user.OrganizacionId.ToString()));
        identity.AddClaim(new Claim("NombreCompleto", user.NombreCompleto ?? string.Empty));
        identity.AddClaim(new Claim("RamaAsignada", user.RamaAsignada.ToString()));

        return identity;
    }
}
