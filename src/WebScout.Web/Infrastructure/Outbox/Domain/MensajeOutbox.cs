using WebScout.Web.Common.Domain;

namespace WebScout.Web.Infrastructure.Outbox.Domain;

/// <summary>
/// Representa un mensaje o notificación persistido en el buzón transaccional de salida (Outbox).
/// Garantiza entrega fiable 'at-least-once' y resistencia ante caídas de proveedores de correo o SMTP (ADR07 / PA11).
/// </summary>
public class MensajeOutbox : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador del grupo scout propietario de la comunicación (aislamiento multitenant estricto).
    /// </summary>
    public Guid OrganizacionId { get; set; }

    /// <summary>
    /// Tipo de evento de dominio que originó la notificación (ej. "InscripcionSubsanacionRequerida").
    /// </summary>
    public string TipoEvento { get; set; } = string.Empty;

    /// <summary>
    /// Clave determinista de idempotencia para evitar envíos duplicados ante reintentos (PA11).
    /// </summary>
    public string ClaveDeduplicacion { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del destinatario.
    /// </summary>
    public string DestinatarioEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible del destinatario.
    /// </summary>
    public string DestinatarioNombre { get; set; } = string.Empty;

    /// <summary>
    /// Asunto del correo, sanitizado contra inyecciones CRLF (CWE-93).
    /// </summary>
    public string Asunto { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de respuesta opcional (ej. buzón del Kraal del grupo correspondiente).
    /// </summary>
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// Nombre visible opcional del remitente (ej. "Grupo Scout Lestonnac 330").
    /// </summary>
    public string? RemitenteNombre { get; set; }

    /// <summary>
    /// Contenido renderizado en formato HTML seguro (sin datos médicos sensibles ni PII restringida, RNF09).
    /// </summary>
    public string CuerpoHtml { get; set; } = string.Empty;

    /// <summary>
    /// Alternativa en texto plano para clientes de correo sin renderizado HTML.
    /// </summary>
    public string CuerpoTextoPlano { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual del mensaje en la cola Outbox.
    /// </summary>
    public EstadoMensajeOutbox Estado { get; set; } = EstadoMensajeOutbox.Pendiente;

    /// <summary>
    /// Contador de intentos de envío efectuados.
    /// </summary>
    public int Reintentos { get; set; } = 0;

    /// <summary>
    /// Límite máximo de intentos antes de pasar a estado Fallido.
    /// </summary>
    public int MaxReintentos { get; set; } = 5;

    /// <summary>
    /// Marca temporal mínima para el siguiente reintento con backoff exponencial.
    /// </summary>
    public DateTime? ProximoReintentoUtc { get; set; }

    /// <summary>
    /// Detalle técnico del último error presentado por el servidor SMTP si falló.
    /// </summary>
    public string? ErrorUltimoReintento { get; set; }

    /// <summary>
    /// Fecha de creación del registro en UTC (dentro de la transacción de negocio).
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha en la que el mensaje fue entregado satisfactoriamente en UTC.
    /// </summary>
    public DateTime? FechaProcesadoUtc { get; set; }
}
