using WebScout.Web.Features.Orders.Domain;

namespace WebScout.Web.Features.Orders.Services;

public record CrearLineaPedidoDto(Guid ProductoId, string? Talla, int Cantidad);

public record CrearPedidoDto(
    Guid? EducandoId,
    string? Observaciones,
    List<CrearLineaPedidoDto> Lineas);

public record FiltroPedidosKraalDto(
    EstadoPedidoMaterial? Estado = null,
    Guid? ProductoId = null,
    string? TerminoBusqueda = null,
    DateTime? Desde = null,
    DateTime? Hasta = null);

public record ResumenPedidosKraalDto(
    int TotalPedidos,
    int Solicitados,
    int Revisados,
    int PedidosAlProveedor,
    int Entregados,
    int Incidencias);

public record ResumenAprovisionamientoItemDto(
    string ProductoNombre,
    string? Talla,
    int CantidadTotal,
    decimal TotalEstimado);

/// <summary>
/// Servicio de gestión de catálogo de uniformidad y pedidos de material scout (Core 1.29).
/// Cumple los requisitos RF48-RF54, PA07 (entrega sin bloqueo) y PA08 (rectificación administrativa con auditoría).
/// </summary>
public interface IServicioPedidosMaterial
{
    Task<List<ProductoMaterial>> ObtenerCatalogoAsync(Guid organizacionId, bool soloActivos = true, CancellationToken ct = default);
    Task<ProductoMaterial?> ObtenerProductoPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductoMaterial> GuardarProductoAsync(ProductoMaterial producto, string usuario, CancellationToken ct = default);

    Task<List<PedidoMaterial>> ObtenerPedidosFamiliaAsync(Guid organizacionId, Guid usuarioId, CancellationToken ct = default);
    Task<PedidoMaterial?> ObtenerPedidoPorIdAsync(Guid pedidoId, CancellationToken ct = default);
    Task<PedidoMaterial> CrearPedidoAsync(Guid organizacionId, Guid usuarioId, CrearPedidoDto dto, string usuario, CancellationToken ct = default);
    Task CancelarPedidoFamiliaAsync(Guid pedidoId, Guid usuarioId, string motivo, string usuario, CancellationToken ct = default);

    Task AdjuntarComprobantePagoAsync(Guid pedidoId, string nombreArchivo, Stream stream, string usuario, CancellationToken ct = default);

    Task<List<PedidoMaterial>> ObtenerPedidosKraalAsync(Guid organizacionId, FiltroPedidosKraalDto? filtro = null, CancellationToken ct = default);
    Task<ResumenPedidosKraalDto> ObtenerResumenKraalAsync(Guid organizacionId, CancellationToken ct = default);
    Task CambiarEstadoPedidoAsync(Guid pedidoId, EstadoPedidoMaterial nuevoEstado, string? motivo, string usuario, CancellationToken ct = default);

    Task<(byte[] CsvBytes, string NombreArchivo)> GenerarCsvAprovisionamientoAsync(Guid organizacionId, List<Guid>? pedidosIds = null, CancellationToken ct = default);
    Task ImportarPedidoHistorico128Async(
        Guid organizacionId,
        string codigo,
        string solicitante,
        string email,
        string estado128,
        string producto,
        string? talla,
        int cantidad,
        decimal precioUnitario,
        string? notas,
        DateTime fecha,
        CancellationToken ct = default);
}
