using System.ComponentModel.DataAnnotations.Schema;
using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Participants.Domain;

/// <summary>
/// Expediente sanitario y médico anual del educando.
/// Centraliza alergias, intolerancias, medicación activa, CIP de salud
/// y contactos de emergencia para campamentos y salidas scouts.
/// </summary>
public class FichaMedica : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant / Grupo scout al que pertenece el expediente.
    /// </summary>
    public Guid OrganizacionId { get; set; }

    /// <summary>
    /// Identificador del educando al que corresponde la ficha médica (Relación 1 a 1).
    /// </summary>
    public Guid EducandoId { get; set; }

    /// <summary>
    /// Código de Identificación Personal (CIP) / Nº Seguridad Social / Póliza de seguro.
    /// </summary>
    public required string NumeroSeguridadSocial { get; set; }

    /// <summary>
    /// Entidad aseguradora (ej. SESCAM, Sanitas, Adeslas, etc.).
    /// </summary>
    public string? EntidadAseguradora { get; set; }

    /// <summary>
    /// Alergias alimentarias diagnosticadas (cacahuetes, huevo, marisco, frutos secos, etc.).
    /// </summary>
    public string? AlergiasAlimentarias { get; set; }

    /// <summary>
    /// Alergias a medicamentos (penicilina, AINEs, etc.).
    /// </summary>
    public string? AlergiasMedicamentosas { get; set; }

    /// <summary>
    /// Alergias ambientales / picaduras (polen, ácaros, avispas, etc.).
    /// </summary>
    public string? AlergiasAmbientales { get; set; }

    /// <summary>
    /// Régimen dietético e intolerancias (celiaquía, intolerancia a la lactosa, vegetariano, exclusión cerdo, etc.).
    /// </summary>
    public string? RegimenAlimenticio { get; set; }

    /// <summary>
    /// Medicación activa y pautas de administración para campamentos (posología, horarios, instrucciones).
    /// </summary>
    public string? MedicacionActiva { get; set; }

    /// <summary>
    /// Enfermedades crónicas o patologías relevantes (asma, epilepsia, diabetes, etc.).
    /// </summary>
    public string? EnfermedadesCronicas { get; set; }

    /// <summary>
    /// Declaración responsable: Calendario oficial de vacunación al día.
    /// </summary>
    public bool VacunacionAlDia { get; set; } = true;

    /// <summary>
    /// Observaciones relativas a vacunas (ej. antitetánica actualizada).
    /// </summary>
    public string? ObservacionesVacunacion { get; set; }

    /// <summary>
    /// Capacidad natatoria: ¿Sabe nadar con soltura en actividades acuáticas?
    /// </summary>
    public bool SabeNadar { get; set; } = true;

    /// <summary>
    /// Teléfono de emergencia prioritario (Tutor 1).
    /// </summary>
    public required string TelefonoEmergenciaPrincipal { get; set; }

    /// <summary>
    /// Nombre de la persona de contacto prioritario (ej. "María García (Madre)").
    /// </summary>
    public string? NombreContactoEmergenciaPrincipal { get; set; }

    /// <summary>
    /// Teléfono de emergencia secundario o de reserva.
    /// </summary>
    public string? TelefonoEmergenciaSecundario { get; set; }

    /// <summary>
    /// Nombre del contacto secundario (ej. "Carlos García (Padre)").
    /// </summary>
    public string? NombreContactoEmergenciaSecundario { get; set; }

    /// <summary>
    /// Observaciones adicionales relevantes para el equipo de scouters y botiquín de campamento.
    /// </summary>
    public string? ObservacionesAdicionales { get; set; }

    /// <summary>
    /// Fecha y hora UTC en la que la familia actualizó o confirmó por última vez esta ficha.
    /// </summary>
    public DateTime UltimaActualizacionUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identificador del usuario que realizó la última actualización.
    /// </summary>
    public Guid? ActualizadoPorId { get; set; }

    // Propiedad de navegación
    public Educando? Educando { get; set; }

    /// <summary>
    /// Alertas sanitarias relevantes (alergias de todo tipo, enfermedades crónicas, medicación y dietas),
    /// ignorando respuestas negativas escritas como texto ("Sin alergias conocidas", "Ninguna", etc.).
    /// </summary>
    public IReadOnlyList<AlertaSanitaria> ObtenerAlertasSanitarias() => AlertasSanitarias.Obtener(
        AlergiasMedicamentosas,
        AlergiasAlimentarias,
        AlergiasAmbientales,
        EnfermedadesCronicas,
        MedicacionActiva,
        RegimenAlimenticio);

    // Puentes y propiedades retrocompatibles de transición (NotMapped)
    [NotMapped]
    public Guid OrganizationId
    {
        get => OrganizacionId;
        set => OrganizacionId = value;
    }

    [NotMapped]
    public Guid ParticipantId
    {
        get => EducandoId;
        set => EducandoId = value;
    }

    [NotMapped]
    public string HealthInsuranceNumber
    {
        get => NumeroSeguridadSocial;
        set => NumeroSeguridadSocial = value;
    }

    [NotMapped]
    public string? HealthInsuranceEntity
    {
        get => EntidadAseguradora;
        set => EntidadAseguradora = value;
    }

    [NotMapped]
    public string? FoodAllergies
    {
        get => AlergiasAlimentarias;
        set => AlergiasAlimentarias = value;
    }

    [NotMapped]
    public string? MedicalAllergies
    {
        get => AlergiasMedicamentosas;
        set => AlergiasMedicamentosas = value;
    }

    [NotMapped]
    public string? EnvironmentalAllergies
    {
        get => AlergiasAmbientales;
        set => AlergiasAmbientales = value;
    }

    [NotMapped]
    public string? DietaryRequirements
    {
        get => RegimenAlimenticio;
        set => RegimenAlimenticio = value;
    }

    [NotMapped]
    public string? ActiveMedication
    {
        get => MedicacionActiva;
        set => MedicacionActiva = value;
    }

    [NotMapped]
    public string? ChronicConditions
    {
        get => EnfermedadesCronicas;
        set => EnfermedadesCronicas = value;
    }

    [NotMapped]
    public bool VaccinationsUpToDate
    {
        get => VacunacionAlDia;
        set => VacunacionAlDia = value;
    }

    [NotMapped]
    public string? VaccinationObservations
    {
        get => ObservacionesVacunacion;
        set => ObservacionesVacunacion = value;
    }

    [NotMapped]
    public bool CanSwim
    {
        get => SabeNadar;
        set => SabeNadar = value;
    }

    [NotMapped]
    public string PrimaryEmergencyPhone
    {
        get => TelefonoEmergenciaPrincipal;
        set => TelefonoEmergenciaPrincipal = value;
    }

    [NotMapped]
    public string? PrimaryEmergencyContactName
    {
        get => NombreContactoEmergenciaPrincipal;
        set => NombreContactoEmergenciaPrincipal = value;
    }

    [NotMapped]
    public string? SecondaryEmergencyPhone
    {
        get => TelefonoEmergenciaSecundario;
        set => TelefonoEmergenciaSecundario = value;
    }

    [NotMapped]
    public string? SecondaryEmergencyContactName
    {
        get => NombreContactoEmergenciaSecundario;
        set => NombreContactoEmergenciaSecundario = value;
    }

    [NotMapped]
    public string? AdditionalObservations
    {
        get => ObservacionesAdicionales;
        set => ObservacionesAdicionales = value;
    }

    [NotMapped]
    public DateTime LastUpdatedAtUtc
    {
        get => UltimaActualizacionUtc;
        set => UltimaActualizacionUtc = value;
    }

    [NotMapped]
    public Guid? LastUpdatedById
    {
        get => ActualizadoPorId;
        set => ActualizadoPorId = value;
    }

    [NotMapped]
    public Educando? Participant
    {
        get => Educando;
        set => Educando = value;
    }
}

/// <summary>
/// Alias retrocompatible de transición para FichaMedica.
/// </summary>
public class ParticipantHealthRecord : FichaMedica
{
}
