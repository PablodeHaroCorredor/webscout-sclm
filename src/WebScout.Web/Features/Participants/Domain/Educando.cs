using System.ComponentModel.DataAnnotations.Schema;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Participants.Domain;

/// <summary>
/// Educando o miembro juvenil del grupo scout.
/// Es una entidad persistente a lo largo de las rondas solares,
/// vinculada permanentemente a su tutor legal (UsuarioScout).
/// </summary>
public class Educando : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant / Grupo scout al que pertenece el educando (ej. Lestonnac 330).
    /// </summary>
    public Guid OrganizacionId { get; set; }

    /// <summary>
    /// Identificador del tutor legal responsable (UsuarioScout con rol Familia).
    /// </summary>
    public Guid TutorLegalId { get; set; }

    public required string Nombre { get; set; }

    public required string Apellidos { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public string? Dni { get; set; }

    public string? Sexo { get; set; }

    /// <summary>
    /// Rama o sección pedagógica actual del educando (Castores, Manada, Tropa...).
    /// </summary>
    public RamaScout RamaActual { get; set; } = RamaScout.Ninguna;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacionUtc { get; set; }

    // Propiedades de navegación
    public UsuarioScout? TutorLegal { get; set; }

    public FichaMedica? FichaMedica { get; set; }

    // Helpers de dominio en castellano
    public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();

    /// <summary>
    /// Calcula la edad del educando a una fecha de referencia determinada (por defecto hoy).
    /// </summary>
    public int CalcularEdad(DateOnly? fechaReferencia = null)
    {
        var target = fechaReferencia ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var edad = target.Year - FechaNacimiento.Year;
        if (FechaNacimiento > target.AddYears(-edad))
        {
            edad--;
        }
        return edad;
    }

    // Puentes y propiedades retrocompatibles de transición (NotMapped)
    [NotMapped]
    public Guid OrganizationId
    {
        get => OrganizacionId;
        set => OrganizacionId = value;
    }

    [NotMapped]
    public Guid LegalGuardianId
    {
        get => TutorLegalId;
        set => TutorLegalId = value;
    }

    [NotMapped]
    public string FirstName
    {
        get => Nombre;
        set => Nombre = value;
    }

    [NotMapped]
    public string LastName
    {
        get => Apellidos;
        set => Apellidos = value;
    }

    [NotMapped]
    public DateOnly BirthDate
    {
        get => FechaNacimiento;
        set => FechaNacimiento = value;
    }

    [NotMapped]
    public string? Gender
    {
        get => Sexo;
        set => Sexo = value;
    }

    [NotMapped]
    public ScoutBranch CurrentBranch
    {
        get => (ScoutBranch)(int)RamaActual;
        set => RamaActual = (RamaScout)(int)value;
    }

    [NotMapped]
    public bool IsActive
    {
        get => Activo;
        set => Activo = value;
    }

    [NotMapped]
    public DateTime CreatedAtUtc
    {
        get => FechaCreacionUtc;
        set => FechaCreacionUtc = value;
    }

    [NotMapped]
    public DateTime? UpdatedAtUtc
    {
        get => FechaActualizacionUtc;
        set => FechaActualizacionUtc = value;
    }

    [NotMapped]
    public string FullName => NombreCompleto;

    public int CalculateAge(DateOnly? referenceDate = null) => CalcularEdad(referenceDate);

    [NotMapped]
    public UsuarioScout? LegalGuardian
    {
        get => TutorLegal;
        set => TutorLegal = value;
    }

    [NotMapped]
    public FichaMedica? HealthRecord
    {
        get => FichaMedica;
        set => FichaMedica = value;
    }
}

/// <summary>
/// Alias retrocompatible de transición para Educando.
/// </summary>
public class ScoutParticipant : Educando
{
}
