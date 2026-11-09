using WebScout.Web.Infrastructure.Outbox.Domain;

namespace WebScout.Web.Infrastructure.Outbox.Services;

/// <summary>
/// Contrato del servicio Outbox transaccional para persistencia y despacho desacoplado de notificaciones.
/// </summary>
public interface IServicioOutbox
{
    /// <summary>
    /// Encola un mensaje en la tabla Outbox. Si ya existe un mensaje con la misma clave de deduplicación
    /// en la misma organización, se reutiliza el existente garantizando idempotencia (PA11).
    /// </summary>
    Task<MensajeOutbox> EncolarMensajeAsync(
        Guid organizacionId,
        string tipoEvento,
        string destinatarioEmail,
        string destinatarioNombre,
        string asunto,
        string cuerpoHtml,
        string cuerpoTextoPlano,
        string? claveDeduplicacion = null,
        string? replyToEmail = null,
        string? remitenteNombre = null,
        bool guardarInmediatamente = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Procesa un lote de mensajes pendientes o en reintento, aplicando reintentos exponenciales con jitter.
    /// </summary>
    /// <returns>Número de mensajes procesados con éxito.</returns>
    Task<int> ProcesarLotePendienteAsync(int tamanoLote = 20, CancellationToken cancellationToken = default);
}
