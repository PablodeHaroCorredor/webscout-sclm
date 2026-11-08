using Microsoft.AspNetCore.Identity;
using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Usuario autenticado en WebScout SCLM.
/// Todo usuario pertenece a una organización o grupo scout concreto (OrganizationId),
/// garantizando aislamiento multitenant estricto.
/// </summary>
public class ScoutUser : IdentityUser<Guid>, ITenantScopedEntity
{
    public Guid OrganizationId { get; set; }

    public required string FullName { get; set; }

    public string? Dni { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Rama a la que está asignado el usuario si actúa como educador/scouter.
    /// Si es Familia, el valor suele ser None.
    /// </summary>
    public ScoutBranch AssignedBranch { get; set; } = ScoutBranch.None;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; set; }
}
