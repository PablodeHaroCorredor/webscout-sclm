using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;

namespace WebScout.Web.Features.Inscriptions.Domain;

/// <summary>
/// Representa la inscripción formal de un educando a una actividad o campamento scout.
/// Vincula al menor con la convocatoria, registra la autorización legal expresa del tutor
/// y gestiona la trazabilidad del justificante de pago bancario.
/// Implementa IEntidadOrganizacion para aislamiento multi-inquilino.
/// </summary>
public class InscripcionActividad : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizacionId { get; set; }

    public Guid ActividadId { get; set; }
    public ActividadScout Actividad { get; set; } = null!;

    public Guid EducandoId { get; set; }
    public Educando Educando { get; set; } = null!;

    public Guid TutorLegalId { get; set; }
    public UsuarioScout TutorLegal { get; set; } = null!;

    public EstadoInscripcion Estado { get; set; } = EstadoInscripcion.PendienteJustificante;

    public DateTime FechaInscripcionUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Consentimiento legal informado del tutor para la asistencia, traslados y atención médica urgente.
    /// </summary>
    public bool AutorizacionFirmada { get; set; } = true;

    public DateTime FechaAutorizacionUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cuota fijada en el momento de formalizar la inscripción (en euros con céntimos, ej. 28.50 €).
    /// </summary>
    public decimal ImporteEuros { get; set; } = 0.00m;

    #region Trazabilidad de Justificante de Pago (Transferencia Externa)
    /// <summary>
    /// Nombre original del archivo adjunto (ej. "justificante_alcaraz_lucia.pdf").
    /// </summary>
    public string? NombreArchivoJustificante { get; set; }

    /// <summary>
    /// Ruta interna de almacenamiento seguro del comprobante bancario.
    /// </summary>
    public string? RutaArchivoJustificante { get; set; }

    /// <summary>
    /// Fecha y hora en la que la familia subió el justificante.
    /// </summary>
    public DateTime? FechaSubidaJustificanteUtc { get; set; }
    #endregion

    /// <summary>
    /// Observaciones logísticas aportadas por la familia (ej. horario especial de llegada, vehículo, etc.).
    /// </summary>
    public string? ObservacionesFamilia { get; set; }

    /// <summary>
    /// Observaciones internas del Kraal / Scouter durante la revisión de la plaza o el justificante.
    /// </summary>
    public string? NotasKraal { get; set; }

    public Guid? RevisadoPorUsuarioId { get; set; }
    public UsuarioScout? RevisadoPorUsuario { get; set; }
    public DateTime? FechaRevisionUtc { get; set; }

    /// <summary>
    /// Indica si la inscripción tiene un comprobante de pago bancario adjunto.
    /// </summary>
    public bool TieneJustificanteSubido => !string.IsNullOrWhiteSpace(RutaArchivoJustificante);
}
