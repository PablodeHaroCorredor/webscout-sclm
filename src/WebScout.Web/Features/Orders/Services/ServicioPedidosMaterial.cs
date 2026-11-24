using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Orders.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Features.Orders.Services;

public class ServicioPedidosMaterial : IServicioPedidosMaterial
{
    private readonly WebScoutDbContext _dbContext;
    private readonly IServicioAuditoria _auditoria;
    private readonly IWebHostEnvironment? _environment;
    private readonly ILogger<ServicioPedidosMaterial> _logger;

    public ServicioPedidosMaterial(
        WebScoutDbContext dbContext,
        IServicioAuditoria auditoria,
        ILogger<ServicioPedidosMaterial> logger,
        IWebHostEnvironment? environment = null)
    {
        _dbContext = dbContext;
        _auditoria = auditoria;
        _logger = logger;
        _environment = environment;
    }

    public async Task<List<ProductoMaterial>> ObtenerCatalogoAsync(Guid organizacionId, bool soloActivos = true, CancellationToken ct = default)
    {
        var query = _dbContext.ProductosMaterial
            .Where(p => p.OrganizacionId == organizacionId);

        if (soloActivos)
        {
            query = query.Where(p => p.Activo);
        }

        var productos = await query
            .OrderBy(p => p.OrdenVisualizacion)
            .ThenBy(p => p.Nombre)
            .ToListAsync(ct);

        // Si el catálogo está vacío para esta organización, inicializamos el catálogo scout por defecto
        if (productos.Count == 0)
        {
            await SembrarCatalogoInicialAsync(organizacionId, ct);
            productos = await query
                .OrderBy(p => p.OrdenVisualizacion)
                .ThenBy(p => p.Nombre)
                .ToListAsync(ct);
        }

        return productos;
    }

    public async Task<ProductoMaterial?> ObtenerProductoPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.ProductosMaterial.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<ProductoMaterial> GuardarProductoAsync(ProductoMaterial producto, string usuario, CancellationToken ct = default)
    {
        var existente = await _dbContext.ProductosMaterial.FirstOrDefaultAsync(p => p.Id == producto.Id, ct);
        if (existente == null)
        {
            _dbContext.ProductosMaterial.Add(producto);
            await _dbContext.SaveChangesAsync(ct);

            await _auditoria.RegistrarAsync(
                "CREAR_PRODUCTO_CATALOGO",
                "ProductoMaterial",
                producto.Id.ToString(),
                $"Artículo '{producto.Nombre}' añadido al catálogo por {usuario} con precio {producto.PrecioUnitario:F2} €",
                ct);

            return producto;
        }

        existente.Nombre = producto.Nombre;
        existente.Descripcion = producto.Descripcion;
        existente.Categoria = producto.Categoria;
        existente.ImagenUrl = producto.ImagenUrl;
        existente.PrecioUnitario = producto.PrecioUnitario;
        existente.TallasDisponiblesCsv = producto.TallasDisponiblesCsv;
        existente.Activo = producto.Activo;
        existente.OrdenVisualizacion = producto.OrdenVisualizacion;

        await _dbContext.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(
            "EDITAR_PRODUCTO_CATALOGO",
            "ProductoMaterial",
            existente.Id.ToString(),
            $"Artículo '{existente.Nombre}' actualizado por {usuario}. Nuevo precio unitario: {existente.PrecioUnitario:F2} €",
            ct);

        return existente;
    }

    public async Task<List<PedidoMaterial>> ObtenerPedidosFamiliaAsync(Guid organizacionId, Guid usuarioId, CancellationToken ct = default)
    {
        return await _dbContext.PedidosMaterial
            .Include(p => p.Lineas)
            .Where(p => p.OrganizacionId == organizacionId && p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaSolicitudUtc)
            .ToListAsync(ct);
    }

    public async Task<PedidoMaterial?> ObtenerPedidoPorIdAsync(Guid pedidoId, CancellationToken ct = default)
    {
        return await _dbContext.PedidosMaterial
            .Include(p => p.Lineas)
            .FirstOrDefaultAsync(p => p.Id == pedidoId, ct);
    }

