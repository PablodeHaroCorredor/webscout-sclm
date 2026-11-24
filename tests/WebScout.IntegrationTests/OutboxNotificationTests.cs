using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Inscriptions.Services;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Outbox.Domain;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Outbox.Templates;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class FakeServicioEmail : IServicioEmail
{
    public bool DeberiaFallar { get; set; } = false;
    public string MensajeFallo { get; set; } = "Servidor SMTP apagado o inalcanzable (Error de conexión)";
    public List<(string Destinatario, string Asunto, string Html, string? MessageId)> MensajesEnviados { get; } = new();

    public Task EnviarEmailAsync(
        string destinatarioEmail,
        string destinatarioNombre,
        string asunto,
        string cuerpoHtml,
        string cuerpoTextoPlano,
        string? remitenteNombre = null,
        string? replyToEmail = null,
        string? messageId = null,
        CancellationToken cancellationToken = default)
    {
        if (DeberiaFallar)
        {
            throw new InvalidOperationException(MensajeFallo);
        }

        MensajesEnviados.Add((destinatarioEmail, asunto, cuerpoHtml, messageId));
        return Task.CompletedTask;
    }
}

public class OutboxNotificationTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;
    private readonly FakeAlmacenamientoJustificantes _fakeStorage;
    private readonly FakeServicioEmail _fakeEmail;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public OutboxNotificationTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();

        _options = _baseDatos.Opciones;

        _fakeStorage = new FakeAlmacenamientoJustificantes();
        _fakeEmail = new FakeServicioEmail();

        var initialTenant = new TenantContext();
        initialTenant.SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);

        using var context = new WebScoutDbContext(_options, initialTenant);

        // Seed de Grupo Scout
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

        // Seed de Tutor y Scouter
        var tutorId = Guid.NewGuid();
        context.Users.Add(new UsuarioScout
        {
            Id = tutorId,
            UserName = "familia@scout.es",
            Email = "familia@scout.es",
            NombreCompleto = "Padre Responsable",
            OrganizacionId = OrgLestonnac
        });

        var scouterId = Guid.NewGuid();
        context.Users.Add(new UsuarioScout
        {
            Id = scouterId,
            UserName = "scouter@scout.es",
            Email = "scouter@scout.es",
            NombreCompleto = "Scouter Coordinador",
            OrganizacionId = OrgLestonnac
        });

        // Seed de Educando
        var educando = new Educando
        {
            Id = Guid.NewGuid(),
            OrganizacionId = OrgLestonnac,
            TutorLegalId = tutorId,
            Nombre = "Pablo",
            Apellidos = "Scout Menor",
            FechaNacimiento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)),
            RamaActual = RamaScout.Manada,
            Activo = true
        };
        context.Educandos.Add(educando);

        // Seed de Actividad
        var actividad = new ActividadScout
        {
            Id = Guid.NewGuid(),
            OrganizacionId = OrgLestonnac,
            Titulo = "Campamento de Primavera 2027",
            Lugar = "Sierra del Segura",
            FechaInicio = DateTime.UtcNow.AddDays(20),
            FechaFin = DateTime.UtcNow.AddDays(23),
            Precio = 45m,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Manada, RamaScout.Tropa]
        };
        context.Actividades.Add(actividad);

        context.SaveChanges();
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
    }

    private (WebScoutDbContext dbContext, ITenantContext tenantContext) CreateContext()
    {
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);
        return (new WebScoutDbContext(_options, tenant), tenant);
    }

    [Fact]
    public async Task Inscripcion_ConServidorCorreoApagado_DebeGuardarInscripcionYDejarMensajePendiente()
    {
        // PA11: Trámite guardado en base de datos con servidor de correo apagado
        var (dbContext, tenant) = CreateContext();

        // 1. Simular servidor de correo SMTP totalmente apagado
        _fakeEmail.DeberiaFallar = true;

        var servicioOutbox = new ServicioOutbox(dbContext, _fakeEmail, NullLogger<ServicioOutbox>.Instance);
        var servicioInscripciones = new ServicioInscripciones(
            dbContext,
            tenant,
            _fakeStorage,
            NullLogger<ServicioInscripciones>.Instance,
            servicioOutbox);

        var educando = await dbContext.Educandos.FirstAsync();
        var actividad = await dbContext.Actividades.FirstAsync();
        var tutor = await dbContext.Users.FirstAsync(u => u.Email == "familia@scout.es");
        var scouter = await dbContext.Users.FirstAsync(u => u.Email == "scouter@scout.es");

        // 2. Realizar inscripción familiar con correo apagado
        var inscripcion = await servicioInscripciones.InscribirEducandoAsync(
            actividad.Id,
            educando.Id,
            tutor.Id,
            "Anotación familiar inicial");

        // 3. El Kraal revisa y solicita subsanación con servidor de correo apagado
        await servicioInscripciones.RevisarInscripcionAsync(
            inscripcion.Id,
            EstadoInscripcion.RequiereSubsanacion,
            "Justificante ilegible, por favor resubir en formato PDF legible",
            scouter.Id);

        // 4. Verificación de PA11: El trámite se GUARDÓ con éxito a pesar de que el correo fallará
        var inscripcionEnBd = await dbContext.Inscripciones.FindAsync(inscripcion.Id);
        Assert.NotNull(inscripcionEnBd);
        Assert.Equal(EstadoInscripcion.RequiereSubsanacion, inscripcionEnBd.Estado);
        Assert.Equal("Justificante ilegible, por favor resubir en formato PDF legible", inscripcionEnBd.NotasKraal);

        // 5. Verificación Outbox: El mensaje de subsanación está persistido en la BD en estado Pendiente
        var mensajesOutbox = await dbContext.MensajesOutbox.ToListAsync();
        Assert.NotEmpty(mensajesOutbox);

        var mensajeSubsanacion = mensajesOutbox.FirstOrDefault(m => m.TipoEvento == "InscripcionSubsanacionRequerida");
        Assert.NotNull(mensajeSubsanacion);
        Assert.Equal(EstadoMensajeOutbox.Pendiente, mensajeSubsanacion.Estado);
        Assert.Equal(tutor.Email, mensajeSubsanacion.DestinatarioEmail);
        Assert.Equal(0, mensajeSubsanacion.Reintentos);

        // 6. Ejecutar el worker con SMTP todavía apagado
        var procesadosConFallo = await servicioOutbox.ProcesarLotePendienteAsync();
        Assert.Equal(0, procesadosConFallo);

        // El mensaje sigue pendiente pero incrementó sus reintentos y registró el error técnico
        var mensajePostFallo = await dbContext.MensajesOutbox.FindAsync(mensajeSubsanacion.Id);
        Assert.NotNull(mensajePostFallo);
        Assert.Equal(EstadoMensajeOutbox.Pendiente, mensajePostFallo.Estado);
        Assert.Equal(1, mensajePostFallo.Reintentos);
        Assert.Contains("Servidor SMTP apagado", mensajePostFallo.ErrorUltimoReintento);
    }

    [Fact]
    public async Task WorkerDespachadorOutbox_CuandoServicioSmtpDisponible_DebeDespacharMensajeYMarcarEnviado()
    {
        // PA11: El worker de segundo plano reintenta y entrega los mensajes pendientes al reiniciar el servicio de correo
        var (dbContext, tenant) = CreateContext();

        var servicioOutbox = new ServicioOutbox(dbContext, _fakeEmail, NullLogger<ServicioOutbox>.Instance);

        // 1. Encolar mensaje pendiente
        var mensaje = await servicioOutbox.EncolarMensajeAsync(
            OrgLestonnac,
            "InscripcionConfirmada",
            "familia@scout.es",
            "Familia Scout",
            "¡Plaza Confirmada!",
            "<p>Plaza confirmada</p>",
            "Plaza confirmada",
            claveDeduplicacion: "test-confirmacion-1");

        Assert.Equal(EstadoMensajeOutbox.Pendiente, mensaje.Estado);

        // 2. Ejecutar procesamiento con SMTP disponible
        _fakeEmail.DeberiaFallar = false;
        var enviados = await servicioOutbox.ProcesarLotePendienteAsync();

        // 3. Verificación
        Assert.Equal(1, enviados);
        Assert.Single(_fakeEmail.MensajesEnviados);
        Assert.Equal("familia@scout.es", _fakeEmail.MensajesEnviados[0].Destinatario);

        var mensajeActualizado = await dbContext.MensajesOutbox.FindAsync(mensaje.Id);
        Assert.NotNull(mensajeActualizado);
        Assert.Equal(EstadoMensajeOutbox.Enviado, mensajeActualizado.Estado);
        Assert.NotNull(mensajeActualizado.FechaProcesadoUtc);
        Assert.Null(mensajeActualizado.ErrorUltimoReintento);
    }

    [Fact]
    public async Task ServicioOutbox_ConMismaClaveDeduplicacion_NoDebeDuplicarMensaje()
    {
        // PA11: Deduplicación de intención para no enviar correos repetidos
        var (dbContext, _) = CreateContext();
        var servicioOutbox = new ServicioOutbox(dbContext, _fakeEmail, NullLogger<ServicioOutbox>.Instance);

        const string claveUnica = "intencion-unica-123";

        // Primer encolado
        var m1 = await servicioOutbox.EncolarMensajeAsync(
            OrgLestonnac,
            "RecordatorioPago",
            "familia@scout.es",
            "Familia Scout",
            "Recordatorio de pago",
            "<p>Aviso</p>",
            "Aviso",
            claveDeduplicacion: claveUnica);

        // Segundo encolado idéntico (reintento del cliente o invocación doble)
        var m2 = await servicioOutbox.EncolarMensajeAsync(
            OrgLestonnac,
            "RecordatorioPago",
            "familia@scout.es",
            "Familia Scout",
            "Recordatorio de pago",
            "<p>Aviso</p>",
            "Aviso",
            claveDeduplicacion: claveUnica);

        // Debe retornar la misma entidad y existir un único registro en la base de datos
        Assert.Equal(m1.Id, m2.Id);

        var total = await dbContext.MensajesOutbox
            .CountAsync(m => m.OrganizacionId == OrgLestonnac && m.ClaveDeduplicacion == claveUnica);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task ServicioOutbox_ConFalloReiterado_AplicaBackoffYMarcaFallidoTrasMaxReintentos()
    {
        var (dbContext, _) = CreateContext();
        _fakeEmail.DeberiaFallar = true;

        var servicioOutbox = new ServicioOutbox(dbContext, _fakeEmail, NullLogger<ServicioOutbox>.Instance);

        var mensaje = await servicioOutbox.EncolarMensajeAsync(
            OrgLestonnac,
            "AvisoCritico",
            "familia@scout.es",
            "Familia Scout",
            "Aviso Crítico",
            "<p>Aviso</p>",
            "Aviso",
            claveDeduplicacion: "fallo-maximo-test");

        mensaje.MaxReintentos = 3;
        await dbContext.SaveChangesAsync();

        // 3 ciclos de fallo simulando avance temporal
        for (int i = 0; i < 3; i++)
        {
            mensaje.ProximoReintentoUtc = DateTime.UtcNow.AddMinutes(-1);
            await dbContext.SaveChangesAsync();

            await servicioOutbox.ProcesarLotePendienteAsync();
        }

        var mensajeFinal = await dbContext.MensajesOutbox.FindAsync(mensaje.Id);
        Assert.NotNull(mensajeFinal);
        Assert.Equal(3, mensajeFinal.Reintentos);
        Assert.Equal(EstadoMensajeOutbox.Fallido, mensajeFinal.Estado);
    }

    [Theory]
    [InlineData("Asunto normal", "Asunto normal")]
    [InlineData("Asunto con\r\nBcc: hacker@bad.com", "Asunto con  Bcc: hacker@bad.com")]
    [InlineData("Cabecera\nInyectada\r\nTest", "Cabecera Inyectada  Test")]
    public void ServicioEmailSmtp_SanitizaCabecera_ContraInyeccionCRLF(string input, string esperado)
    {
        var resultado = ServicioEmailSmtp.SanitizarCabecera(input);
        Assert.Equal(esperado, resultado);
        Assert.DoesNotContain("\r", resultado);
        Assert.DoesNotContain("\n", resultado);
    }

    [Fact]
    public void PlantillasCorreo_Subsanacion_CumpleMinimizacionYZeroPii()
    {
        // RNF09: Sin documentos ni datos médicos en correos de aviso
        var plantilla = PlantillasCorreo.SubsanacionRequerida(
            "Pablo Scout",
            "Campamento de Verano",
            "El justificante adjunto está cortado y no se aprecia el importe.",
            "https://lestonnac.localhost/familias/inscripciones",
            "Grupo Scout Lestonnac 330");

        Assert.Contains("Pablo Scout", plantilla.CuerpoHtml);
        Assert.Contains("Campamento de Verano", plantilla.CuerpoHtml);
        Assert.Contains("Grupo Scout Lestonnac 330", plantilla.CuerpoHtml);
        Assert.Contains("https://lestonnac.localhost/familias/inscripciones", plantilla.CuerpoHtml);

        // Verificar que no vuelca datos sensibles prohibidos
        Assert.DoesNotContain("IBAN", plantilla.CuerpoHtml);
        Assert.DoesNotContain("alergia", plantilla.CuerpoHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("medicación", plantilla.CuerpoHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("enfermedad", plantilla.CuerpoHtml, StringComparison.OrdinalIgnoreCase);
    }
}
