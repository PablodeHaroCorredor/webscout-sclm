using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Features.Admissions.Services;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Identity.Services;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Features.Participants.Services;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

/// <summary>
/// Paso 2 del Coordinador de sección: editar, dar de baja, reactivar e incorporar educandos de su sección,
/// con los casos negativos (otra sección, Scouter sin coordinación) comprobados en servidor.
/// </summary>
public class GestionSeccionTests : IDisposable
{
    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly AlcanceFijo CoordinadorManada = new(AlcanceSeccion.DeSeccion(RamaScout.Manada, FuncionSeccion.Coordinador));
    private static readonly AlcanceFijo ScouterManada = new(AlcanceSeccion.DeSeccion(RamaScout.Manada, FuncionSeccion.Scouter));

    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;
    private readonly Guid _tutorId = Guid.NewGuid();

    public GestionSeccionTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();
        _options = _baseDatos.Opciones;

        using var db = new WebScoutDbContext(_options, Tenant());
        db.Users.Add(new UsuarioScout
        {
            Id = _tutorId,
            OrganizacionId = OrgLestonnac,
            NombreCompleto = "Familia sintética",
            UserName = "familia@test.es",
            NormalizedUserName = "FAMILIA@TEST.ES",
            Email = "familia@test.es",
            NormalizedEmail = "FAMILIA@TEST.ES"
        });
        db.SaveChanges();
    }

    public void Dispose() => _baseDatos.Dispose();

    // ---- Editar datos ----

    [Fact]
    public async Task Coordinador_EditaDatosDeSuSeccion_YQuedaAuditado()
    {
        var id = await CrearEducandoAsync(RamaScout.Manada);
        var (servicio, db) = Crear(CoordinadorManada);

        await servicio.ActualizarDatosAsync(id, new DatosEducandoEditables("Lucía ", "Sintética Pérez", new DateOnly(2016, 1, 2), "12345678z", null));

        var educando = await db.Educandos.AsNoTracking().SingleAsync(e => e.Id == id);
        Assert.Equal("Lucía", educando.Nombre);
        Assert.Equal("12345678Z", educando.Dni);
        Assert.Equal(RamaScout.Manada, educando.RamaActual);
        var auditoria = await db.RegistrosAuditoria.AsNoTracking().SingleAsync(r => r.RecursoId == id.ToString());
        Assert.Equal("EducandoDatosActualizados", auditoria.Accion);
        Assert.DoesNotContain("Lucía", auditoria.Detalles ?? string.Empty);
    }

    [Fact]
    public async Task Coordinador_NoEditaEducandosDeOtraSeccion()
    {
        var id = await CrearEducandoAsync(RamaScout.Tropa);
        var (servicio, db) = Crear(CoordinadorManada);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.ActualizarDatosAsync(id, new DatosEducandoEditables("Otro", "Nombre", new DateOnly(2013, 1, 1), null, null)));

        Assert.Equal("Educando", (await db.Educandos.AsNoTracking().SingleAsync(e => e.Id == id)).Nombre);
    }

    [Fact]
    public async Task ScouterSinCoordinacion_NoEditaNiDaDeBaja()
    {
        var id = await CrearEducandoAsync(RamaScout.Manada);
        var (servicio, _) = Crear(ScouterManada);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.ActualizarDatosAsync(id, new DatosEducandoEditables("Otro", "Nombre", new DateOnly(2016, 1, 1), null, null)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.DarDeBajaAsync(id, "Motivo"));
    }

    [Fact]
    public async Task EditarDatos_Invalidos_SeRechazan()
    {
        var id = await CrearEducandoAsync(RamaScout.Manada);
        var (servicio, _) = Crear(CoordinadorManada);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.ActualizarDatosAsync(id, new DatosEducandoEditables(" ", "Apellidos", new DateOnly(2016, 1, 1), null, null)));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.ActualizarDatosAsync(id, new DatosEducandoEditables("Nombre", "Apellidos", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), null, null)));
    }

    // ---- Baja y reactivación ----

    [Fact]
    public async Task Baja_SacaDelCensoActivo_YReactivarLoDevuelve()
    {
        var id = await CrearEducandoAsync(RamaScout.Manada);
        var (servicio, db) = Crear(CoordinadorManada);
        var censo = new ServicioEducandos(db, CoordinadorManada);

        await servicio.DarDeBajaAsync(id, "La familia deja el grupo");

        Assert.Empty(await censo.ObtenerTodosDelTenantAsync());
        Assert.Equal([id], (await servicio.ObtenerBajasAsync()).Select(e => e.Id));
        Assert.Contains(await db.RegistrosAuditoria.AsNoTracking().Select(r => r.Accion).ToListAsync(), a => a == "EducandoBaja");

        await servicio.ReactivarAsync(id);

        Assert.Equal([id], (await censo.ObtenerTodosDelTenantAsync()).Select(e => e.Id));
        Assert.Empty(await servicio.ObtenerBajasAsync());
    }

    [Fact]
    public async Task Baja_SinMotivo_SeRechaza()
    {
        var id = await CrearEducandoAsync(RamaScout.Manada);
        var (servicio, _) = Crear(CoordinadorManada);

        await Assert.ThrowsAsync<ArgumentException>(() => servicio.DarDeBajaAsync(id, "  "));
    }

    [Fact]
    public async Task Bajas_DeOtraSeccion_NoSeVenNiSeReactivan()
    {
        var tropa = await CrearEducandoAsync(RamaScout.Tropa, activo: false);
        var (servicio, _) = Crear(CoordinadorManada);

        Assert.Empty(await servicio.ObtenerBajasAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.ReactivarAsync(tropa));
    }

    [Fact]
    public async Task AlcanceGlobal_GestionaCualquierSeccion()
    {
        var tropa = await CrearEducandoAsync(RamaScout.Tropa);
        var (servicio, _) = Crear(AlcanceFijo.Global);

        await servicio.DarDeBajaAsync(tropa, "Cambio de grupo");

        Assert.Single(await servicio.ObtenerBajasAsync());
    }

    // ---- Admisiones ----

    [Fact]
    public async Task Admisiones_Coordinador_VeEIncorporaSoloSuSeccion()
    {
        var manada = await RegistrarSolicitudAsync("Lobato", new DateOnly(2017, 4, 15));
        var tropa = await RegistrarSolicitudAsync("Scout", new DateOnly(2013, 4, 15));
        Assert.Equal(RamaScout.Manada, manada.RamaSugerida);
        Assert.Equal(RamaScout.Tropa, tropa.RamaSugerida);

        var admisiones = CrearAdmisiones(CoordinadorManada, out _);

        Assert.Equal([manada.Id], (await admisiones.ObtenerSolicitudesAsync(filtroRama: RamaScout.Tropa)).Select(s => s.Id));

        // Ni la solicitud de otra sección, ni la propia hacia otra rama
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            admisiones.ConvertirSolicitudEnEducandoAsync(tropa.Id, RamaScout.Tropa, null, Guid.NewGuid()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            admisiones.ConvertirSolicitudEnEducandoAsync(manada.Id, RamaScout.Tropa, null, Guid.NewGuid()));

        var educando = await admisiones.ConvertirSolicitudEnEducandoAsync(manada.Id, RamaScout.Manada, null, Guid.NewGuid());
        Assert.Equal(RamaScout.Manada, educando.RamaActual);
    }

    [Fact]
    public async Task Admisiones_ScouterSinCoordinacion_NoVeSolicitudes()
    {
        await RegistrarSolicitudAsync("Lobato", new DateOnly(2017, 4, 15));
        var admisiones = CrearAdmisiones(ScouterManada, out _);

        Assert.Empty(await admisiones.ObtenerSolicitudesAsync());
    }

    // ---- Utilidades ----

    private static TenantContext Tenant()
    {
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);
        return tenant;
    }

    private (ServicioGestionSeccion Servicio, WebScoutDbContext Db) Crear(IServicioAlcanceSeccion alcance)
    {
        var tenant = Tenant();
        var db = new WebScoutDbContext(_options, tenant);
        var auditoria = new ServicioAuditoria(db, tenant, new HttpContextAccessor(), NullLogger<ServicioAuditoria>.Instance);
        return (new ServicioGestionSeccion(db, alcance, auditoria), db);
    }

    private ServicioAdmisiones CrearAdmisiones(IServicioAlcanceSeccion alcance, out WebScoutDbContext db)
    {
        var tenant = Tenant();
        db = new WebScoutDbContext(_options, tenant);
        var outbox = new ServicioOutbox(db, new FakeServicioEmail(), NullLogger<ServicioOutbox>.Instance);
        return new ServicioAdmisiones(db, tenant, outbox, NullLogger<ServicioAdmisiones>.Instance, alcance);
    }

    private async Task<WebScout.Web.Features.Admissions.Domain.SolicitudAdmision> RegistrarSolicitudAsync(string nombre, DateOnly nacimiento)
    {
        // El registro público no depende del alcance
        var admisiones = CrearAdmisiones(AlcanceFijo.Global, out var db);
        using (db)
        {
            return await admisiones.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
                nombre, "Sintético", nacimiento, "Tutor Sintético", "Madre",
                $"{nombre.ToLowerInvariant()}@test.es", "600000000", null, true, false));
        }
    }

    private async Task<Guid> CrearEducandoAsync(RamaScout rama, bool activo = true)
    {
        using var db = new WebScoutDbContext(_options, Tenant());
        var id = Guid.NewGuid();
        db.Educandos.Add(new Educando
        {
            Id = id,
            OrganizacionId = OrgLestonnac,
            TutorLegalId = _tutorId,
            Nombre = "Educando",
            Apellidos = "Sintético",
            RamaActual = rama,
            FechaNacimiento = new DateOnly(2015, 6, 1),
            Activo = activo
        });
        await db.SaveChangesAsync();
        return id;
    }
}
