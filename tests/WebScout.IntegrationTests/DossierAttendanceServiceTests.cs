using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Dossier.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class DossierAttendanceServiceTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public DossierAttendanceServiceTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();

        _options = _baseDatos.Opciones;
    }

    private static UsuarioScout CrearUsuario(Guid id, Guid organizacionId, string nombre, string email)
    {
        return new UsuarioScout
        {
            Id = id,
            OrganizacionId = organizacionId,
            NombreCompleto = nombre,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FechaCreacionUtc = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task ObtenerDossierActividad_CompilaMetricasYFicheroSanitarioCorrectamente()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenant);
        db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "María Pérez", "maria@test.es"));
        db.Educandos.Add(new Educando
        {
            Id = educandoId,
            OrganizacionId = OrgLestonnac,
            TutorLegalId = tutorId,
            Nombre = "Marcos",
            Apellidos = "García",
            RamaActual = RamaScout.Tropa,
            FechaNacimiento = new DateOnly(2014, 9, 20),
            Activo = true,
            FichaMedica = new FichaMedica
            {
                Id = Guid.NewGuid(),
                OrganizacionId = OrgLestonnac,
                NumeroSeguridadSocial = "CIP-123456",
                EntidadAseguradora = "SESCAM",
                EnfermedadesCronicas = "Asma bronquial",
                MedicacionActiva = "Ventolín",
                AlergiasAlimentarias = "Gluten",
                RegimenAlimenticio = "Celiaco",
                TelefonoEmergenciaPrincipal = "+34 600 111 222",
                NombreContactoEmergenciaPrincipal = "María Pérez",
                VacunacionAlDia = true,
                SabeNadar = true
            }
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Acampada de Tropa en Alcaraz",
            Lugar = "Albergue Los Batanes",
            Tipo = TipoActividad.Acampada,
            Estado = EstadoActividad.Publicada,
            Precio = 25.00m,
            FechaInicio = DateTime.UtcNow.AddDays(5),
            FechaFin = DateTime.UtcNow.AddDays(7)
        });
        db.Inscripciones.Add(new InscripcionActividad
        {
            Id = Guid.NewGuid(),
            OrganizacionId = OrgLestonnac,
            ActividadId = actividadId,
            EducandoId = educandoId,
            TutorLegalId = tutorId,
            Estado = EstadoInscripcion.Confirmada,
            ImporteEuros = 25.00m,
            AutorizacionFirmada = true
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioDossierAsistencia(db, tenant, NullLogger<ServicioDossierAsistencia>.Instance);

        // Act
        var dossier = await servicio.ObtenerDossierActividadAsync(actividadId);

        // Assert
        Assert.NotNull(dossier);
        Assert.Equal(1, dossier.TotalInscritos);
        Assert.Equal(1, dossier.TotalConfirmados);
        Assert.Equal(25.00m, dossier.TotalPresupuestadoEuros);
        Assert.Equal(25.00m, dossier.TotalRecaudadoEuros);
        Assert.Single(dossier.RegistrosSanitarios);
        Assert.Equal("CIP-123456", dossier.RegistrosSanitarios[0].NumeroSeguridadSocial);
        Assert.True(dossier.RegistrosSanitarios[0].TieneAlertasMedicas);
        Assert.Single(dossier.AlertasIntendencia);
        Assert.Equal("Gluten", dossier.AlertasIntendencia[0].AlergiasAlimentarias);
        Assert.Single(dossier.DirectorioEmergencia);
        Assert.Equal("+34 600 111 222", dossier.DirectorioEmergencia[0].TelefonoPrincipal);
    }

    [Fact]
    public async Task RegistrarCheckInSalida_MarcaPresenteYFechaUtc()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();
        var inscripcionId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenant);
        db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Carlos García", "carlos@test.es"));
        db.Educandos.Add(new Educando
        {
            Id = educandoId,
            OrganizacionId = OrgLestonnac,
            TutorLegalId = tutorId,
            Nombre = "Lucía",
            Apellidos = "García",
            RamaActual = RamaScout.Manada,
            FechaNacimiento = new DateOnly(2016, 5, 12),
            Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Salida Manada",
            Lugar = "Parque",
            Tipo = TipoActividad.Salida,
            Precio = 0m,
            FechaInicio = DateTime.UtcNow.AddDays(1),
            FechaFin = DateTime.UtcNow.AddDays(1).AddHours(4)
        });
        db.Inscripciones.Add(new InscripcionActividad
        {
            Id = inscripcionId,
            OrganizacionId = OrgLestonnac,
            ActividadId = actividadId,
            EducandoId = educandoId,
            TutorLegalId = tutorId,
            Estado = EstadoInscripcion.Confirmada,
            PresenteEnSalida = false
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioDossierAsistencia(db, tenant, NullLogger<ServicioDossierAsistencia>.Instance);

        // Act
        await servicio.RegistrarCheckInSalidaAsync(inscripcionId, true);

        // Assert
        var inscripcion = await db.Inscripciones.FirstAsync(i => i.Id == inscripcionId);
        Assert.True(inscripcion.PresenteEnSalida);
        Assert.NotNull(inscripcion.FechaCheckInSalidaUtc);
    }

    [Fact]
    public async Task RegistrarCheckInRegreso_MarcaEntregadoYFechaUtc()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();
        var inscripcionId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenant);
        db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Carlos García", "carlos@test.es"));
        db.Educandos.Add(new Educando
        {
            Id = educandoId,
            OrganizacionId = OrgLestonnac,
            TutorLegalId = tutorId,
            Nombre = "Lucía",
            Apellidos = "García",
            RamaActual = RamaScout.Manada,
            FechaNacimiento = new DateOnly(2016, 5, 12),
            Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Salida Manada",
            Lugar = "Parque",
            Tipo = TipoActividad.Salida,
            Precio = 0m,
            FechaInicio = DateTime.UtcNow.AddDays(1),
            FechaFin = DateTime.UtcNow.AddDays(1).AddHours(4)
        });
        db.Inscripciones.Add(new InscripcionActividad
        {
            Id = inscripcionId,
            OrganizacionId = OrgLestonnac,
            ActividadId = actividadId,
            EducandoId = educandoId,
            TutorLegalId = tutorId,
            Estado = EstadoInscripcion.Confirmada,
            PresenteEnRegreso = false
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioDossierAsistencia(db, tenant, NullLogger<ServicioDossierAsistencia>.Instance);

        // Act
        await servicio.RegistrarCheckInRegresoAsync(inscripcionId, true);

        // Assert
        var inscripcion = await db.Inscripciones.FirstAsync(i => i.Id == inscripcionId);
        Assert.True(inscripcion.PresenteEnRegreso);
        Assert.NotNull(inscripcion.FechaCheckInRegresoUtc);
    }

    [Fact]
    public async Task ActualizarObservacionAsistencia_PersisteNotaRecogida()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();
        var inscripcionId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenant);
        db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Carlos García", "carlos@test.es"));
        db.Educandos.Add(new Educando
        {
            Id = educandoId,
            OrganizacionId = OrgLestonnac,
            TutorLegalId = tutorId,
            Nombre = "Lucía",
            Apellidos = "García",
            RamaActual = RamaScout.Manada,
            FechaNacimiento = new DateOnly(2016, 5, 12),
            Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Salida Manada",
            Lugar = "Parque",
            Tipo = TipoActividad.Salida,
            Precio = 0m,
            FechaInicio = DateTime.UtcNow.AddDays(1),
            FechaFin = DateTime.UtcNow.AddDays(1).AddHours(4)
        });
        db.Inscripciones.Add(new InscripcionActividad
        {
            Id = inscripcionId,
            OrganizacionId = OrgLestonnac,
            ActividadId = actividadId,
            EducandoId = educandoId,
            TutorLegalId = tutorId,
            Estado = EstadoInscripcion.Confirmada
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioDossierAsistencia(db, tenant, NullLogger<ServicioDossierAsistencia>.Instance);

        // Act
        await servicio.ActualizarObservacionAsistenciaAsync(inscripcionId, "Recogida por los abuelos con autorización");

        // Assert
        var inscripcion = await db.Inscripciones.FirstAsync(i => i.Id == inscripcionId);
        Assert.Equal("Recogida por los abuelos con autorización", inscripcion.ObservacionesAsistencia);
    }

    [Fact]
    public async Task GenerarCsvAsistencia_GeneraCabecerasYRegistrosSeparadosPorPuntoYComa()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenant);
        db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Carlos García", "carlos@test.es"));
        db.Educandos.Add(new Educando
        {
            Id = educandoId,
            OrganizacionId = OrgLestonnac,
            TutorLegalId = tutorId,
            Nombre = "Lucía",
            Apellidos = "García",
            RamaActual = RamaScout.Manada,
            FechaNacimiento = new DateOnly(2016, 5, 12),
            Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Salida Manada",
            Lugar = "Parque",
            Tipo = TipoActividad.Salida,
            Precio = 10.00m,
            FechaInicio = DateTime.UtcNow.AddDays(1),
            FechaFin = DateTime.UtcNow.AddDays(1).AddHours(4)
        });
        db.Inscripciones.Add(new InscripcionActividad
        {
            Id = Guid.NewGuid(),
            OrganizacionId = OrgLestonnac,
            ActividadId = actividadId,
            EducandoId = educandoId,
            TutorLegalId = tutorId,
            Estado = EstadoInscripcion.Confirmada,
            ImporteEuros = 10.00m,
            PresenteEnSalida = true,
            FechaCheckInSalidaUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioDossierAsistencia(db, tenant, NullLogger<ServicioDossierAsistencia>.Instance);

        // Act
        var csv = await servicio.GenerarCsvAsistenciaAsync(actividadId);

        // Assert
        Assert.Contains("Educando;Rama;Edad;Tutor Legal;Telefono Emergencia Principal", csv);
        Assert.Contains("Lucía García", csv);
        Assert.Contains("Manada", csv);
        Assert.Contains("SI", csv);
    }

    [Fact]
    public async Task AislamientoMultitenant_DossierNoAccesibleDesdeOtroGrupoScout()
    {
        // Arrange
        var tenantLestonnac = new TenantContext();
        tenantLestonnac.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tenantSanVicente = new TenantContext();
        tenantSanVicente.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 412", 412);

        var actividadLestonnacId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenantLestonnac))
        {
            db.Actividades.Add(new ActividadScout
            {
                Id = actividadLestonnacId,
                OrganizacionId = OrgLestonnac,
                Titulo = "Actividad Privada Lestonnac",
                Lugar = "Base",
                Tipo = TipoActividad.Reunion,
                Precio = 0m,
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddHours(2)
            });
            await db.SaveChangesAsync();
        }

        // Act & Assert: Intentar consultar desde San Vicente debe lanzar excepción por no existir en su tenant
        using (var dbSanVicente = new WebScoutDbContext(_options, tenantSanVicente))
        {
            var servicioSanVicente = new ServicioDossierAsistencia(dbSanVicente, tenantSanVicente, NullLogger<ServicioDossierAsistencia>.Instance);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                servicioSanVicente.ObtenerDossierActividadAsync(actividadLestonnacId));
        }
    }

    [Fact]
    public async Task GenerarCsvAsistencia_ShouldSanitizeFormulas_PreventingCsvInjection()
    {
        var tenantLestonnac = new TenantContext();
        tenantLestonnac.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var actividadId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenantLestonnac))
        {
            var tutor = new UsuarioScout
            {
                Id = tutorId,
                OrganizacionId = OrgLestonnac,
                UserName = "tutor.inyeccion@scoutslestonnac.es",
                Email = "tutor.inyeccion@scoutslestonnac.es",
                NombreCompleto = "=cmd|' /C calc'!A0" // Fórmula maliciosa
            };
            db.Users.Add(tutor);

            var educando = new Educando
            {
                Id = educandoId,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorId,
                Nombre = "+2+5", // Fórmula maliciosa
                Apellidos = "@SUM(A1:A10)", // Fórmula maliciosa
                FechaNacimiento = new DateOnly(2014, 1, 1),
                Sexo = "M",
                RamaActual = RamaScout.Tropa,
                Activo = true
            };
            db.Educandos.Add(educando);

            var actividad = new ActividadScout
            {
                Id = actividadId,
                OrganizacionId = OrgLestonnac,
                Titulo = "Salida con Prueba de Inyección",
                Lugar = "Parque",
                Tipo = TipoActividad.Salida,
                Precio = 10m,
                FechaInicio = DateTime.UtcNow.AddDays(5),
                FechaFin = DateTime.UtcNow.AddDays(5).AddHours(4)
            };
            db.Actividades.Add(actividad);

            db.Inscripciones.Add(new InscripcionActividad
            {
                OrganizacionId = OrgLestonnac,
                ActividadId = actividadId,
                EducandoId = educandoId,
                TutorLegalId = tutorId,
                Estado = EstadoInscripcion.Confirmada,
                ImporteEuros = 10m,
                ObservacionesAsistencia = "-10-20" // Fórmula maliciosa
            });

            await db.SaveChangesAsync();
        }

        using (var db = new WebScoutDbContext(_options, tenantLestonnac))
        {
            var servicio = new ServicioDossierAsistencia(db, tenantLestonnac, NullLogger<ServicioDossierAsistencia>.Instance);
            var csv = await servicio.GenerarCsvAsistenciaAsync(actividadId);

            // Verificar que los campos con fórmula al inicio están prefijados con apóstrofe para neutralizar la ejecución
            Assert.Contains("'+2+5", csv);
            Assert.Contains("'=cmd", csv);
            Assert.Contains("'-10-20", csv);
        }
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
    }
}
