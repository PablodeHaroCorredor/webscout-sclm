using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Participants.Domain;

/// <summary>
/// Educando o miembro juvenil del grupo scout.
/// Es una entidad persistente a lo largo de las rondas solares,
/// vinculada permanentemente a su tutor legal (ScoutUser).
/// </summary>
public class ScoutParticipant : ITenantScopedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant / Grupo scout al que pertenece el educando (ej. Lestonnac 330).
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Identificador del tutor legal responsable (ScoutUser con rol Familia).
    /// </summary>
    public Guid LegalGuardianId { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public DateOnly BirthDate { get; set; }

    public string? Dni { get; set; }

    public string? Gender { get; set; }

    /// <summary>
    /// Rama o sección pedagógica actual del educando.
    /// </summary>
    public ScoutBranch CurrentBranch { get; set; } = ScoutBranch.None;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    // Propiedades de navegación
    public ScoutUser? LegalGuardian { get; set; }

    public ParticipantHealthRecord? HealthRecord { get; set; }

    // Helpers de dominio
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Calcula la edad del educando a una fecha de referencia determinada (por defecto hoy).
    /// </summary>
    public int CalculateAge(DateOnly? referenceDate = null)
    {
        var target = referenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var age = target.Year - BirthDate.Year;
        if (BirthDate > target.AddYears(-age))
        {
            age--;
        }
        return age;
    }
}
