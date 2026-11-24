using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Dossier.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Identity.Services;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Inscriptions.Services;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Features.Participants.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

/// <summary>Alcance fijo para pruebas de servicios que no verifican la resolución del alcance.</summary>
public sealed class AlcanceFijo(AlcanceSeccion alcance) : IServicioAlcanceSeccion
{
    public static readonly AlcanceFijo Global = new(AlcanceSeccion.Global);

    public Task<AlcanceSeccion> ObtenerAlcanceAsync() => Task.FromResult(alcance);
}

public sealed class UsuarioActualFijo(Guid? id) : IUsuarioActual
{
    public Task<Guid?> ObtenerIdAsync() => Task.FromResult(id);
}

/// <summary>
/// Alcance por sección del Kraal evaluado en servidor: resolución desde base de datos, revocación inmediata
/// (PA02), una sola sección por persona y límite aplicado por los servicios aunque se pida otra rama.
/// </summary>
public class AlcanceSeccionTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AlcanceSeccionTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();
        _options = _baseDatos.Opciones;

        using var db = new WebScoutDbContext(_options, new TenantContext());
        foreach (var rol in RolesScout.Todos)
        {
            db.Roles.Add(new RolScout(rol) { Id = Guid.NewGuid(), NormalizedName = rol.ToUpperInvariant() });
        }
        db.SaveChanges();
    }

    public void Dispose() => _baseDatos.Dispose();

    // ---- Reglas del alcance ----

    [Fact]
    public void Coordinador_GestionaSoloSuSeccion_ScouterSoloConsulta()
    {
        var coordinador = AlcanceSeccion.DeSeccion(RamaScout.Manada, FuncionSeccion.Coordinador);
        var scouter = AlcanceSeccion.DeSeccion(RamaScout.Manada, FuncionSeccion.Scouter);

        Assert.True(coordinador.PuedeGestionar(RamaScout.Manada));
        Assert.False(coordinador.PuedeGestionar(RamaScout.Tropa));
        Assert.False(coordinador.PuedeVer(RamaScout.Tropa));

        Assert.True(scouter.PuedeVer(RamaScout.Manada));
        Assert.False(scouter.PuedeGestionar(RamaScout.Manada));

        Assert.True(AlcanceSeccion.Global.PuedeGestionar(RamaScout.Clan));
        Assert.False(AlcanceSeccion.Ninguno.PuedeVer(RamaScout.Manada));
    }

    [Fact]
    public void RamaDeConsulta_SinAlcanceGlobal_IgnoraLaRamaPedida()
    {
        var scouterManada = AlcanceSeccion.DeSeccion(RamaScout.Manada, FuncionSeccion.Scouter);

        Assert.Equal(RamaScout.Manada, scouterManada.RamaDeConsulta(RamaScout.Tropa));
        Assert.Equal(RamaScout.Manada, scouterManada.RamaDeConsulta(null));
        Assert.Equal(RamaScout.Tropa, AlcanceSeccion.Global.RamaDeConsulta(RamaScout.Tropa));
        Assert.Null(AlcanceSeccion.Global.RamaDeConsulta(null));
    }

    // ---- Resolución desde base de datos ----

    [Fact]
    public async Task Scouter_ConAsignacionVigente_ObtieneSuSeccion()
    {
        var usuarioId = await CrearMiembroAsync(OrgLestonnac, RolesScout.Scouter, RamaScout.Manada, FuncionSeccion.Coordinador);

        var alcance = await ResolverAlcanceAsync(OrgLestonnac, usuarioId);

        Assert.Equal(AlcanceSeccion.DeSeccion(RamaScout.Manada, FuncionSeccion.Coordinador), alcance);
    }

    [Fact]
    public async Task RevocarAsignacion_SeAplicaEnLaSiguienteConsulta()
    {
        var usuarioId = await CrearMiembroAsync(OrgLestonnac, RolesScout.Scouter, RamaScout.Manada, FuncionSeccion.Scouter);
        var tenant = Tenant(OrgLestonnac);
        using var db = new WebScoutDbContext(_options, tenant);
        var servicio = new ServicioAlcanceSeccion(db, new UsuarioActualFijo(usuarioId));

        Assert.True((await servicio.ObtenerAlcanceAsync()).PuedeVer(RamaScout.Manada));

        // Revocación desde otro contexto (otra petición), con la «sesión» del scouter aún abierta
        using (var dbAdmin = new WebScoutDbContext(_options, tenant))
        {
            var asignacion = await dbAdmin.AsignacionesSeccion.SingleAsync(a => a.UsuarioId == usuarioId);
            asignacion.RevocadaUtc = DateTime.UtcNow;
            await dbAdmin.SaveChangesAsync();
        }

        Assert.Equal(AlcanceSeccion.Ninguno, await servicio.ObtenerAlcanceAsync());
    }

    [Fact]
    public async Task Coordinacion_TieneAlcanceGlobal()
    {
        var usuarioId = await CrearMiembroAsync(OrgLestonnac, RolesScout.Coordinacion, rama: null, funcion: null);

        Assert.Equal(AlcanceSeccion.Global, await ResolverAlcanceAsync(OrgLestonnac, usuarioId));
    }

    [Fact]
    public async Task Scouter_SinAsignacion_NoTieneAlcance()
    {
        var usuarioId = await CrearMiembroAsync(OrgLestonnac, RolesScout.Scouter, rama: null, funcion: null);

        Assert.Equal(AlcanceSeccion.Ninguno, await ResolverAlcanceAsync(OrgLestonnac, usuarioId));
    }

    [Fact]
    public async Task CuentaDesactivada_NoTieneAlcance()
    {
        var usuarioId = await CrearMiembroAsync(OrgLestonnac, RolesScout.Scouter, RamaScout.Manada, FuncionSeccion.Coordinador);
        using (var db = new WebScoutDbContext(_options, Tenant(OrgLestonnac)))
        {
            var usuario = await db.Users.SingleAsync(u => u.Id == usuarioId);
            usuario.Activo = false;
            await db.SaveChangesAsync();
        }

        Assert.Equal(AlcanceSeccion.Ninguno, await ResolverAlcanceAsync(OrgLestonnac, usuarioId));
    }

    [Fact]
    public async Task CoordinadorDeOtroGrupo_NoTieneAlcanceEnEsteGrupo()
    {
        var coordinadorSanVicente = await CrearMiembroAsync(OrgSanVicente, RolesScout.Scouter, RamaScout.Manada, FuncionSeccion.Coordinador);

        Assert.Equal(AlcanceSeccion.Ninguno, await ResolverAlcanceAsync(OrgLestonnac, coordinadorSanVicente));
    }

    [Fact]
    public async Task UnaPersona_NoPuedeTenerDosSeccionesVigentes()
    {
        var usuarioId = await CrearMiembroAsync(OrgLestonnac, RolesScout.Scouter, RamaScout.Manada, FuncionSeccion.Coordinador);
        using var db = new WebScoutDbContext(_options, Tenant(OrgLestonnac));

        db.AsignacionesSeccion.Add(NuevaAsignacion(OrgLestonnac, usuarioId, RamaScout.Tropa, FuncionSeccion.Coordinador));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();

        // Tras revocar la vigente sí puede asignarse otra sección
        var vigente = await db.AsignacionesSeccion.SingleAsync(a => a.UsuarioId == usuarioId);
        vigente.RevocadaUtc = DateTime.UtcNow;
        db.AsignacionesSeccion.Add(NuevaAsignacion(OrgLestonnac, usuarioId, RamaScout.Tropa, FuncionSeccion.Coordinador));
        await db.SaveChangesAsync();

        Assert.Equal(RamaScout.Tropa, (await ResolverAlcanceAsync(OrgLestonnac, usuarioId)).Rama);
    }

    // ---- Límite aplicado por los servicios ----

    [Fact]
    public async Task Censo_CoordinadorManada_NoVeOtraSeccionAunqueLaPida()
    {
        var datos = await SembrarActividadConDosSeccionesAsync();
        using var db = new WebScoutDbContext(_options, Tenant(OrgLestonnac));
        var servicio = new ServicioEducandos(db, CoordinadorManada);

        Assert.Empty(await servicio.ObtenerPorRamaAsync(RamaScout.Tropa));
        var todos = await servicio.ObtenerTodosDelTenantAsync();
        Assert.Equal([datos.EducandoManada], todos.Select(e => e.Id));
    }

    [Fact]
    public async Task MesaInscripciones_CoordinadorManada_SoloRecibeSuSeccion()
    {
        var datos = await SembrarActividadConDosSeccionesAsync();
        using var db = new WebScoutDbContext(_options, Tenant(OrgLestonnac));
        var servicio = new ServicioInscripciones(db, Tenant(OrgLestonnac), new FakeAlmacenamientoJustificantes(),
            NullLogger<ServicioInscripciones>.Instance, CoordinadorManada);

        var mesa = await servicio.ObtenerInscripcionesMesaKraalAsync(ramaFiltro: RamaScout.Tropa);
        var actividad = await servicio.ObtenerInscripcionesActividadAsync(datos.ActividadId);

        Assert.Equal([datos.InscripcionManada], mesa.Select(i => i.Id));
        Assert.Equal([datos.InscripcionManada], actividad.Select(i => i.Id));
    }

    [Fact]
    public async Task RevisarInscripcion_DeOtraSeccion_SeRechazaSinModificarla()
    {
        var datos = await SembrarActividadConDosSeccionesAsync();
        using (var db = new WebScoutDbContext(_options, Tenant(OrgLestonnac)))
        {
            var servicio = new ServicioInscripciones(db, Tenant(OrgLestonnac), new FakeAlmacenamientoJustificantes(),
                NullLogger<ServicioInscripciones>.Instance, CoordinadorManada);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                servicio.RevisarInscripcionAsync(datos.InscripcionTropa, EstadoInscripcion.Confirmada, null, Guid.NewGuid()));
        }

        using var dbLectura = new WebScoutDbContext(_options, Tenant(OrgLestonnac));
        var inscripcion = await dbLectura.Inscripciones.SingleAsync(i => i.Id == datos.InscripcionTropa);
        Assert.Equal(EstadoInscripcion.EnRevision, inscripcion.Estado);
    }

    [Fact]
    public async Task Dossier_CoordinadorManada_SoloIncluyeSuSeccion()
    {
        var datos = await SembrarActividadConDosSeccionesAsync();
        using var db = new WebScoutDbContext(_options, Tenant(OrgLestonnac));
        var servicio = new ServicioDossierAsistencia(db, Tenant(OrgLestonnac),
            NullLogger<ServicioDossierAsistencia>.Instance, CoordinadorManada);

        var dossier = await servicio.ObtenerDossierActividadAsync(datos.ActividadId);

        Assert.Equal([datos.InscripcionManada], dossier.Inscripciones.Select(i => i.Id));
        Assert.All(dossier.RegistrosSanitarios, r => Assert.Equal(RamaScout.Manada, r.Rama));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.RegistrarCheckInSalidaAsync(datos.InscripcionTropa, true));
    }

    // ---- Utilidades ----

    private static readonly AlcanceFijo CoordinadorManada =
        new(AlcanceSeccion.DeSeccion(RamaScout.Manada, FuncionSeccion.Coordinador));

    private static TenantContext Tenant(Guid organizacionId)
    {
        var tenant = new TenantContext();
        tenant.SetTenant(organizacionId, organizacionId == OrgLestonnac ? "lestonnac" : "sanvicente", "Grupo", 1);
        return tenant;
    }

    private async Task<AlcanceSeccion> ResolverAlcanceAsync(Guid organizacionId, Guid usuarioId)
    {
        using var db = new WebScoutDbContext(_options, Tenant(organizacionId));
        return await new ServicioAlcanceSeccion(db, new UsuarioActualFijo(usuarioId)).ObtenerAlcanceAsync();
    }

    private static AsignacionSeccion NuevaAsignacion(Guid org, Guid usuarioId, RamaScout rama, FuncionSeccion funcion) =>
        new() { Id = Guid.NewGuid(), OrganizacionId = org, UsuarioId = usuarioId, Rama = rama, Funcion = funcion };

    private async Task<Guid> CrearMiembroAsync(Guid org, string rol, RamaScout? rama, FuncionSeccion? funcion)
    {
        using var db = new WebScoutDbContext(_options, Tenant(org));
        var id = Guid.NewGuid();
        var email = $"{id:N}@test.es";
        db.Users.Add(new UsuarioScout
        {
            Id = id,
            OrganizacionId = org,
            NombreCompleto = "Miembro sintético",
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant()
        });
        var rolId = await db.Roles.Where(r => r.Name == rol).Select(r => r.Id).SingleAsync();
        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = id, RoleId = rolId });
        if (rama is not null && funcion is not null)
        {
            db.AsignacionesSeccion.Add(NuevaAsignacion(org, id, rama.Value, funcion.Value));
        }
        await db.SaveChangesAsync();
        return id;
    }

    private sealed record DatosDosSecciones(
        Guid ActividadId, Guid EducandoManada, Guid EducandoTropa, Guid InscripcionManada, Guid InscripcionTropa);

    private async Task<DatosDosSecciones> SembrarActividadConDosSeccionesAsync()
    {
        using var db = new WebScoutDbContext(_options, Tenant(OrgLestonnac));
        var tutorId = Guid.NewGuid();
        db.Users.Add(new UsuarioScout
        {
            Id = tutorId,
            OrganizacionId = OrgLestonnac,
            NombreCompleto = "Familia sintética",
            UserName = "familia@test.es",
            NormalizedUserName = "FAMILIA@TEST.ES",
            Email = "familia@test.es",
            NormalizedEmail = "FAMILIA@TEST.ES"
        });

        var actividadId = Guid.NewGuid();
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Acampada conjunta",
            Lugar = "Albergue sintético",
            Tipo = TipoActividad.Acampada,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Manada, RamaScout.Tropa],
            Precio = 25m,
            FechaInicio = DateTime.UtcNow.AddDays(10),
            FechaFin = DateTime.UtcNow.AddDays(12)
        });

        Guid CrearEducandoInscrito(RamaScout rama, string nombre, out Guid inscripcionId)
        {
            var educandoId = Guid.NewGuid();
            db.Educandos.Add(new Educando
            {
                Id = educandoId,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorId,
                Nombre = nombre,
                Apellidos = "Sintético",
                RamaActual = rama,
                FechaNacimiento = new DateOnly(2014, 3, 1),
                Activo = true,
                FichaMedica = new FichaMedica
                {
                    Id = Guid.NewGuid(),
                    OrganizacionId = OrgLestonnac,
                    NumeroSeguridadSocial = "000000000000",
                    TelefonoEmergenciaPrincipal = "600000000"
                }
            });
            inscripcionId = Guid.NewGuid();
            db.Inscripciones.Add(new InscripcionActividad
            {
                Id = inscripcionId,
                OrganizacionId = OrgLestonnac,
                ActividadId = actividadId,
                EducandoId = educandoId,
                TutorLegalId = tutorId,
                Estado = EstadoInscripcion.EnRevision,
                ImporteEuros = 25m,
                AutorizacionFirmada = true
            });
            return educandoId;
        }

        var manada = CrearEducandoInscrito(RamaScout.Manada, "Lobato", out var inscripcionManada);
        var tropa = CrearEducandoInscrito(RamaScout.Tropa, "Scout", out var inscripcionTropa);
        await db.SaveChangesAsync();

        return new DatosDosSecciones(actividadId, manada, tropa, inscripcionManada, inscripcionTropa);
    }
}