    public async Task<PedidoMaterial> CrearPedidoAsync(Guid organizacionId, Guid usuarioId, CrearPedidoDto dto, string usuario, CancellationToken ct = default)
    {
        if (dto.Lineas == null || dto.Lineas.Count == 0)
        {
            throw new ArgumentException("El pedido debe contener al menos un artículo.");
        }

        foreach (var lineaDto in dto.Lineas)
        {
            if (lineaDto.Cantidad < 1 || lineaDto.Cantidad > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(dto.Lineas), "La cantidad por artículo debe situarse entre 1 y 10 unidades (RF49).");
            }
        }

        // Obtener datos del solicitante
        var solicitante = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        var solicitanteNombre = solicitante?.NombreCompleto ?? usuario;
        var solicitanteEmail = solicitante?.Email ?? usuario;
        var solicitanteTelefono = solicitante?.PhoneNumber;

        // Obtener datos del educando si aplica
        string? educandoNombre = null;
        string? ramaEducativa = null;
        if (dto.EducandoId.HasValue)
        {
            var educando = await _dbContext.Educandos.FirstOrDefaultAsync(e => e.Id == dto.EducandoId.Value, ct);
            if (educando != null)
            {
                educandoNombre = $"{educando.Nombre} {educando.Apellidos}".Trim();
                ramaEducativa = educando.RamaActual.ToString();
            }
        }

        // Cargar productos para tomar instantánea de precios
        var productosIds = dto.Lineas.Select(l => l.ProductoId).Distinct().ToList();
        var productos = await _dbContext.ProductosMaterial
            .Where(p => productosIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        // Generar código único correlativo para el año en curso
        var anio = DateTime.UtcNow.Year;
        var totalExistentesAnio = await _dbContext.PedidosMaterial
            .CountAsync(p => p.OrganizacionId == organizacionId && p.FechaSolicitudUtc.Year == anio, ct);
        var codigoPedido = $"PED-{anio}-{(totalExistentesAnio + 1):D4}";

        var pedido = new PedidoMaterial
        {
            OrganizacionId = organizacionId,
            CodigoPedido = codigoPedido,
            UsuarioId = usuarioId,
            SolicitanteNombre = solicitanteNombre,
            SolicitanteEmail = solicitanteEmail,
            SolicitanteTelefono = solicitanteTelefono,
            EducandoId = dto.EducandoId,
            EducandoNombre = educandoNombre,
            RamaEducativa = ramaEducativa,
            Estado = EstadoPedidoMaterial.Solicitado,
            ObservacionesFamilia = dto.Observaciones,
            FechaSolicitudUtc = DateTime.UtcNow,
            ModificadoPor = usuario
        };

        decimal total = 0m;
        foreach (var lineaDto in dto.Lineas)
        {
            if (!productos.TryGetValue(lineaDto.ProductoId, out var producto))
            {
                throw new InvalidOperationException($"El artículo seleccionado con ID '{lineaDto.ProductoId}' ya no existe en el catálogo.");
            }

            var linea = new LineaPedidoMaterial
            {
                OrganizacionId = organizacionId,
                PedidoMaterialId = pedido.Id,
                ProductoMaterialId = producto.Id,
                ProductoNombre = producto.Nombre,
                Talla = string.IsNullOrWhiteSpace(lineaDto.Talla) ? "Talla Única" : lineaDto.Talla.Trim(),
                Cantidad = lineaDto.Cantidad,
                PrecioUnitario = producto.PrecioUnitario
            };

            total += linea.Subtotal;
            pedido.Lineas.Add(linea);
        }

        pedido.TotalEstimado = total;

        _dbContext.PedidosMaterial.Add(pedido);
        await _dbContext.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(
            "CREAR_PEDIDO_MATERIAL",
            "PedidoMaterial",
            pedido.Id.ToString(),
            $"Nuevo pedido '{pedido.CodigoPedido}' registrado por '{solicitanteNombre}' ({pedido.Lineas.Count} artículos, total {pedido.TotalEstimado:F2} €). Estado: Solicitado.",
            ct);

        _logger.LogInformation("Pedido {CodigoPedido} creado para la organización {OrgId} con importe {Total} €",
            pedido.CodigoPedido, organizacionId, pedido.TotalEstimado);

        return pedido;
    }

