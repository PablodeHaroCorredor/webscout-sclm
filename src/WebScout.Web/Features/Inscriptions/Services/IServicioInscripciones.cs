using WebScout.Web.Features.Inscriptions.Domain;

namespace WebScout.Web.Features.Inscriptions.Services;

/// <summary>
/// Contrato del servicio de inscripciones a actividades scout y gestión de justificantes de pago bancario.
/// </summary>
public interface IServicioInscripciones
{
    /// <summary>
    /// Formaliza la inscripción de un educando a una actividad o campamento.
    /// Valida que la convocatoria admita inscripciones, pertenencia a la rama y existencia de ficha médica.
    /// </summary>
    Task<InscripcionActividad> InscribirEducandoAsync(
        Guid actividadId,
        Guid educandoId,
        Guid tutorId,
        string? observaciones = null);

    /// <summary>
    /// Obtiene el historial de inscripciones de los educandos a cargo de un tutor legal.
    /// </summary>
    Task<IReadOnlyList<InscripcionActividad>> ObtenerInscripcionesFamiliaAsync(Guid tutorId);

    /// <summary>
    /// Obtiene todos los inscritos a una actividad para su gestión y revisión por el Kraal.
    /// </summary>
    Task<IReadOnlyList<InscripcionActividad>> ObtenerInscripcionesActividadAsync(Guid actividadId);

    /// <summary>
    /// Obtiene una inscripción por su identificador único dentro del tenant activo.
    /// </summary>
    Task<InscripcionActividad?> ObtenerPorIdAsync(Guid inscripcionId);

    /// <summary>
    /// Registra la subida del archivo de comprobante bancario por parte de la familia.
    /// Transiciona el estado a EnRevision.
    /// </summary>
    Task SubirJustificantePagoAsync(
        Guid inscripcionId,
        string nombreArchivoOriginal,
        Stream streamContenido);

    /// <summary>
    /// Permite al Scouter o Tesorero validar el justificante, confirmar la plaza o requerir subsanación.
    /// </summary>
    Task RevisarInscripcionAsync(
        Guid inscripcionId,
        EstadoInscripcion nuevoEstado,
        string? notasKraal,
        Guid scouterId);

    /// <summary>
    /// Cancela una inscripción existente a petición de la familia.
    /// </summary>
    Task CancelarInscripcionAsync(Guid inscripcionId, Guid tutorId);
}
