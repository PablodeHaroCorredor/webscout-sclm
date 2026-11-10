using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Orders.Domain;

/// <summary>
/// Artículo disponible en el catálogo de material y uniformidad del grupo scout (RF48).
/// Mantiene inmutabilidad de precios históricos: las modificaciones de catálogo no
/// alteran pedidos registrados con anterioridad.
/// </summary>
public class ProductoMaterial : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant / Grupo scout al que pertenece el catálogo (aislamiento multitenant estricto).
    /// </summary>
    public Guid OrganizacionId { get; set; }

    /// <summary>
    /// Nombre visible del artículo (ej. 'Polo Scout Amarillo', 'Pañoleta Lestonnac 330').
    /// </summary>
    public required string Nombre { get; set; }

    /// <summary>
    /// Descripción pedagógica o de uso asociativo.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Categoría para agrupación (ej. 'Uniformidad', 'Insignias y Pañoletas', 'Acampada').
    /// </summary>
    public string? Categoria { get; set; }

    /// <summary>
    /// URL o ruta relativa de la imagen ilustrativa.
    /// </summary>
    public string? ImagenUrl { get; set; }

    /// <summary>
    /// Precio unitario orientativo para las familias en euros.
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Lista de tallas disponibles separadas por comas (ej. '6-8, 10-12, 14, S, M, L, XL').
    /// Si es nulo o vacío, se trata de un artículo de talla única (ej. pañoleta, cantimplora).
    /// </summary>
    public string? TallasDisponiblesCsv { get; set; }

    /// <summary>
    /// Indica si el producto requiere obligatoriamente seleccionar talla al pedir.
    /// </summary>
    public bool RequiereTalla => !string.IsNullOrWhiteSpace(TallasDisponiblesCsv);

    /// <summary>
    /// Indica si el producto está visible en el catálogo de familias para nuevos pedidos.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Orden de prioridad visual en la cuadrícula del catálogo.
    /// </summary>
    public int OrdenVisualizacion { get; set; }

    /// <summary>
    /// Fecha de alta en el catálogo.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Devuelve las tallas configuradas en una lista limpia y sin espacios.
    /// </summary>
    public IReadOnlyList<string> ObtenerTallas()
    {
        if (string.IsNullOrWhiteSpace(TallasDisponiblesCsv))
            return Array.Empty<string>();

        return TallasDisponiblesCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
