namespace WebScout.Web.Features.Admissions.Domain;

/// <summary>
/// Estados de tramitación de una solicitud de admisión de un nuevo educando en el grupo scout.
/// </summary>
public enum EstadoSolicitudAdmision
{
    /// <summary>
    /// Solicitud recién recibida pendiente de revisión inicial por secretaría.
    /// </summary>
    Pendiente = 0,

    /// <summary>
    /// Solicitud válida pero sin plaza vacante actual en la rama correspondiente; en lista de espera ordenada.
    /// </summary>
    EnListaEspera = 1,

    /// <summary>
    /// Solicitud aceptada formalmente para incorporarse al censo del grupo scout.
    /// </summary>
    Admitida = 2,

    /// <summary>
    /// Solicitud desestimada (por ejemplo, fuera de rango de edad o renuncia de la familia).
    /// </summary>
    Rechazada = 3,

    /// <summary>
    /// Solicitud archivada o resuelta históricamente.
    /// </summary>
    Archivada = 4
}
