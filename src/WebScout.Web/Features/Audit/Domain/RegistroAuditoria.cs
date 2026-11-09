using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Audit.Domain;

/// <summary>
/// Registro inmutable de auditoría para trazabilidad de acceso a datos médicos,
/// descarga de justificantes y operaciones de seguridad (RGPD / PA06 / RF40).
/// </summary>
public class RegistroAuditoria : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Organización / Grupo scout al que pertenece el evento auditado.
    /// </summary>
    public Guid OrganizacionId { get; set; }

    /// <summary>
    /// Identificador del usuario que ejecutó la acción (si estaba autenticado).
    /// </summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>
    /// Email o nombre del actor al momento de la acción.
    /// </summary>
    public string? EmailUsuario { get; set; }

    /// <summary>
    /// Acción realizada: DescargaJustificante, ConsultaFichaMedica, ExportacionDossierAsistenciaCsv, etc.
    /// </summary>
    public required string Accion { get; set; }

    /// <summary>
    /// Tipo de entidad o recurso accedido (ej. "InscripcionActividad", "FichaMedica", "ActividadScout").
    /// </summary>
    public required string TipoRecurso { get; set; }

    /// <summary>
    /// Identificador del recurso accedido.
    /// </summary>
    public string? RecursoId { get; set; }

    /// <summary>
    /// Metadatos o detalles complementarios de la operación.
    /// </summary>
    public string? Detalles { get; set; }

    /// <summary>
    /// Dirección IP del cliente en la solicitud.
    /// </summary>
    public string? DireccionIp { get; set; }

    /// <summary>
    /// Timestamp UTC inmutable del evento.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
}