    public async Task CancelarPedidoFamiliaAsync(Guid pedidoId, Guid usuarioId, string motivo, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("Es obligatorio indicar un motivo breve para cancelar la solicitud (RF52).", nameof(motivo));
        }

        var pedido = await _dbContext.PedidosMaterial
            .FirstOrDefaultAsync(p => p.Id == pedidoId, ct);

        if (pedido == null)
        {
            throw new InvalidOperationException($"No se encontró el pedido con identificador '{pedidoId}'.");
        }

        // Verificar pertenencia o rol administrativo
        if (pedido.UsuarioId != usuarioId && !_dbContext.Users.Any(u => u.Id == usuarioId))
        {
            throw new UnauthorizedAccessException("No tienes autorización para cancelar este pedido.");
        }

        // Regla RF52: Solo cancelable en Solicitado o Revisado
        if (!pedido.PermiteCancelacionFamilia)
        {
            throw new InvalidOperationException(
                $"No es posible cancelar el pedido directamente porque se encuentra en estado '{pedido.Estado}'. Por favor, contacta con el scouter de material.");
        }

        var estadoAnterior = pedido.Estado;
        pedido.Estado = EstadoPedidoMaterial.Cancelado;
        pedido.MotivoEstado = motivo.Trim();
        pedido.ModificadoPor = usuario;
        pedido.FechaUltimoCambioEstadoUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(
            "CANCELAR_PEDIDO_FAMILIA",
            "PedidoMaterial",
            pedido.Id.ToString(),
            $"Pedido '{pedido.CodigoPedido}' cancelado por el solicitante. Estado previo: {estadoAnterior}. Motivo: '{motivo}'.",
            ct);
    }

    public async Task AdjuntarComprobantePagoAsync(Guid pedidoId, string nombreArchivo, Stream stream, string usuario, CancellationToken ct = default)
    {
        var pedido = await _dbContext.PedidosMaterial
            .FirstOrDefaultAsync(p => p.Id == pedidoId, ct);

        if (pedido == null)
        {
            throw new InvalidOperationException($"No se encontró el pedido con identificador '{pedidoId}'.");
        }

        if (!pedido.PermiteAdjuntarComprobante)
        {
            throw new InvalidOperationException($"No se puede adjuntar comprobante a un pedido en estado '{pedido.Estado}'.");
        }

        // Crear directorio seguro para justificantes
        var uploadsFolder = Path.Combine(
            _environment?.ContentRootPath ?? Directory.GetCurrentDirectory(),
            "uploads",
            "comprobantes-material",
            pedido.OrganizacionId.ToString());

        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(nombreArchivo);
        var nombreSanitizado = $"{pedido.CodigoPedido}_{Guid.NewGuid():N}{extension}";
        var rutaDestino = Path.Combine(uploadsFolder, nombreSanitizado);

        await using (var fileStream = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write))
        {
            await stream.CopyToAsync(fileStream, ct);
        }

        pedido.ComprobantePagoRuta = rutaDestino;
        pedido.ComprobantePagoNombreOriginal = nombreArchivo;
        pedido.ComprobantePagoFechaUtc = DateTime.UtcNow;
        pedido.ModificadoPor = usuario;

        // IMPORTANTE (PA07 / RF51): Subir el justificante NO altera el estado del pedido a ningún flujo rígido
        await _dbContext.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(
            "ADJUNTAR_COMPROBANTE_PAGO",
            "PedidoMaterial",
            pedido.Id.ToString(),
            $"Comprobante de pago voluntario adjuntado ('{nombreArchivo}') para el pedido '{pedido.CodigoPedido}' por {usuario}.",
            ct);
    }

    public async Task<List<PedidoMaterial>> ObtenerPedidosKraalAsync(Guid organizacionId, FiltroPedidosKraalDto? filtro = null, CancellationToken ct = default)
    {
        var query = _dbContext.PedidosMaterial
            .Include(p => p.Lineas)
            .Where(p => p.OrganizacionId == organizacionId);

        if (filtro != null)
        {
            if (filtro.Estado.HasValue)
            {
                query = query.Where(p => p.Estado == filtro.Estado.Value);
            }

            if (filtro.ProductoId.HasValue)
            {
                query = query.Where(p => p.Lineas.Any(l => l.ProductoMaterialId == filtro.ProductoId.Value));
            }

            if (!string.IsNullOrWhiteSpace(filtro.TerminoBusqueda))
            {
                var term = filtro.TerminoBusqueda.Trim().ToLower();
                query = query.Where(p =>
                    p.CodigoPedido.ToLower().Contains(term) ||
                    p.SolicitanteNombre.ToLower().Contains(term) ||
                    (p.EducandoNombre != null && p.EducandoNombre.ToLower().Contains(term)) ||
                    p.SolicitanteEmail.ToLower().Contains(term));
            }

            if (filtro.Desde.HasValue)
            {
                query = query.Where(p => p.FechaSolicitudUtc >= filtro.Desde.Value);
            }

            if (filtro.Hasta.HasValue)
            {
                query = query.Where(p => p.FechaSolicitudUtc <= filtro.Hasta.Value);
            }
        }

        return await query
            .OrderByDescending(p => p.FechaSolicitudUtc)
            .ToListAsync(ct);
    }

    public async Task<ResumenPedidosKraalDto> ObtenerResumenKraalAsync(Guid organizacionId, CancellationToken ct = default)
    {
        var lista = await _dbContext.PedidosMaterial
            .Where(p => p.OrganizacionId == organizacionId)
            .Select(p => p.Estado)
            .ToListAsync(ct);

        var total = lista.Count;
        var solicitados = lista.Count(e => e == EstadoPedidoMaterial.Solicitado);
        var revisados = lista.Count(e => e == EstadoPedidoMaterial.Revisado);
        var proveedor = lista.Count(e => e == EstadoPedidoMaterial.PedidoAlProveedor);
        var entregados = lista.Count(e => e == EstadoPedidoMaterial.Entregado);
        var incidencias = lista.Count(e => e == EstadoPedidoMaterial.Rechazado || e == EstadoPedidoMaterial.Cancelado);

        return new ResumenPedidosKraalDto(total, solicitados, revisados, proveedor, entregados, incidencias);
    }

    public async Task CambiarEstadoPedidoAsync(Guid pedidoId, EstadoPedidoMaterial nuevoEstado, string? motivo, string usuario, CancellationToken ct = default)
    {
        var pedido = await _dbContext.PedidosMaterial
            .FirstOrDefaultAsync(p => p.Id == pedidoId, ct);

        if (pedido == null)
        {
            throw new InvalidOperationException($"No se encontró el pedido con identificador '{pedidoId}'.");
        }

        var estadoAnterior = pedido.Estado;

        // Principios PA07 y PA08: El scouter / responsable de material tiene potestad completa
        // para marcar 'Entregado' sin justificante bancario, y para rectificar administrativamente
        // entre cualquiera de los 6 estados (incluso reabrir pedidos entregados o cancelados).
        pedido.Estado = nuevoEstado;
        pedido.MotivoEstado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        pedido.ModificadoPor = usuario;
        pedido.FechaUltimoCambioEstadoUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        await _auditoria.RegistrarAsync(
            "CAMBIO_ESTADO_PEDIDO_MATERIAL",
            "PedidoMaterial",
            pedido.Id.ToString(),
            $"Estado del pedido '{pedido.CodigoPedido}' modificado de '{estadoAnterior}' a '{nuevoEstado}' por '{usuario}'. Motivo/Aclaración: '{motivo ?? "Sin observaciones adicionales"}'.",
            ct);

        _logger.LogInformation("Pedido {CodigoPedido} cambió de {EstadoAnt} a {NuevoEstado} por {Usuario}",
            pedido.CodigoPedido, estadoAnterior, nuevoEstado, usuario);
    }

    public async Task<(byte[] CsvBytes, string NombreArchivo)> GenerarCsvAprovisionamientoAsync(Guid organizacionId, List<Guid>? pedidosIds = null, CancellationToken ct = default)
    {
        var query = _dbContext.PedidosMaterial
            .Include(p => p.Lineas)
            .Where(p => p.OrganizacionId == organizacionId);

        if (pedidosIds != null && pedidosIds.Count > 0)
        {
            query = query.Where(p => pedidosIds.Contains(p.Id));
        }
        else
        {
            // Por defecto, se incluyen pedidos activos que requieren aprovisionamiento (Solicitados, Revisados o En Proveedor)
            query = query.Where(p =>
                p.Estado == EstadoPedidoMaterial.Solicitado ||
                p.Estado == EstadoPedidoMaterial.Revisado ||
                p.Estado == EstadoPedidoMaterial.PedidoAlProveedor);
        }

        var pedidos = await query
            .OrderBy(p => p.FechaSolicitudUtc)
            .ToListAsync(ct);

        // 1. Agrupación para compras (Producto + Talla => Unidades)
        var resumenAgrupado = pedidos
            .SelectMany(p => p.Lineas)
            .GroupBy(l => new { l.ProductoNombre, Talla = l.Talla ?? "Talla Única" })
            .Select(g => new ResumenAprovisionamientoItemDto(
                g.Key.ProductoNombre,
                g.Key.Talla,
                g.Sum(l => l.Cantidad),
                g.Sum(l => l.Subtotal)))
            .OrderBy(r => r.ProductoNombre)
            .ThenBy(r => r.Talla)
            .ToList();

        var sb = new StringBuilder();

        // Encabezado del informe
        sb.AppendLine("INFORME DE APROVISIONAMIENTO Y PEDIDOS DE MATERIAL SCOUT");
        sb.AppendLine($"Fecha de Generación:;{DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Total Pedidos Seleccionados:;{pedidos.Count}");
        sb.AppendLine();

        // SECCIÓN 1: Resumen conjunto agrupado para proveedores
        sb.AppendLine("=== 1. RESUMEN DE COMPRA CONJUNTA A PROVEEDOR ===");
        sb.AppendLine("Artículo;Talla;Unidades Totales;Importe Estimado (€)");

        foreach (var item in resumenAgrupado)
        {
            sb.AppendLine($"\"{item.ProductoNombre}\";\"{item.Talla}\";{item.CantidadTotal};{item.TotalEstimado:F2}");
        }

        sb.AppendLine();
        sb.AppendLine($"TOTAL UNIDADES:;{resumenAgrupado.Sum(r => r.CantidadTotal)}");
        sb.AppendLine($"TOTAL ESTIMADO:;{resumenAgrupado.Sum(r => r.TotalEstimado):F2} €");
        sb.AppendLine();

        // SECCIÓN 2: Detalle por pedido y familia
        sb.AppendLine("=== 2. DETALLE DE SOLICITUDES INDIVIDUALES ===");
        sb.AppendLine("Código Pedido;Fecha Solicitud;Solicitante;Email;Teléfono;Educando;Rama;Artículo;Talla;Cantidad;Subtotal (€);Estado;Tiene Justificante");

        foreach (var p in pedidos)
        {
            foreach (var l in p.Lineas)
            {
                sb.AppendLine($"\"{p.CodigoPedido}\";\"{p.FechaSolicitudUtc:yyyy-MM-dd}\";\"{p.SolicitanteNombre}\";\"{p.SolicitanteEmail}\";\"{p.SolicitanteTelefono ?? ""}\";\"{p.EducandoNombre ?? ""}\";\"{p.RamaEducativa ?? ""}\";\"{l.ProductoNombre}\";\"{l.Talla ?? "Talla Única"}\";{l.Cantidad};{l.Subtotal:F2};\"{p.Estado}\";\"{(p.TieneComprobante ? "SÍ" : "NO")}\"");
            }
        }

        // Codificación UTF-8 con BOM para que Microsoft Excel en español reconozca caracteres y tildes automáticamente
        var utf8Bom = new UTF8Encoding(true);
        var bytes = utf8Bom.GetBytes(sb.ToString());
        var nombreArchivo = $"Aprovisionamiento_MaterialScout_{DateTime.Now:yyyyMMdd_HHmm}.csv";

        return (bytes, nombreArchivo);
    }

    private async Task SembrarCatalogoInicialAsync(Guid organizacionId, CancellationToken ct)
    {
        var articulos = new List<ProductoMaterial>
        {
            new()
            {
                OrganizacionId = organizacionId,
                Nombre = "Pañoleta Oficial Grupo Scout Lestonnac 330",
                Descripcion = "Colores asociativos verde bosque y ribete negro. Símbolo de pertenencia y compromiso con el escultismo.",
                Categoria = "Insignias y Pañoletas",
                PrecioUnitario = 7.50m,
                TallasDisponiblesCsv = null, // Talla única
                Activo = true,
                OrdenVisualizacion = 1,
                ImagenUrl = "/images/material/panoleta.jpg"
            },
            new()
            {
                OrganizacionId = organizacionId,
                Nombre = "Polo Scout Amarillo - Castores y Lobatos",
                Descripcion = "Polo oficial 100% algodón pique con bordado oficial de Scouts de Castilla-La Mancha para las secciones menores.",
                Categoria = "Uniformidad",
                PrecioUnitario = 14.00m,
                TallasDisponiblesCsv = "6-8, 10-12, 14, XS, S, M",
                Activo = true,
                OrdenVisualizacion = 2,
                ImagenUrl = "/images/material/polo-amarillo.jpg"
            },
            new()
            {
                OrganizacionId = organizacionId,
                Nombre = "Polo Scout Azul Marino - Tropa Brownsea",
                Descripcion = "Polo técnico y resistente para actividades de aire libre, marchas y campamentos de la sección Scout.",
                Categoria = "Uniformidad",
                PrecioUnitario = 15.00m,
                TallasDisponiblesCsv = "10-12, 14, XS, S, M, L, XL, XXL",
                Activo = true,
                OrdenVisualizacion = 3,
                ImagenUrl = "/images/material/polo-azul.jpg"
            },
            new()
            {
                OrganizacionId = organizacionId,
                Nombre = "Camisa Scout Oficial SCLM",
                Descripcion = "Camisa clásica con bolsillos de solapa, presillas para hombreras y botones reforzados para ceremonias y eventos federativos.",
                Categoria = "Uniformidad",
                PrecioUnitario = 22.00m,
                TallasDisponiblesCsv = "10-12, 14, S, M, L, XL, XXL",
                Activo = true,
                OrdenVisualizacion = 4,
                ImagenUrl = "/images/material/camisa-scout.jpg"
            },
            new()
            {
                OrganizacionId = organizacionId,
                Nombre = "Camiseta Oficial Campamento de Verano",
                Descripcion = "Camiseta transpirable de algodón hilado en anillo para grandes actividades, raids y veladas de campamento.",
                Categoria = "Campamento",
                PrecioUnitario = 9.00m,
                TallasDisponiblesCsv = "6-8, 10-12, 14, S, M, L, XL, XXL",
                Activo = true,
                OrdenVisualizacion = 5,
                ImagenUrl = "/images/material/camiseta-campamento.jpg"
            },
            new()
            {
                OrganizacionId = organizacionId,
                Nombre = "Cantimplora Scout de Aluminio 1L",
                Descripcion = "Cantimplora clásica con funda de fieltro aislante y mosquetón de anclaje para mochila de marcha.",
                Categoria = "Campamento",
                PrecioUnitario = 8.50m,
                TallasDisponiblesCsv = null, // Talla única
                Activo = true,
                OrdenVisualizacion = 6,
                ImagenUrl = "/images/material/cantimplora.jpg"
            },
            new()
            {
                OrganizacionId = organizacionId,
                Nombre = "Cuaderno y Libro de Progresión Educativa",
                Descripcion = "Guía de pruebas, etapas de progresión personal y especialidades scouts adaptada a la rama correspondiente.",
                Categoria = "Material Pedagógico",
                PrecioUnitario = 6.00m,
                TallasDisponiblesCsv = null, // Talla única
                Activo = true,
                OrdenVisualizacion = 7,
                ImagenUrl = "/images/material/libro-progresion.jpg"
            }
        };

        _dbContext.ProductosMaterial.AddRange(articulos);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Catálogo scout inicial sembrado con éxito para la organización {OrgId}", organizacionId);
    }
}
