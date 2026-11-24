using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Orders.Domain;
using WebScout.Web.Features.Orders.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class OrderMaterialServiceTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public OrderMaterialServiceTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();

        _options = _baseDatos.Opciones;

        var initialTenant = new TenantContext();
        initialTenant.SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);

        using var context = new WebScoutDbContext(_options, initialTenant);

        // Seed Tenants
        context.GruposScout.Add(new GrupoScout
        {
            Id = OrgLestonnac,
            Nombre = "Grupo Scout Lestonnac 330",
            Slug = "lestonnac",
            NumeroGrupo = 330,
            EmailContacto = "coordinacion@lestonnac.org",
            Activo = true,
            Paleta = PaletteConfig.DefaultLestonnac
        });

        context.GruposScout.Add(new GrupoScout
        {
            Id = OrgSanVicente,
            Nombre = "Grupo Scout San Vicente 572",
            Slug = "sanvicente",
            NumeroGrupo = 572,
            EmailContacto = "secretaria@sanvicente572.org",
            Activo = true,
            Paleta = PaletteConfig.DefaultLestonnac
        });

        context.SaveChanges();
    }

    private (WebScoutDbContext Db, TenantContext Tenant, IServicioPedidosMaterial Service, IServicioAuditoria Audit) CreateServices(Guid? tenantId = null)
    {
        var activeTenantId = tenantId ?? OrgLestonnac;
        var slug = activeTenantId == OrgLestonnac ? "lestonnac" : "sanvicente";
        var nombre = activeTenantId == OrgLestonnac ? "Grupo Scout Lestonnac 330" : "Grupo Scout San Vicente 572";
        var numero = activeTenantId == OrgLestonnac ? 330 : 572;

        var tenant = new TenantContext();
        tenant.SetTenant(activeTenantId, slug, nombre, numero);

        var db = new WebScoutDbContext(_options, tenant);
        var audit = new ServicioAuditoria(db, tenant, new HttpContextAccessor(), NullLogger<ServicioAuditoria>.Instance);
        var service = new ServicioPedidosMaterial(db, audit, NullLogger<ServicioPedidosMaterial>.Instance);

        return (db, tenant, service, audit);
    }

    [Fact]
    public async Task CrearPedido_ConCantidadesValidas_CreaPedidoYPreservaInmutabilidadDePrecios()
    {
        // Arrange
        var (db, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var poloAmarillo = catalogo.First(p => p.Nombre.Contains("Polo Scout Amarillo"));

        var usuarioId = Guid.NewGuid();
        db.Users.Add(new UsuarioScout
        {
            Id = usuarioId,
            UserName = "familia.perez@example.com",
            Email = "familia.perez@example.com",
            NombreCompleto = "Familia Pérez Gómez",
            PhoneNumber = "600112233",
            OrganizacionId = OrgLestonnac
        });
        await db.SaveChangesAsync();

        var dto = new CrearPedidoDto(
            EducandoId: null,
            Observaciones: "Entregar en la base scout",
            Lineas: new List<CrearLineaPedidoDto>
            {
                new(poloAmarillo.Id, "10-12", 2)
            });

        // Act
        var pedido = await service.CrearPedidoAsync(OrgLestonnac, usuarioId, dto, "familia.perez@example.com");

        // Alterar precio en catálogo posteriormente (ej. sube de 14.00 a 18.00 €)
        poloAmarillo.PrecioUnitario = 18.00m;
        await service.GuardarProductoAsync(poloAmarillo, "Scouter Material");

        // Assert
        Assert.NotNull(pedido);
        Assert.StartsWith("PED-", pedido.CodigoPedido);
        Assert.Equal(EstadoPedidoMaterial.Solicitado, pedido.Estado);
        Assert.Equal(28.00m, pedido.TotalEstimado); // 2 * 14.00 € original
        Assert.Single(pedido.Lineas);
        Assert.Equal("10-12", pedido.Lineas[0].Talla);
        Assert.Equal(14.00m, pedido.Lineas[0].PrecioUnitario); // Inmutable
        Assert.Equal(28.00m, pedido.Lineas[0].Subtotal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public async Task CrearPedido_ConCantidadInvalida_LanzaExcepcion(int cantidadInvalida)
    {
        // Arrange
        var (_, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var producto = catalogo.First();

        var dto = new CrearPedidoDto(
            EducandoId: null,
            Observaciones: null,
            Lineas: new List<CrearLineaPedidoDto>
            {
                new(producto.Id, "Talla Única", cantidadInvalida)
            });

        // Act & Assert (RF49: límite 1 a 10 unidades)
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CrearPedidoAsync(OrgLestonnac, Guid.NewGuid(), dto, "usuario@test.com"));
    }

    [Fact]
    public async Task CancelarPedidoFamilia_EnEstadoSolicitadoORevisado_PermiteCancelacionConMotivo()
    {
        // Arrange
        var (_, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var panoleta = catalogo.First(p => p.Nombre.Contains("Pañoleta"));

        var usuarioId = Guid.NewGuid();
        var dto = new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(panoleta.Id, null, 1) });
        var pedido = await service.CrearPedidoAsync(OrgLestonnac, usuarioId, dto, "familia@test.com");

        // Act (RF52: Cancelación motivada en estado temprano)
        await service.CancelarPedidoFamiliaAsync(
            pedido.Id,
            usuarioId,
            "Ya heredamos la pañoleta de su hermano mayor",
            "familia@test.com");

        var pedidoActualizado = await service.ObtenerPedidoPorIdAsync(pedido.Id);

        // Assert
        Assert.NotNull(pedidoActualizado);
        Assert.Equal(EstadoPedidoMaterial.Cancelado, pedidoActualizado.Estado);
        Assert.Equal("Ya heredamos la pañoleta de su hermano mayor", pedidoActualizado.MotivoEstado);
    }

    [Fact]
    public async Task CancelarPedidoFamilia_EnProveedorOEntregado_LanzaExcepcion()
    {
        // Arrange
        var (_, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var producto = catalogo.First();

        var usuarioId = Guid.NewGuid();
        var dto = new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(producto.Id, null, 1) });
        var pedido = await service.CrearPedidoAsync(OrgLestonnac, usuarioId, dto, "familia@test.com");

        // El Kraal tramita el pedido con el proveedor
        await service.CambiarEstadoPedidoAsync(
            pedido.Id,
            EstadoPedidoMaterial.PedidoAlProveedor,
            "Encargo enviado a la fábrica textil",
            "Scouter Intendencia");

        // Act & Assert (RF52: Bloqueo de cancelación directa una vez encargado)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CancelarPedidoFamiliaAsync(pedido.Id, usuarioId, "Ya no lo quiero", "familia@test.com"));

        Assert.Contains("contacta con el scouter", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelarPedidoFamilia_SinMotivo_LanzaExcepcion()
    {
        // Arrange
        var (_, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var producto = catalogo.First();

        var usuarioId = Guid.NewGuid();
        var dto = new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(producto.Id, null, 1) });
        var pedido = await service.CrearPedidoAsync(OrgLestonnac, usuarioId, dto, "familia@test.com");

        // Act & Assert (RF52: Motivo obligatorio)
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CancelarPedidoFamiliaAsync(pedido.Id, usuarioId, "   ", "familia@test.com"));
    }

    [Fact]
    public async Task AdjuntarComprobantePago_GuardaArchivo_YNoAlteraEstadoDelPedido()
    {
        // Arrange
        var (_, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var producto = catalogo.First();

        var usuarioId = Guid.NewGuid();
        var dto = new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(producto.Id, null, 1) });
        var pedido = await service.CrearPedidoAsync(OrgLestonnac, usuarioId, dto, "familia@test.com");

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("FALSO_JUSTIFICANTE_PDF"));

        // Act (RF51 / PA07: Comprobante voluntario sin mutación rígida de estado)
        await service.AdjuntarComprobantePagoAsync(
            pedido.Id,
            "recibo_banco_123.pdf",
            stream,
            "familia@test.com");

        var actualizado = await service.ObtenerPedidoPorIdAsync(pedido.Id);

        // Assert
        Assert.NotNull(actualizado);
        Assert.True(actualizado.TieneComprobante);
        Assert.Equal("recibo_banco_123.pdf", actualizado.ComprobantePagoNombreOriginal);
        Assert.Equal(EstadoPedidoMaterial.Solicitado, actualizado.Estado); // Estado permanece Solicitado
    }

    [Fact]
    public async Task PrincipioPA07_EntregaEnMano_PermiteMarcarEntregadoSinJustificanteBancario()
    {
        // Arrange (PA07: Principio asociativo de entrega sin bloqueo bancario)
        var (_, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var panoleta = catalogo.First(p => p.Nombre.Contains("Pañoleta"));

        var usuarioId = Guid.NewGuid();
        var dto = new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(panoleta.Id, null, 1) });
        var pedido = await service.CrearPedidoAsync(OrgLestonnac, usuarioId, dto, "familia@test.com");

        Assert.False(pedido.TieneComprobante); // Sin justificante alguno

        // Act: Scouter entrega la pañoleta en mano en la base scout
        await service.CambiarEstadoPedidoAsync(
            pedido.Id,
            EstadoPedidoMaterial.Entregado,
            "Entregada en mano al educando tras la ceremonia de promesas",
            "Scouter Coordinador");

        var actualizado = await service.ObtenerPedidoPorIdAsync(pedido.Id);

        // Assert
        Assert.NotNull(actualizado);
        Assert.Equal(EstadoPedidoMaterial.Entregado, actualizado.Estado);
        Assert.False(actualizado.TieneComprobante);
    }

    [Fact]
    public async Task PrincipioPA08_RectificacionAdministrativa_PermiteReabrirPedidoEntregadoYAudita()
    {
        // Arrange (PA08: Rectificación administrativa libre y trazabilidad inmutable)
        var (db, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var producto = catalogo.First();

        var usuarioId = Guid.NewGuid();
        var dto = new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(producto.Id, null, 1) });
        var pedido = await service.CrearPedidoAsync(OrgLestonnac, usuarioId, dto, "familia@test.com");

        await service.CambiarEstadoPedidoAsync(pedido.Id, EstadoPedidoMaterial.Entregado, "Entregado", "Scouter A");

        // Act: Scouter rectifica el estado volviendo a 'Revisado' por corrección humana
        await service.CambiarEstadoPedidoAsync(
            pedido.Id,
            EstadoPedidoMaterial.Revisado,
            "Rectificación: se confundió el educando con otro de la misma seisenas",
            "Scouter A");

        var actualizado = await service.ObtenerPedidoPorIdAsync(pedido.Id);
        var auditorias = await db.RegistrosAuditoria
            .Where(r => r.RecursoId == pedido.Id.ToString())
            .ToListAsync();

        // Assert
        Assert.NotNull(actualizado);
        Assert.Equal(EstadoPedidoMaterial.Revisado, actualizado.Estado);
        Assert.True(auditorias.Count >= 2);
        Assert.Contains(auditorias, a => a.Accion == "CAMBIO_ESTADO_PEDIDO_MATERIAL" && a.Detalles!.Contains("Rectificación"));
    }

    [Fact]
    public async Task GenerarCsvAprovisionamiento_CalculaAgrupacionCorrectaConUtf8Bom()
    {
        // Arrange
        var (_, _, service, _) = CreateServices();
        var catalogo = await service.ObtenerCatalogoAsync(OrgLestonnac);
        var polo = catalogo.First(p => p.Nombre.Contains("Polo"));
        var panoleta = catalogo.First(p => p.Nombre.Contains("Pañoleta"));

        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();

        await service.CrearPedidoAsync(OrgLestonnac, u1, new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto>
        {
            new(polo.Id, "10-12", 3),
            new(panoleta.Id, null, 2)
        }), "user1@test.com");

        await service.CrearPedidoAsync(OrgLestonnac, u2, new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto>
        {
            new(polo.Id, "10-12", 4),
            new(polo.Id, "M", 1)
        }), "user2@test.com");

        // Act (RF53: Exportación en lote para compras a distribuidor)
        var (csvBytes, nombreArchivo) = await service.GenerarCsvAprovisionamientoAsync(OrgLestonnac);
        var contenidoCsv = Encoding.UTF8.GetString(csvBytes);

        // Assert
        Assert.NotNull(csvBytes);
        Assert.True(csvBytes.Length > 0);
        Assert.StartsWith("Aprovisionamiento_MaterialScout_", nombreArchivo);

        // Verificar agregación: 3 + 4 = 7 polos talla 10-12
        Assert.Contains("=== 1. RESUMEN DE COMPRA CONJUNTA A PROVEEDOR ===", contenidoCsv);
        Assert.Contains(";\"10-12\";7;", contenidoCsv);
        Assert.Contains(";\"M\";1;", contenidoCsv);
        Assert.Contains(";\"Talla Única\";2;", contenidoCsv);
        Assert.Contains("=== 2. DETALLE DE SOLICITUDES INDIVIDUALES ===", contenidoCsv);
    }

    [Theory]
    [InlineData("Aceptado", EstadoPedidoMaterial.Revisado)]
    [InlineData("Justificante recibido", EstadoPedidoMaterial.Revisado)]
    [InlineData("Entregado", EstadoPedidoMaterial.Entregado)]
    [InlineData("Rechazado", EstadoPedidoMaterial.Rechazado)]
    public async Task ImportarPedidoHistorico128_MapeaEstadosSegunCore129(string estadoHistorico128, EstadoPedidoMaterial estadoEsperado)
    {
        // Arrange & Act (RF54: Compatibilidad e importación con pedidos históricos 1.28)
        var (db, _, service, _) = CreateServices();
        var codigo = $"PED-HIST-{Guid.NewGuid():N}";

        await service.ImportarPedidoHistorico128Async(
            OrgLestonnac,
            codigo,
            "Carlos Navarro",
            "carlos.navarro@example.com",
            estadoHistorico128,
            "Polo Scout Oficial",
            "L",
            1,
            14.50m,
            "Pedido importado desde sistema antiguo 1.28",
            DateTime.UtcNow.AddMonths(-6));

        var importado = await db.PedidosMaterial
            .Include(p => p.Lineas)
            .FirstOrDefaultAsync(p => p.CodigoPedido == codigo);

        // Assert
        Assert.NotNull(importado);
        Assert.Equal(estadoEsperado, importado.Estado);
        Assert.Contains("Core 1.28", importado.NotasInternasKraal);
    }

    [Fact]
    public async Task AislamientoMultitenant_PedidosEntreGruposEstanCompletamenteAislados()
    {
        // Arrange
        var (_, _, serviceLestonnac, _) = CreateServices(OrgLestonnac);
        var (_, _, serviceSanVicente, _) = CreateServices(OrgSanVicente);

        var catalogoLestonnac = await serviceLestonnac.ObtenerCatalogoAsync(OrgLestonnac);
        var catalogoSanVicente = await serviceSanVicente.ObtenerCatalogoAsync(OrgSanVicente);

        var pLestonnac = catalogoLestonnac.First();
        var pSanVicente = catalogoSanVicente.First();

        var pedidoL = await serviceLestonnac.CrearPedidoAsync(
            OrgLestonnac,
            Guid.NewGuid(),
            new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(pLestonnac.Id, null, 1) }),
            "lestonnac@test.com");

        var pedidoSV = await serviceSanVicente.CrearPedidoAsync(
            OrgSanVicente,
            Guid.NewGuid(),
            new CrearPedidoDto(null, null, new List<CrearLineaPedidoDto> { new(pSanVicente.Id, null, 1) }),
            "sanvicente@test.com");

        // Act
        var pedidosL = await serviceLestonnac.ObtenerPedidosKraalAsync(OrgLestonnac);
        var pedidosSV = await serviceSanVicente.ObtenerPedidosKraalAsync(OrgSanVicente);

        // Assert: Aislamiento multitenant absoluto
        Assert.Contains(pedidosL, p => p.Id == pedidoL.Id);
        Assert.DoesNotContain(pedidosL, p => p.Id == pedidoSV.Id);

        Assert.Contains(pedidosSV, p => p.Id == pedidoSV.Id);
        Assert.DoesNotContain(pedidosSV, p => p.Id == pedidoL.Id);
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
    }
}
