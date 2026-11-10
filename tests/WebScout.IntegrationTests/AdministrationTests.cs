using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Administration.Domain;
using WebScout.Web.Features.Administration.Services;
using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Admissions.Services;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Outbox.Domain;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class AdministrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<WebScoutDbContext> _options;
    private readonly FakeServicioEmail _fakeEmail;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AdministrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<WebScoutDbContext>()
            .UseSqlite(_connection)
            .Options;

        _fakeEmail = new FakeServicioEmail();

        var initialTenant = new TenantContext();
        initialTenant.SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);

        using var context = new WebScoutDbContext(_options, initialTenant);
        context.Database.EnsureCreated();

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

        // Seed Roles
        context.Roles.Add(new RolScout
        {
            Id = Guid.NewGuid(),
            Name = "Familia",
            NormalizedName = "FAMILIA",
            ConcurrencyStamp = Guid.NewGuid().ToString()
        });

        context.SaveChanges();
    }

    private (WebScoutDbContext Db, TenantContext Tenant, IServicioConfiguracionGrupo ConfigService, IServicioAdmisiones AdmissionService, IServicioOutbox OutboxService, IServicioAuditoria AuditService) CreateServices(Guid? tenantId = null)
    {
        var activeTenantId = tenantId ?? OrgLestonnac;
        var slug = activeTenantId == OrgLestonnac ? "lestonnac" : "sanvicente";
        var nombre = activeTenantId == OrgLestonnac ? "Grupo Scout Lestonnac 330" : "Grupo Scout San Vicente 572";
        var numero = activeTenantId == OrgLestonnac ? 330 : 572;

        var tenant = new TenantContext();
        tenant.SetTenant(activeTenantId, slug, nombre, numero);

        var db = new WebScoutDbContext(_options, tenant);
        var outbox = new ServicioOutbox(db, _fakeEmail, NullLogger<ServicioOutbox>.Instance);
        var audit = new ServicioAuditoria(db, tenant, new Microsoft.AspNetCore.Http.HttpContextAccessor(), NullLogger<ServicioAuditoria>.Instance);
        var config = new ServicioConfiguracionGrupo(db, tenant, audit, NullLogger<ServicioConfiguracionGrupo>.Instance);
        var admissions = new ServicioAdmisiones(db, tenant, outbox, NullLogger<ServicioAdmisiones>.Instance);

        return (db, tenant, config, admissions, outbox, audit);
    }

    [Fact]
    public async Task ConvertirSolicitudEnEducando_CreaEducandoCuentaTutorYFichaMedicaConOutbox()
    {
        // Arrange
        var (db, _, _, admissionService, _, _) = CreateServices();
        var scouterId = Guid.NewGuid();

        var solicitud = await admissionService.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Mateo",
            ApellidosEducando: "García López",
            FechaNacimiento: new DateOnly(2017, 4, 15),
            NombreTutor: "Laura López",
            RelacionTutor: "Madre",
            EmailTutor: "laura.lopez@example.com",
            TelefonoTutor: "611223344",
            Observaciones: "Alergia a frutos secos",
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: false));

        // Act - Convertir a Educando en Manada
        var educando = await admissionService.ConvertirSolicitudEnEducandoAsync(
            solicitud.Id,
            RamaScout.Manada,
            notasKraal: "Admitido tras entrevista con Akela",
            scouterId: scouterId);

        // Assert
        Assert.NotNull(educando);
        Assert.Equal("Mateo", educando.Nombre);
        Assert.Equal("García López", educando.Apellidos);
        Assert.Equal(RamaScout.Manada, educando.RamaActual);
        Assert.True(educando.Activo);

        // 1. Verificar cuenta de tutor legal creada
        var tutor = await db.Users.FirstOrDefaultAsync(u => u.Id == educando.TutorLegalId);
        Assert.NotNull(tutor);
        Assert.Equal("laura.lopez@example.com", tutor.Email);
        Assert.Equal("Laura López", tutor.NombreCompleto);

        // 2. Verificar Ficha Médica inicial creada
        var ficha = await db.FichasMedicas.FirstOrDefaultAsync(f => f.EducandoId == educando.Id);
        Assert.NotNull(ficha);
        Assert.Equal("611223344", ficha.TelefonoEmergenciaPrincipal);

        // 3. Verificar estado de la solicitud
        var solActualizada = await db.SolicitudesAdmision.FindAsync(solicitud.Id);
        Assert.NotNull(solActualizada);
        Assert.Equal(EstadoSolicitudAdmision.Admitida, solActualizada.Estado);
        Assert.NotNull(solActualizada.FechaResolucionUtc);

        // 4. Verificar correo encolado en Outbox (PA11 / RF14)
        var outboxMsg = await db.MensajesOutbox
            .FirstOrDefaultAsync(m => m.DestinatarioEmail == "laura.lopez@example.com" 
                                   && m.TipoEvento == "BienvenidaAltaEducandoCenso");
        Assert.NotNull(outboxMsg);
        Assert.Equal(EstadoMensajeOutbox.Pendiente, outboxMsg.Estado);
        Assert.Contains("Alta en Censo de Mateo García López", outboxMsg.Asunto);

        // 5. Verificar evento de auditoría
        var audit = await db.RegistrosAuditoria
            .FirstOrDefaultAsync(a => a.Accion == "AltaEducandoDesdeAdmision" && a.RecursoId == educando.Id.ToString());
        Assert.NotNull(audit);
    }

    [Fact]
    public async Task ConvertirSolicitudEnEducando_ConTutorExistente_ReutilizaCuentaSinDuplicar()
    {
        // Arrange
        var (db, _, _, admissionService, _, _) = CreateServices();
        var scouterId = Guid.NewGuid();

        // Crear primero el hermano mayor
        var sol1 = await admissionService.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Sofía",
            ApellidosEducando: "Martínez",
            FechaNacimiento: new DateOnly(2014, 5, 10),
            NombreTutor: "Carlos Martínez",
            RelacionTutor: "Padre",
            EmailTutor: "carlos.martinez@example.com",
            TelefonoTutor: "622334455",
            Observaciones: null,
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: false));

        var ed1 = await admissionService.ConvertirSolicitudEnEducandoAsync(sol1.Id, RamaScout.Tropa, null, scouterId);

        // Crear la hermana menor
        var sol2 = await admissionService.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Lucía",
            ApellidosEducando: "Martínez",
            FechaNacimiento: new DateOnly(2019, 8, 20),
            NombreTutor: "Carlos Martínez",
            RelacionTutor: "Padre",
            EmailTutor: "carlos.martinez@example.com",
            TelefonoTutor: "622334455",
            Observaciones: null,
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: false));

        // Act
        var ed2 = await admissionService.ConvertirSolicitudEnEducandoAsync(sol2.Id, RamaScout.Castores, null, scouterId);

        // Assert
        Assert.Equal(ed1.TutorLegalId, ed2.TutorLegalId);
        var tutoresConEmail = await db.Users.CountAsync(u => u.Email == "carlos.martinez@example.com");
        Assert.Equal(1, tutoresConEmail);
    }

    [Fact]
    public async Task AjustesGrupo_GuardadoParcialPorAreas_ConservaLasOtrasAreasInalteradas_PA16()
    {
        // Arrange
        var (_, _, configService, _, _, _) = CreateServices();

        // 1. Configurar inicialmente Área Económica
        await configService.GuardarAreaEconomicaAsync(new AjustesEconomicosDto(
            IbanCuentaBancaria: "ES91 2100 0418 4502 0005 1332",
            TitularCuenta: "Grupo Scout Lestonnac",
            ConceptoCuotaSugerido: "Cuota Ronda 2026/2027"));

        // 2. Configurar Área de Admisiones
        await configService.GuardarAreaAdmisionesAsync(new AjustesAdmisionesDto(
            AdmisionesAbiertas: true,
            MensajeBienvenidaAdmisiones: "Bienvenidos al plazo de admisiones"));

        // Act - Guardar EXCLUSIVAMENTE Área Institucional
        await configService.GuardarAreaInstitucionalAsync(new AjustesInstitucionalesDto(
            Cif: "G02998877",
            DireccionBase: "Colegio Lestonnac, Albacete",
            TelefonoContacto: "967 11 22 33",
            EmailOficial: "info@scoutslestonnac.es",
            RondaSolarActiva: "2026/2027",
            FechaInicioRonda: new DateOnly(2026, 10, 1),
            FechaFinRonda: new DateOnly(2027, 7, 31)));

        // Assert (PA16)
        var configActual = await configService.ObtenerConfiguracionAsync();

        // Institucional actualizada
        Assert.Equal("G02998877", configActual.Cif);
        Assert.Equal("Colegio Lestonnac, Albacete", configActual.DireccionBase);

        // Económica intacta
        Assert.Equal("ES91 2100 0418 4502 0005 1332", configActual.IbanCuentaBancaria);
        Assert.Equal("Grupo Scout Lestonnac", configActual.TitularCuenta);

        // Admisiones intacta
        Assert.True(configActual.AdmisionesAbiertas);
        Assert.Equal("Bienvenidos al plazo de admisiones", configActual.MensajeBienvenidaAdmisiones);
    }

    [Fact]
    public async Task MonitorOutbox_ResumenEstadisticasYReintentoManual_PA11()
    {
        // Arrange
        var (db, _, _, _, outboxService, _) = CreateServices();

        // Encolar 2 mensajes
        var msg1 = await outboxService.EncolarMensajeAsync(
            OrgLestonnac, "Evento1", "test1@example.com", "User 1", "Asunto 1", "<p>Html</p>", "Texto");
        var msg2 = await outboxService.EncolarMensajeAsync(
            OrgLestonnac, "Evento2", "test2@example.com", "User 2", "Asunto 2", "<p>Html</p>", "Texto");

        // Simular que msg2 quedó fallido
        msg2.Estado = EstadoMensajeOutbox.Fallido;
        msg2.Reintentos = 5;
        msg2.ErrorUltimoReintento = "SMTP Connection Timeout";
        await db.SaveChangesAsync();

        // Act 1: Obtener estadísticas
        var resumen = await outboxService.ObtenerResumenEstadisticasAsync();
        Assert.Equal(2, resumen.TotalMensajes);
        Assert.Equal(1, resumen.Pendientes);
        Assert.Equal(1, resumen.Fallidos);

        // Act 2: Reintentar mensaje 2 manualmente
        await outboxService.ReintentarMensajeAsync(msg2.Id);

        // Assert PA11
        var msg2Recuperado = await db.MensajesOutbox.FindAsync(msg2.Id);
        Assert.NotNull(msg2Recuperado);
        Assert.Equal(EstadoMensajeOutbox.Pendiente, msg2Recuperado.Estado);
        Assert.Equal(0, msg2Recuperado.Reintentos);
        Assert.NotNull(msg2Recuperado.ProximoReintentoUtc);
    }

    [Fact]
    public async Task DiagnosticoYBackups_SimulacionDeRespaldoRegistraAuditoria_PA19()
    {
        // Arrange
        var (db, _, configService, _, _, auditService) = CreateServices();

        // Act
        var config = await configService.RegistrarSimulacionBackupAsync(
            "Verificación de snapshot PostgreSQL 17 y blob storage superada");

        // Assert (PA19 / RF65)
        Assert.NotNull(config.FechaUltimoBackupUtc);
        Assert.Contains("Verificación de snapshot", config.EstadoUltimoBackup);

        var registros = await auditService.ObtenerRegistrosAsync(filtroAccion: "VerificacionCopiaSeguridad");
        Assert.Single(registros);
        Assert.Equal(nameof(ConfiguracionGrupo), registros[0].TipoRecurso);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
