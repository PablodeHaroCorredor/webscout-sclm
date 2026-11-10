using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Orders.Domain;

/// <summary>
/// Línea de artículo dentro de una solicitud de material (RF49).
/// Guarda una instantánea inmutable del nombre del producto y del precio unitario en el
/// momento de cursar el pedido, garantizando que futuras ediciones del catálogo no alteren el histórico.
/// </summary>
public class LineaPedidoMaterial : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant / Grupo scout (aislamiento multitenant).
    /// </summary>
    public Guid OrganizacionId { get; set; }

    /// <summary>
    /// Pedido contenedor al que pertenece esta línea.
    /// </summary>
    public Guid PedidoMaterialId { get; set; }
    public PedidoMaterial? Pedido { get; set; }

    /// <summary>
    /// Producto del catálogo al que hace referencia (puede ser nulo en pedidos históricos migrados del Core 1.28).
    /// </summary>
    public Guid? ProductoMaterialId { get; set; }
    public ProductoMaterial? Producto { get; set; }

    /// <summary>
    /// Instantánea inmutable del nombre del producto.
    /// </summary>
    public required string ProductoNombre { get; set; }

    /// <summary>
    /// Talla seleccionada por la familia (ej. '10-12', 'M') o 'Talla Única'.
    /// </summary>
    public string? Talla { get; set; }

    /// <summary>
    /// Cantidad solicitada (limitada entre 1 y 10 unidades por el Core 1.29).
    /// </summary>
    public int Cantidad { get; set; } = 1;

    /// <summary>
    /// Instantánea inmutable del precio unitario en el momento del encargo.
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Subtotal de la línea (Cantidad * PrecioUnitario).
    /// </summary>
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
