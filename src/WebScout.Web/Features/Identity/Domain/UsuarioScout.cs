using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Participants.Domain;

namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Usuario autenticado en WebScout SCLM.
/// Todo usuario pertenece a una organización o grupo scout concreto (OrganizacionId),
/// garantizando aislamiento multitenant estricto.
/// </summary>
public class UsuarioScout : IdentityUser<Guid>, IEntidadOrganizacion
{
    public Guid OrganizacionId { get; set; }

    public required string NombreCompleto { get; set; }

    public string? Dni { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Rama a la que está asignado el usuario si actúa como educador/scouter.
    /// Si es Familia, el valor es Ninguna.
    /// </summary>
    public RamaScout RamaAsignada { get; set; } = RamaScout.Ninguna;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UltimoAccesoUtc { get; set; }

    /// <summary>
    /// Educandos censados bajo la tutela de esta cuenta familiar.
    /// </summary>
    public ICollection<Educando> Educandos { get; set; } = new List<Educando>();

    // Propiedades puente / aliases retrocompatibles (NotMapped)
    [NotMapped]
    public Guid OrganizationId
    {
        get => OrganizacionId;
        set => OrganizacionId = value;
    }

    [NotMapped]
    public string FullName
    {
        get => NombreCompleto;
        set => NombreCompleto = value;
    }

    [NotMapped]
    public bool IsActive
    {
        get => Activo;
        set => Activo = value;
    }

    [NotMapped]
    public ScoutBranch AssignedBranch
    {
        get => (ScoutBranch)(int)RamaAsignada;
        set => RamaAsignada = (RamaScout)(int)value;
    }

    [NotMapped]
    public DateTime CreatedAtUtc
    {
        get => FechaCreacionUtc;
        set => FechaCreacionUtc = value;
    }

    [NotMapped]
    public DateTime? LastLoginAtUtc
    {
        get => UltimoAccesoUtc;
        set => UltimoAccesoUtc = value;
    }
}

/// <summary>
/// Alias retrocompatible de transición para UsuarioScout.
/// </summary>
public class ScoutUser : UsuarioScout
{
}
