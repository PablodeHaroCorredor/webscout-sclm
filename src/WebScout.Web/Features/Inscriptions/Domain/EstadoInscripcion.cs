namespace WebScout.Web.Features.Inscriptions.Domain;

/// <summary>
/// Ciclo de vida y estado de tramitación de la inscripción de un educando a una actividad.
/// </summary>
public enum EstadoInscripcion
{
    /// <summary>
    /// La familia ha inscrito al menor pero aún debe realizar la transferencia y adjuntar el justificante.
    /// (Solo aplica en actividades con cuota > 0).
    /// </summary>
    PendienteJustificante = 1,

    /// <summary>
    /// La familia ha subido el justificante bancario de pago y está en revisión por el Kraal / Tesorería.
    /// </summary>
    EnRevision = 2,

    /// <summary>
    /// Plaza confirmada definitivamente (automático si es gratuita, o tras validación del justificante).
    /// </summary>
    Confirmada = 3,

    /// <summary>
    /// El justificante no es válido o el importe es incorrecto; se solicita a la familia que lo resuba.
    /// </summary>
    RequiereSubsanacion = 4,

    /// <summary>
    /// Inscripción anulada por la familia antes de la salida o cancelada por la coordinación.
    /// </summary>
    Cancelada = 5
}

public static class EstadoInscripcionExtensions
{
    public static string ObtenerNombre(this EstadoInscripcion estado) => estado switch
    {
        EstadoInscripcion.PendienteJustificante => "Pendiente de Justificante",
        EstadoInscripcion.EnRevision => "En Revisión",
        EstadoInscripcion.Confirmada => "Confirmada",
        EstadoInscripcion.RequiereSubsanacion => "Requiere Subsanación",
        EstadoInscripcion.Cancelada => "Cancelada",
        _ => estado.ToString()
    };
}
