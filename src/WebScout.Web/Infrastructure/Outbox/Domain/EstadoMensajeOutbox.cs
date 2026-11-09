namespace WebScout.Web.Infrastructure.Outbox.Domain;

/// <summary>
/// Estados del ciclo de vida de un mensaje en el buzón de salida transaccional (Outbox).
/// </summary>
public enum EstadoMensajeOutbox
{
    /// <summary>
    /// Mensaje persistido en la base de datos pendiente de ser recogido por el worker.
    /// </summary>
    Pendiente = 0,

    /// <summary>
    /// Mensaje bloqueado y en proceso de envío activo por un worker.
    /// </summary>
    EnProceso = 1,

    /// <summary>
    /// Mensaje entregado satisfactoriamente al servidor SMTP / transporte de correo.
    /// </summary>
    Enviado = 2,

    /// <summary>
    /// Mensaje que ha superado el número máximo de reintentos con error permanente.
    /// Requiere intervención o reintento manual desde el panel de administración.
    /// </summary>
    Fallido = 3,

    /// <summary>
    /// Mensaje descartado explícitamente (por ejemplo, cancelado por supersesión).
    /// </summary>
    Descartado = 4
}
