using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Admissions.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class AdmissionServiceTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;
    private readonly FakeServicioEmail _fakeEmail;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AdmissionServiceTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();

        _options = _baseDatos.Opciones;

        _fakeEmail = new FakeServicioEmail();

        var initialTenant = new TenantContext();
        initialTenant.SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);

        using var context = new WebScoutDbContext(_options, initialTenant);

        // Seed Tenant 1: Lestonnac
        context.GruposScout.Add(new GrupoScout
        {
            Id = OrgLestonnac,
            Nombre = "Grupo Scout Lestonnac 330",
            Slug = "lestonnac",
            NumeroGrupo = 330,
            EmailContacto = "kraal@lestonnac.org",
            Activo = true,
            Paleta = PaletteConfig.DefaultLestonnac
        });

        // Seed Tenant 2: San Vicente
        context.GruposScout.Add(new GrupoScout
        {
            Id = OrgSanVicente,
            Nombre = "Grupo Scout San Vicente 572",
            Slug = "sanvicente",
            NumeroGrupo = 572,
            EmailContacto = "secretaria@sanvicente572.org",
            Activo = true,
            Paleta = new PaletteConfig
            {
                PrimaryHex = "#0D47A1",
                SecondaryHex = "#FFB300",
                AccentHex = "#E65100",
                TextOnPrimaryHex = "#FFFFFF",
                TextOnSecondaryHex = "#000000",
                SurfaceHeaderHex = "#0D47A1"
            }
        });

        context.SaveChanges();
    }

    private (WebScoutDbContext DbContext, TenantContext TenantContext, IServicioAdmisiones Service) CreateService(Guid? tenantId = null)
    {
        var activeTenantId = tenantId ?? OrgLestonnac;
        var slug = activeTenantId == OrgLestonnac ? "lestonnac" : "sanvicente";
        var nombre = activeTenantId == OrgLestonnac ? "Grupo Scout Lestonnac 330" : "Grupo Scout San Vicente 572";
        var numero = activeTenantId == OrgLestonnac ? 330 : 572;

        var tenant = new TenantContext();
        tenant.SetTenant(activeTenantId, slug, nombre, numero);

        var dbContext = new WebScoutDbContext(_options, tenant);
        var servicioOutbox = new ServicioOutbox(dbContext, _fakeEmail, NullLogger<ServicioOutbox>.Instance);
        var servicioAdmisiones = new ServicioAdmisiones(
            dbContext,
            tenant,
            servicioOutbox,
            NullLogger<ServicioAdmisiones>.Instance);

        return (dbContext, tenant, servicioAdmisiones);
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
    }

    [Theory]
    [InlineData(5, RamaScout.Castores)]
    [InlineData(6, RamaScout.Castores)]
    [InlineData(8, RamaScout.Castores)]
    [InlineData(9, RamaScout.Manada)]
    [InlineData(11, RamaScout.Manada)]
    [InlineData(12, RamaScout.Tropa)]
    [InlineData(14, RamaScout.Tropa)]
    [InlineData(15, RamaScout.Escultas)]
    [InlineData(17, RamaScout.Escultas)]
    [InlineData(18, RamaScout.Clan)]
    [InlineData(20, RamaScout.Clan)]
    public void CalcularRamaPedagogica_SegunEdad_AsignaRamaCorrecta(int edad, RamaScout ramaEsperada)
    {
        var fechaReferencia = new DateOnly(2026, 11, 1);
        var fechaNacimiento = fechaReferencia.AddYears(-edad);

        var rama = SolicitudAdmision.CalcularRamaPedagogica(fechaNacimiento, fechaReferencia);

        Assert.Equal(ramaEsperada, rama);
    }

    [Fact]
    public async Task RegistrarSolicitudAdmision_SinConsentimientoRGPD_LanzaExcepcion()
    {
        var (dbContext, _, service) = CreateService();

        var dto = new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Martín",
            ApellidosEducando: "Ruiz",
            FechaNacimiento: new DateOnly(2017, 5, 10),
            NombreTutor: "Elena Gómez",
            RelacionTutor: "Madre",
            EmailTutor: "elena@example.com",
            TelefonoTutor: "600123456",
            Observaciones: null,
            ConsentimientoPrivacidad: false, // PA13: no consent
            ConsentimientoComunicaciones: false
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegistrarSolicitudAdmisionAsync(dto));

        Assert.Contains("RGPD", ex.Message);
        Assert.Empty(await dbContext.SolicitudesAdmision.ToListAsync());
        Assert.Empty(await dbContext.MensajesOutbox.ToListAsync());
    }

    [Theory]
    [InlineData("", "Ruiz", "tutor@test.es", "600000000")]
    [InlineData("Martín", "", "tutor@test.es", "600000000")]
    [InlineData("Martín", "Ruiz", "", "600000000")]
    [InlineData("Martín", "Ruiz", "tutor@test.es", "")]
    public async Task RegistrarSolicitudAdmision_DatosIncompletos_LanzaArgumentException(
        string nombre, string apellidos, string email, string telefono)
    {
        var (_, _, service) = CreateService();

        var dto = new RegistrarSolicitudAdmisionDto(
            NombreEducando: nombre,
            ApellidosEducando: apellidos,
            FechaNacimiento: new DateOnly(2017, 5, 10),
            NombreTutor: "Tutor Test",
            RelacionTutor: "Padre",
            EmailTutor: email,
            TelefonoTutor: telefono,
            Observaciones: null,
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: false
        );

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegistrarSolicitudAdmisionAsync(dto));
    }

    [Fact]
    public async Task RegistrarSolicitudAdmision_ConDatosValidos_CalculaRamaYEncolaMensajesOutbox()
    {
        var (dbContext, _, service) = CreateService();

        // Menor de 10 años -> Manada
        var fechaNac = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));
        var dto = new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Lucía",
            ApellidosEducando: "Navarro Sánchez",
            FechaNacimiento: fechaNac,
            NombreTutor: "Carlos Navarro",
            RelacionTutor: "Padre",
            EmailTutor: "carlos.navarro@example.com",
            TelefonoTutor: "611222333",
            Observaciones: "Interés especial en campamento de verano",
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: true
        );

        var solicitud = await service.RegistrarSolicitudAdmisionAsync(dto);

        // Verificación de la entidad en BD
        Assert.NotEqual(Guid.Empty, solicitud.Id);
        Assert.Equal(OrgLestonnac, solicitud.OrganizacionId);
        Assert.Equal("Lucía Navarro Sánchez", solicitud.NombreCompletoEducando);
        Assert.Equal(RamaScout.Manada, solicitud.RamaSugerida);
        Assert.Equal(EstadoSolicitudAdmision.Pendiente, solicitud.Estado);

        // Verificación de los mensajes Outbox (RF14 / PA11)
        var mensajesOutbox = await dbContext.MensajesOutbox.ToListAsync();
        Assert.Equal(2, mensajesOutbox.Count);

        // 1. Acuse a la familia
        var acuseFamilia = mensajesOutbox.FirstOrDefault(m => m.TipoEvento == "SolicitudAdmisionPresentada");
        Assert.NotNull(acuseFamilia);
        Assert.Equal("carlos.navarro@example.com", acuseFamilia.DestinatarioEmail);
        Assert.Equal("Carlos Navarro", acuseFamilia.DestinatarioNombre);
        Assert.Contains("Solicitud de admisión recibida", acuseFamilia.Asunto);
        Assert.Equal($"solicitud-admision-acuse-{solicitud.Id}", acuseFamilia.ClaveDeduplicacion);

        // 2. Notificación interna al Kraal
        var avisoKraal = mensajesOutbox.FirstOrDefault(m => m.TipoEvento == "AvisoSecretariaNuevaAdmision");
        Assert.NotNull(avisoKraal);
        Assert.Equal("kraal@lestonnac.org", avisoKraal.DestinatarioEmail);
        Assert.Contains("Nueva solicitud de admisión", avisoKraal.Asunto);
        Assert.Equal($"solicitud-admision-kraal-{solicitud.Id}", avisoKraal.ClaveDeduplicacion);
    }

    [Fact]
    public async Task RegistrarMensajeContacto_SinConsentimientoRGPD_LanzaExcepcion()
    {
        var (dbContext, _, service) = CreateService();

        var dto = new RegistrarMensajeContactoDto(
            NombreRemitente: "Ana López",
            EmailRemitente: "ana@example.com",
            TelefonoRemitente: "655444333",
            Asunto: "Información sobre cuotas",
            Mensaje: "Hola, quisiera consultar las cuotas para dos hermanos.",
            ConsentimientoPrivacidad: false
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegistrarMensajeContactoAsync(dto));

        Assert.Contains("privacidad", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await dbContext.MensajesContacto.ToListAsync());
        Assert.Empty(await dbContext.MensajesOutbox.ToListAsync());
    }

    [Fact]
    public async Task RegistrarMensajeContacto_ConDatosValidos_EncolaMensajeYGuardaEnBuzon()
    {
        var (dbContext, _, service) = CreateService();

        var dto = new RegistrarMensajeContactoDto(
            NombreRemitente: "David Martos",
            EmailRemitente: "david.martos@example.com",
            TelefonoRemitente: "688999000",
            Asunto: "Duda sobre uniforme y reuniones",
            Mensaje: "¿A qué hora empiezan las reuniones de Castores los sábados?",
            ConsentimientoPrivacidad: true
        );

        var mensaje = await service.RegistrarMensajeContactoAsync(dto);

        // Verificación de la entidad en BD
        Assert.NotEqual(Guid.Empty, mensaje.Id);
        Assert.Equal(OrgLestonnac, mensaje.OrganizacionId);
        Assert.Equal("David Martos", mensaje.NombreRemitente);
        Assert.False(mensaje.Leido);
        Assert.False(mensaje.Respondido);

        // Verificación del mensaje Outbox
        var outbox = await dbContext.MensajesOutbox.FirstOrDefaultAsync(m => m.TipoEvento == "MensajeContactoPresentado");
        Assert.NotNull(outbox);
        Assert.Equal("david.martos@example.com", outbox.DestinatarioEmail);
        Assert.Contains("Hemos recibido su mensaje", outbox.Asunto);
        Assert.Equal($"mensaje-contacto-{mensaje.Id}", outbox.ClaveDeduplicacion);
    }

    [Fact]
    public async Task AislamientoMultitenant_SolicitudesNoVisiblesEntreGrupos()
    {
        // 1. Grupo Lestonnac registra una solicitud
        var (dbLestonnac, _, serviceLestonnac) = CreateService(OrgLestonnac);
        var dto = new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Samuel",
            ApellidosEducando: "García",
            FechaNacimiento: new DateOnly(2015, 3, 1),
            NombreTutor: "Laura Mora",
            RelacionTutor: "Madre",
            EmailTutor: "laura.mora@example.com",
            TelefonoTutor: "612345678",
            Observaciones: null,
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: false
        );

        var solicitudLestonnac = await serviceLestonnac.RegistrarSolicitudAdmisionAsync(dto);

        // 2. Grupo San Vicente consulta las solicitudes
        var (_, _, serviceSanVicente) = CreateService(OrgSanVicente);
        var solicitudesSanVicente = await serviceSanVicente.ObtenerSolicitudesAsync();

        // RF13: El grupo San Vicente no debe ver ninguna solicitud del grupo Lestonnac
        Assert.Empty(solicitudesSanVicente);

        // 3. Grupo San Vicente intenta cambiar el estado de la solicitud de Lestonnac -> Bloqueado
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceSanVicente.CambiarEstadoSolicitudAsync(
                solicitudLestonnac.Id,
                EstadoSolicitudAdmision.Admitida,
                "Intento cross-tenant no autorizado",
                Guid.NewGuid()));

        Assert.Contains("no pertenece a este grupo scout", ex.Message);
    }

    [Fact]
    public async Task AislamientoMultitenant_MensajesContactoNoVisiblesEntreGrupos()
    {
        // 1. Grupo Lestonnac recibe un mensaje
        var (_, _, serviceLestonnac) = CreateService(OrgLestonnac);
        var dto = new RegistrarMensajeContactoDto(
            NombreRemitente: "Vecino Lestonnac",
            EmailRemitente: "vecino@example.com",
            TelefonoRemitente: null,
            Asunto: "Agradecimiento",
            Mensaje: "Gracias por la labor social en el barrio.",
            ConsentimientoPrivacidad: true
        );
        var mensajeLestonnac = await serviceLestonnac.RegistrarMensajeContactoAsync(dto);

        // 2. Grupo San Vicente consulta buzón de contacto
        var (_, _, serviceSanVicente) = CreateService(OrgSanVicente);
        var mensajesSanVicente = await serviceSanVicente.ObtenerMensajesContactoAsync();

        Assert.Empty(mensajesSanVicente);

        // 3. Grupo San Vicente intenta marcar como leído un mensaje de Lestonnac -> Bloqueado
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceSanVicente.MarcarMensajeLeidoAsync(mensajeLestonnac.Id, true));

        Assert.Contains("no pertenece a este grupo scout", ex.Message);
    }

    [Fact]
    public async Task CambiarEstadoSolicitud_ActualizaEstadoYNotasKraal()
    {
        var (dbContext, _, service) = CreateService();

        var dto = new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Mateo",
            ApellidosEducando: "Cano",
            FechaNacimiento: new DateOnly(2014, 8, 20),
            NombreTutor: "Jorge Cano",
            RelacionTutor: "Padre",
            EmailTutor: "jorge@example.com",
            TelefonoTutor: "699000111",
            Observaciones: null,
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: false
        );

        var solicitud = await service.RegistrarSolicitudAdmisionAsync(dto);
        var scouterId = Guid.NewGuid();

        // Transición 1: Pasar a Lista de Espera
        await service.CambiarEstadoSolicitudAsync(
            solicitud.Id,
            EstadoSolicitudAdmision.EnListaEspera,
            "Rama Manada completa temporalmente. En primer puesto de lista de espera.",
            scouterId);

        var actualizada1 = await dbContext.SolicitudesAdmision.FindAsync(solicitud.Id);
        Assert.NotNull(actualizada1);
        Assert.Equal(EstadoSolicitudAdmision.EnListaEspera, actualizada1.Estado);
        Assert.Equal(scouterId, actualizada1.AtendidoPorUsuarioId);
        Assert.Contains("completa temporalmente", actualizada1.NotasKraal);
        Assert.NotNull(actualizada1.FechaResolucionUtc);

        // Transición 2: Admitir al educando
        await service.CambiarEstadoSolicitudAsync(
            solicitud.Id,
            EstadoSolicitudAdmision.Admitida,
            "Plaza liberada. Se contacta a la familia para tramitar alta.",
            scouterId);

        var actualizada2 = await dbContext.SolicitudesAdmision.FindAsync(solicitud.Id);
        Assert.NotNull(actualizada2);
        Assert.Equal(EstadoSolicitudAdmision.Admitida, actualizada2.Estado);
        Assert.Contains("Plaza liberada", actualizada2.NotasKraal);
    }

    [Fact]
    public async Task MarcarMensajeLeido_ActualizaEstadoLeido()
    {
        var (dbContext, _, service) = CreateService();

        var dto = new RegistrarMensajeContactoDto(
            NombreRemitente: "Sara Benítez",
            EmailRemitente: "sara@example.com",
            TelefonoRemitente: null,
            Asunto: "Consulta actividades abiertas",
            Mensaje: "¿Hacéis jornadas de puertas abiertas?",
            ConsentimientoPrivacidad: true
        );

        var mensaje = await service.RegistrarMensajeContactoAsync(dto);
        Assert.False(mensaje.Leido);

        // Marcar como leído
        await service.MarcarMensajeLeidoAsync(mensaje.Id, true);
        var mensajeLeido = await dbContext.MensajesContacto.FindAsync(mensaje.Id);
        Assert.NotNull(mensajeLeido);
        Assert.True(mensajeLeido.Leido);

        // Marcar como no leído
        await service.MarcarMensajeLeidoAsync(mensaje.Id, false);
        var mensajeNoLeido = await dbContext.MensajesContacto.FindAsync(mensaje.Id);
        Assert.NotNull(mensajeNoLeido);
        Assert.False(mensajeNoLeido.Leido);
    }

    [Fact]
    public async Task ObtenerSolicitudes_FiltroPorEstadoYPorRama_FiltraCorrectamente()
    {
        var (_, _, service) = CreateService();

        // Solicitud 1: Manada (10 años), Pendiente
        var sol1 = await service.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            "Educando 1", "Apellidos", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)),
            "Tutor 1", "Madre", "tutor1@test.es", "600000001", null, true, false));

        // Solicitud 2: Tropa (13 años), Pendiente
        var sol2 = await service.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            "Educando 2", "Apellidos", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)),
            "Tutor 2", "Padre", "tutor2@test.es", "600000002", null, true, false));

        // Solicitud 3: Manada (9 años), Admitida
        var sol3 = await service.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            "Educando 3", "Apellidos", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-9)),
            "Tutor 3", "Tutor", "tutor3@test.es", "600000003", null, true, false));

        await service.CambiarEstadoSolicitudAsync(sol3.Id, EstadoSolicitudAdmision.Admitida, "Admitido", Guid.NewGuid());

        // Test filtro por rama Manada
        var manadas = await service.ObtenerSolicitudesAsync(filtroRama: RamaScout.Manada);
        Assert.Equal(2, manadas.Count);
        Assert.All(manadas, s => Assert.Equal(RamaScout.Manada, s.RamaSugerida));

        // Test filtro por estado Pendiente
        var pendientes = await service.ObtenerSolicitudesAsync(filtroEstado: EstadoSolicitudAdmision.Pendiente);
        Assert.Equal(2, pendientes.Count);
        Assert.All(pendientes, s => Assert.Equal(EstadoSolicitudAdmision.Pendiente, s.Estado));

        // Test filtro combinado: Rama Manada y Estado Admitida
        var manadaAdmitida = await service.ObtenerSolicitudesAsync(
            filtroEstado: EstadoSolicitudAdmision.Admitida,
            filtroRama: RamaScout.Manada);
        Assert.Single(manadaAdmitida);
        Assert.Equal(sol3.Id, manadaAdmitida[0].Id);
    }
}
