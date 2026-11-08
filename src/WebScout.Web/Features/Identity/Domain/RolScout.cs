using Microsoft.AspNetCore.Identity;

namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Entidad de rol para ASP.NET Core Identity.
/// </summary>
public class RolScout : IdentityRole<Guid>
{
    public RolScout() : base()
    {
    }

    public RolScout(string roleName) : base(roleName)
    {
    }
}

/// <summary>
/// Alias retrocompatible de transición para RolScout.
/// </summary>
public class ScoutRole : RolScout
{
    public ScoutRole() : base() { }
    public ScoutRole(string roleName) : base(roleName) { }
}
