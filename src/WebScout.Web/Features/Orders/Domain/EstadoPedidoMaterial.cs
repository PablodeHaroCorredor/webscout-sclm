namespace WebScout.Web.Features.Orders.Domain;

/// <summary>
/// Máquina de 6 estados del flujo de pedidos de material scout del pedido sencillo.
/// Sustituye la rigidez bancaria comercial por una gestión ágil y asociativa (PA07, PA08).
/// </summary>
public enum EstadoPedidoMaterial
{
    /// <summary>
    /// La familia ha registrado la petición de material en el portal.
    /// </summary>
    Solicitado = 1,

    /// <summary>
    /// El responsable de material ha comprobado el stock o confirmado la necesidad de encargo.
    /// </summary>
    Revisado = 2,

    /// <summary>
    /// El lote de material ha sido encargado a la tienda scout federativa o fabricante textil.
    /// </summary>
    PedidoAlProveedor = 3,

    /// <summary>
    /// El material ha sido entregado en mano al educando o a sus tutores en la base scout (PA07).
    /// No requiere bloqueo informático por conciliación bancaria previa.
    /// </summary>
    Entregado = 4,

    /// <summary>
    /// Petición denegada con motivo registrado (ej. fuera de plazo, talla descatalogada).
    /// </summary>
    Rechazado = 5,

    /// <summary>
    /// Solicitud desistida por la familia o anulada administrativamente (RF52).
    /// </summary>
    Cancelado = 6
}
