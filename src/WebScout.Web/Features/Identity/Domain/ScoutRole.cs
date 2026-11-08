using Microsoft.AspNetCore.Identity;

namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Rol de usuario en la plataforma WebScout SCLM.
/// </summary>
public class ScoutRole : IdentityRole<Guid>
{
    public ScoutRole() : base() { }

    public ScoutRole(string roleName) : base(roleName)
    {
        Id = Guid.NewGuid();
    }
}
