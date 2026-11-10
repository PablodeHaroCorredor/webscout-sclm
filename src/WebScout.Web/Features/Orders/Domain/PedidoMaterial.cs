using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Orders.Domain;

/// <summary>
/// Solicitud de material scout registrada por una familia o gestionada por el Kraal (RF49-RF54).
/// Cumple con las directrices Core 1.29: entrega flexible en mano sin bloqueo bancario obligatorio (PA07),
/// cancelación familiar motivada en etapas tempranas (RF52), rectificación administrativa con auditoría (PA08)
/// y adjunto voluntario de justificante de transferencia (RF51).
/// </summary>
public class PedidoMaterial : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant / Grupo scout al que pertenece el pedido.
    /// </summary>
    public Guid OrganizacionId { get; set; }

    /// <summary>
    /// Código identificativo legible y único para la familia y el kraal (ej. 'PED-2026-0001').
    /// </summary>
    public required string CodigoPedido { get; set; }

    /// <summary>
    /// Identificador del usuario que registró el pedido (rol Familia o Scouter).
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Nombre de la persona de contacto / tutor legal solicitante.
    /// </summary>
    public required string SolicitanteNombre { get; set; }

    /// <summary>
    /// Correo electrónico de notificación.
    /// </summary>
    public required string SolicitanteEmail { get; set; }

    /// <summary>
    /// Teléfono de contacto de la familia.
    /// </summary>
    public string? SolicitanteTelefono { get; set; }

    /// <summary>
    /// Educando para quien se solicita el material (opcional).
    /// </summary>
    public Guid? EducandoId { get; set; }

    /// <summary>
    /// Nombre del educando destinatario (desnormalizado para consultas rápidas).
    /// </summary>
    public string? EducandoNombre { get; set; }

    /// <summary>
    /// Rama educativa del educando (Castores, Lobatos, Tropa, Escultas, Rovers).
    /// </summary>
    public string? RamaEducativa { get; set; }

    /// <summary>
    /// Estado actual en la máquina de 6 estados Core 1.29 (RF50).
    /// </summary>
    public EstadoPedidoMaterial Estado { get; set; } = EstadoPedidoMaterial.Solicitado;

    /// <summary>
    /// Motivo registrado en caso de cancelación, rechazo o rectificación administrativa.
    /// </summary>
    public string? MotivoEstado { get; set; }

    /// <summary>
    /// Observaciones o aclaraciones introducidas por la familia al solicitar.
    /// </summary>
    public string? ObservacionesFamilia { get; set; }

    /// <summary>
    /// Notas internas exclusivas del scouter / responsable de material.
    /// </summary>
    public string? NotasInternasKraal { get; set; }

    /// <summary>
    /// Importe total estimado del pedido en euros (suma de las líneas).
    /// </summary>
    public decimal TotalEstimado { get; set; }

    /// <summary>
    /// Fecha y hora UTC en la que se registró el pedido.
    /// </summary>
    public DateTime FechaSolicitudUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora UTC del último cambio de estado.
    /// </summary>
    public DateTime? FechaUltimoCambioEstadoUtc { get; set; }

    /// <summary>
    /// Nombre o correo de la última persona que modificó el estado o datos del pedido.
    /// </summary>
    public string? ModificadoPor { get; set; }

    /// <summary>
    /// Ruta o identificador del archivo de justificante bancario adjunto (RF51).
    /// </summary>
    public string? ComprobantePagoRuta { get; set; }

    /// <summary>
    /// Nombre original del archivo subido por la familia (ej. 'recibo_transferencia_polo.pdf').
    /// </summary>
    public string? ComprobantePagoNombreOriginal { get; set; }

    /// <summary>
    /// Fecha y hora en la que se subió o actualizó el justificante.
    /// </summary>
    public DateTime? ComprobantePagoFechaUtc { get; set; }

    /// <summary>
    /// Artículos solicitados que componen este pedido.
    /// </summary>
    public List<LineaPedidoMaterial> Lineas { get; set; } = new();

    /// <summary>
    /// Regla asociativa Core 1.29 (RF52): la familia puede cancelar el pedido por sí misma
    /// si está en estado 'Solicitado' o 'Revisado', debiendo indicar un motivo.
    /// Una vez pasa a 'PedidoAlProveedor' o 'Entregado', la cancelación directa queda bloqueada.
    /// </summary>
    public bool PermiteCancelacionFamilia =>
        Estado == EstadoPedidoMaterial.Solicitado || Estado == EstadoPedidoMaterial.Revisado;

    /// <summary>
    /// La familia puede adjuntar o actualizar su comprobante bancario en cualquier fase
    /// anterior a la entrega final (RF51).
    /// </summary>
    public bool PermiteAdjuntarComprobante =>
        Estado != EstadoPedidoMaterial.Cancelado && Estado != EstadoPedidoMaterial.Rechazado;

    /// <summary>
    /// Indica si el pedido ya cuenta con algún justificante bancario registrado.
    /// </summary>
    public bool TieneComprobante => !string.IsNullOrWhiteSpace(ComprobantePagoRuta);
}
