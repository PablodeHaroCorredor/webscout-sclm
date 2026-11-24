using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Administration.Domain;
using WebScout.Web.Features.Administration.Services;
using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Admissions.Services;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Dossier.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

/// <summary>
/// Reglas corregidas en la fase F1 de la auditoría UI/UX de gestión (docs/auditoria-ui-ux-gestion-2026-11-24.md).
/// </summary>
public class AlertasSanitariasTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Sin alergias alimentarias conocidas")]
    [InlineData("Ninguna")]
    [InlineData("ninguna conocida.")]
    [InlineData("No")]
    [InlineData("N/A")]
    [InlineData("No tiene alergias")]
    [InlineData("Sin medicación")]
    public void EsRelevante_RespuestasNegativas_NoSonAlerta(string? texto)
    {
        Assert.False(AlertasSanitarias.EsRelevante(texto));
    }

    [Theory]
    [InlineData("Alérgica a los frutos secos (cacahuete y nuez)")]
    [InlineData("Sin gluten")]
    [InlineData("Penicilina")]
    [InlineData("No tolera la lactosa")]
    public void EsRelevante_DatosClinicosReales_SonAlerta(string texto)
    {
        Assert.True(AlertasSanitarias.EsRelevante(texto));
    }

    [Fact]
    public void ObtenerAlertasSanitarias_IncluyeAlergiasMedicamentosasYAmbientalesYOrdenaPorGravedad()
    {
        var ficha = new FichaMedica
        {
            NumeroSeguridadSocial = "CIP-1",
            TelefonoEmergenciaPrincipal = "600000000",
            AlergiasAlimentarias = "Sin alergias alimentarias conocidas",
            AlergiasMedicamentosas = "Penicilina",
            AlergiasAmbientales = "Picadura de avispa",
            EnfermedadesCronicas = "Asma",
            RegimenAlimenticio = "Ninguno"
        };

        var alertas = ficha.ObtenerAlertasSanitarias();

        Assert.Equal(
            [TipoAlertaSanitaria.AlergiaMedicamentosa, TipoAlertaSanitaria.AlergiaAmbiental, TipoAlertaSanitaria.EnfermedadCronica],
            alertas.Select(a => a.Tipo));
        Assert.Equal(2, alertas.Count(a => a.EsAlergia));
    }
}

public class HoraEspanaTests
{
    [Fact]
    public void AUtc_YAHoraEspana_SonInversas_EnInviernoYVerano()
    {
        var invierno = new DateTime(2026, 12, 14, 9, 0, 0);
        var verano = new DateTime(2027, 7, 15, 10, 0, 0);

        Assert.Equal(new DateTime(2026, 12, 14, 8, 0, 0, DateTimeKind.Utc), HoraEspana.AUtc(invierno));
        Assert.Equal(new DateTime(2027, 7, 15, 8, 0, 0, DateTimeKind.Utc), HoraEspana.AUtc(verano));

        // Guardar lo que se ha cargado no desplaza la hora (antes sumaba 1-2 h en cada edición).
        Assert.Equal(invierno, HoraEspana.AUtc(invierno).AHoraEspana());
        Assert.Equal(verano, HoraEspana.AUtc(verano).AHoraEspana());
    }
}

public class ValidadorAjustesGrupoTests
{
    [Theory]
    [InlineData("coordinacion@scoutslestonnac.es", true)]
    [InlineData("Grupo Scout Lestonnac 330", false)]
    [InlineData("kraal@localhost", false)]
    [InlineData(null, true)]
    public void ValidarEmail(string? email, bool valido) =>
        Assert.Equal(valido, ValidadorAjustesGrupo.ValidarEmail(email) is null);

    [Theory]
    [InlineData("ES91 2100 0418 4502 0005 1332", true)]
    [InlineData("ES91 2100 0418 4502 0005 1333", false)]
    [InlineData("ES91 2100", false)]
    public void ValidarIban(string iban, bool valido) =>
        Assert.Equal(valido, ValidadorAjustesGrupo.ValidarIban(iban) is null);

    [Theory]
    [InlineData("G02123456", true)]
    [InlineData("g-02123456", true)]
    [InlineData("12345678Z", false)]
    public void ValidarCif(string cif, bool valido) =>
        Assert.Equal(valido, ValidadorAjustesGrupo.ValidarCif(cif) is null);

    [Fact]
    public void ValidarRonda_FinAnteriorAInicio_EsError()
    {
        Assert.NotNull(ValidadorAjustesGrupo.ValidarRonda(new DateOnly(2027, 7, 31), new DateOnly(2026, 10, 1)));
        Assert.Null(ValidadorAjustesGrupo.ValidarRonda(new DateOnly(2026, 10, 1), new DateOnly(2027, 7, 31)));
    }
}

