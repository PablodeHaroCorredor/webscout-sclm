using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Participants.Domain;

/// <summary>
/// Expediente sanitario y médico anual del educando.
/// Centraliza alergias, intolerancias, medicación activa, CIP de salud
/// y contactos de emergencia para campamentos y salidas scouts.
/// </summary>
public class ParticipantHealthRecord : ITenantScopedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant / Grupo scout al que pertenece el expediente.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Identificador del educando al que corresponde la ficha médica.
    /// Relación 1 a 1 estricta.
    /// </summary>
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// Código de Identificación Personal (CIP) / Nº Seguridad Social / Póliza de seguro.
    /// </summary>
    public required string HealthInsuranceNumber { get; set; }

    /// <summary>
    /// Entidad aseguradora (ej. SESCAM, Sanitas, Adeslas, etc.).
    /// </summary>
    public string? HealthInsuranceEntity { get; set; }

    /// <summary>
    /// Alergias alimentarias diagnosticadas (cacahuetes, huevo, marisco, frutos secos, etc.).
    /// </summary>
    public string? FoodAllergies { get; set; }

    /// <summary>
    /// Alergias a medicamentos (penicilina, AINEs, etc.).
    /// </summary>
    public string? MedicalAllergies { get; set; }

    /// <summary>
    /// Alergias ambientales / picaduras (polen, ácaros, avispas, etc.).
    /// </summary>
    public string? EnvironmentalAllergies { get; set; }

    /// <summary>
    /// Régimen dietético e intolerancias (celiaquía, intolerancia a la lactosa, vegetariano, exclusión cerdo, etc.).
    /// </summary>
    public string? DietaryRequirements { get; set; }

    /// <summary>
    /// Medicación activa y pautas de administración para campamentos (posología, horarios, instrucciones).
    /// </summary>
    public string? ActiveMedication { get; set; }

    /// <summary>
    /// Enfermedades crónicas o patologías relevantes (asma, epilepsia, diabetes, etc.).
    /// </summary>
    public string? ChronicConditions { get; set; }

    /// <summary>
    /// Declaración responsable: Calendario oficial de vacunación al día.
    /// </summary>
    public bool VaccinationsUpToDate { get; set; } = true;

    /// <summary>
    /// Observaciones relativas a vacunas (ej. antitetánica actualizada).
    /// </summary>
    public string? VaccinationObservations { get; set; }

    /// <summary>
    /// Capacidad natatoria: ¿Sabe nadar con soltura en actividades acuáticas?
    /// </summary>
    public bool CanSwim { get; set; } = true;

    /// <summary>
    /// Teléfono de emergencia prioritario (Tutor 1).
    /// </summary>
    public required string PrimaryEmergencyPhone { get; set; }

    /// <summary>
    /// Nombre de la persona de contacto prioritario (ej. "María García (Madre)").
    /// </summary>
    public string? PrimaryEmergencyContactName { get; set; }

    /// <summary>
    /// Teléfono de emergencia secundario o de reserva.
    /// </summary>
    public string? SecondaryEmergencyPhone { get; set; }

    /// <summary>
    /// Nombre del contacto secundario (ej. "Carlos García (Padre)").
    /// </summary>
    public string? SecondaryEmergencyContactName { get; set; }

    /// <summary>
    /// Observaciones adicionales relevantes para el equipo de scouters y botiquín de campamento.
    /// </summary>
    public string? AdditionalObservations { get; set; }

    /// <summary>
    /// Fecha y hora UTC en la que la familia actualizó o confirmó por última vez esta ficha.
    /// </summary>
    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identificador del usuario que realizó la última actualización.
    /// </summary>
    public Guid? LastUpdatedById { get; set; }

    // Propiedad de navegación
    public ScoutParticipant? Participant { get; set; }
}