public class AuditoriaUiGestionF1ServiceTests : IDisposable
{
    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    public AuditoriaUiGestionF1ServiceTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();
        _options = _baseDatos.Opciones;

        using var db = new WebScoutDbContext(_options, CrearTenant());
        db.GruposScout.Add(new GrupoScout
        {
            Id = OrgLestonnac,
            Nombre = "Grupo Scout Lestonnac 330",
            Slug = "lestonnac",
            NumeroGrupo = 330,
            EmailContacto = "coordinacion@lestonnac.org",
            Activo = true,
            Paleta = PaletteConfig.DefaultLestonnac
        });
        db.Roles.Add(new RolScout { Id = Guid.NewGuid(), Name = "Familia", NormalizedName = "FAMILIA", ConcurrencyStamp = Guid.NewGuid().ToString() });
        db.SaveChanges();
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
    }

    private static TenantContext CrearTenant()
    {
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);
        return tenant;
    }

    private (WebScoutDbContext Db, ServicioConfiguracionGrupo Config, ServicioAdmisiones Admisiones) CrearServicios()
    {
        var tenant = CrearTenant();
        var db = new WebScoutDbContext(_options, tenant);
        var auditoria = new ServicioAuditoria(db, tenant, new Microsoft.AspNetCore.Http.HttpContextAccessor(), NullLogger<ServicioAuditoria>.Instance);
        var outbox = new ServicioOutbox(db, new FakeServicioEmail(), NullLogger<ServicioOutbox>.Instance);
        return (db,
            new ServicioConfiguracionGrupo(db, tenant, auditoria, NullLogger<ServicioConfiguracionGrupo>.Instance),
            new ServicioAdmisiones(db, tenant, outbox, NullLogger<ServicioAdmisiones>.Instance, AlcanceFijo.Global));
    }

    [Fact]
    public async Task ObtenerConfiguracion_NuevaConfiguracion_UsaElEmailDeContactoDelGrupo()
    {
        var (_, config, _) = CrearServicios();

        var resultado = await config.ObtenerConfiguracionAsync();

        Assert.Equal("coordinacion@lestonnac.org", resultado.EmailOficial);
    }

    [Fact]
    public async Task ObtenerConfiguracion_EmailGuardadoConElNombreDelGrupo_SeCorrige()
    {
        var (db, config, _) = CrearServicios();
        db.ConfiguracionesGrupo.Add(new ConfiguracionGrupo { OrganizacionId = OrgLestonnac, EmailOficial = "Grupo Scout Lestonnac 330" });
        await db.SaveChangesAsync();

        var resultado = await config.ObtenerConfiguracionAsync();

        Assert.Equal("coordinacion@lestonnac.org", resultado.EmailOficial);
    }

    [Fact]
    public async Task GuardarAreaEconomica_IbanIncorrecto_SeRechazaSinGuardar()
    {
        var (_, config, _) = CrearServicios();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            config.GuardarAreaEconomicaAsync(new AjustesEconomicosDto("ES91 2100 0418 4502 0005 1333", "Grupo", null)));

        Assert.Contains("IBAN", ex.Message);
        Assert.Null((await config.ObtenerConfiguracionAsync()).IbanCuentaBancaria);
    }

    [Fact]
    public async Task GuardarAreaInstitucional_FinDeRondaAnterior_SeRechaza()
    {
        var (_, config, _) = CrearServicios();

        await Assert.ThrowsAsync<InvalidOperationException>(() => config.GuardarAreaInstitucionalAsync(new AjustesInstitucionalesDto(
            null, null, null, "info@lestonnac.org", "2026/2027", new DateOnly(2027, 7, 31), new DateOnly(2026, 10, 1))));
    }

    [Fact]
    public async Task ConvertirSolicitud_DosVeces_NoDuplicaAlEducando()
    {
        var (db, _, admisiones) = CrearServicios();
        var solicitud = await admisiones.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            NombreEducando: "Nora",
            ApellidosEducando: "Ruiz Sintética",
            FechaNacimiento: new DateOnly(2016, 3, 2),
            NombreTutor: "Tutor Sintético",
            RelacionTutor: "Madre",
            EmailTutor: "tutor.sintetico@example.com",
            TelefonoTutor: "600000001",
            Observaciones: null,
            ConsentimientoPrivacidad: true,
            ConsentimientoComunicaciones: false));

        await admisiones.ConvertirSolicitudEnEducandoAsync(solicitud.Id, RamaScout.Manada, null, Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            admisiones.ConvertirSolicitudEnEducandoAsync(solicitud.Id, RamaScout.Manada, null, Guid.NewGuid()));

        Assert.Contains("ya figura en el censo", ex.Message);
        Assert.Equal(1, await db.Educandos.CountAsync(e => e.Nombre == "Nora"));
    }

    [Fact]
    public async Task ConvertirSolicitud_Rechazada_SeRechaza()
    {
        var (db, _, admisiones) = CrearServicios();
        var solicitud = await admisiones.RegistrarSolicitudAdmisionAsync(new RegistrarSolicitudAdmisionDto(
            "Iker", "Sintético", new DateOnly(2015, 1, 1), "Tutor", "Padre", "iker.tutor@example.com", "600000002", null, true, false));
        solicitud.Estado = EstadoSolicitudAdmision.Rechazada;
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            admisiones.ConvertirSolicitudEnEducandoAsync(solicitud.Id, RamaScout.Tropa, null, Guid.NewGuid()));
        Assert.False(await db.Educandos.AnyAsync(e => e.Nombre == "Iker"));
    }

    [Fact]
    public async Task Dossier_SinFichaMedica_NoInventaDatos_YConFicha_MuestraTodasLasAlergias()
    {
        var tenant = CrearTenant();
        using var db = new WebScoutDbContext(_options, tenant);
        var tutorId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();
        var conFichaId = Guid.NewGuid();
        var sinFichaId = Guid.NewGuid();

        db.Users.Add(new UsuarioScout
        {
            Id = tutorId, OrganizacionId = OrgLestonnac, NombreCompleto = "Tutor Sintético",
            UserName = "t@example.com", NormalizedUserName = "T@EXAMPLE.COM", Email = "t@example.com", NormalizedEmail = "T@EXAMPLE.COM"
        });
        db.Educandos.Add(new Educando
        {
            Id = conFichaId, OrganizacionId = OrgLestonnac, TutorLegalId = tutorId, Nombre = "Ana", Apellidos = "Sintética",
            RamaActual = RamaScout.Tropa, FechaNacimiento = new DateOnly(2014, 1, 1), Activo = true,
            FichaMedica = new FichaMedica
            {
                Id = Guid.NewGuid(), OrganizacionId = OrgLestonnac, NumeroSeguridadSocial = "CIP-9", TelefonoEmergenciaPrincipal = "600000003",
                AlergiasAlimentarias = "Sin alergias alimentarias conocidas", AlergiasMedicamentosas = "Ibuprofeno"
            }
        });
        db.Educandos.Add(new Educando
        {
            Id = sinFichaId, OrganizacionId = OrgLestonnac, TutorLegalId = tutorId, Nombre = "Leo", Apellidos = "Sintético",
            RamaActual = RamaScout.Tropa, FechaNacimiento = new DateOnly(2014, 2, 2), Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId, OrganizacionId = OrgLestonnac, Titulo = "Salida sintética", Lugar = "Sierra", Tipo = TipoActividad.Salida,
            Estado = EstadoActividad.Publicada, Precio = 10m, FechaInicio = DateTime.UtcNow.AddDays(3), FechaFin = DateTime.UtcNow.AddDays(4)
        });
        foreach (var educandoId in new[] { conFichaId, sinFichaId })
        {
            db.Inscripciones.Add(new InscripcionActividad
            {
                Id = Guid.NewGuid(), OrganizacionId = OrgLestonnac, ActividadId = actividadId, EducandoId = educandoId,
                TutorLegalId = tutorId, Estado = EstadoInscripcion.Confirmada, ImporteEuros = 10m, AutorizacionFirmada = true
            });
        }
        await db.SaveChangesAsync();

        var dossier = await new ServicioDossierAsistencia(db, tenant, NullLogger<ServicioDossierAsistencia>.Instance, AlcanceFijo.Global)
            .ObtenerDossierActividadAsync(actividadId);

        var sinFicha = dossier.RegistrosSanitarios.Single(r => r.EducandoId == sinFichaId);
        Assert.False(sinFicha.TieneFicha);
        Assert.Null(sinFicha.EntidadAseguradora);
        Assert.Null(sinFicha.SabeNadar);
        Assert.Null(sinFicha.VacunacionAlDia);

        var conFicha = dossier.RegistrosSanitarios.Single(r => r.EducandoId == conFichaId);
        Assert.Equal([TipoAlertaSanitaria.AlergiaMedicamentosa], conFicha.Alergias.Select(a => a.Tipo));

        // "Sin alergias alimentarias conocidas" no es una alerta para cocina.
        Assert.Empty(dossier.AlertasIntendencia);
    }
}
