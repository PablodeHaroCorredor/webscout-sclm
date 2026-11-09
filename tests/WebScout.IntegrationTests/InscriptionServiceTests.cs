using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Inscriptions.Services;
using WebScout.Web.Features.Inscriptions.Storage;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class FakeAlmacenamientoJustificantes : IAlmacenamientoJustificantes
{
    public Dictionary<string, byte[]> Archivos { get; } = new();

    public async Task<string> GuardarJustificanteAsync(
        Guid organizacionId,
        Guid inscripcionId,
        string nombreArchivoOriginal,
        Stream streamContenido)
    {
        using var ms = new MemoryStream();
        await streamContenido.CopyToAsync(ms);
        var ruta = $"{organizacionId}/{inscripcionId}_{nombreArchivoOriginal}";
        Archivos[ruta] = ms.ToArray();
        return ruta;
    }

    public Task<(Stream Stream, string ContentType, string NombreDescarga)?> ObtenerJustificanteAsync(string rutaRelativa)
    {
        if (Archivos.TryGetValue(rutaRelativa, out var bytes))
        {
            var ms = new MemoryStream(bytes);
            return Task.FromResult<(Stream Stream, string ContentType, string NombreDescarga)?>((ms, "application/pdf", "justificante.pdf"));
        }
        return Task.FromResult<(Stream Stream, string ContentType, string NombreDescarga)?>(null);
    }

    public Task EliminarJustificanteAsync(string rutaRelativa)
    {
        Archivos.Remove(rutaRelativa);
        return Task.CompletedTask;
    }
}

public class InscriptionServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<WebScoutDbContext> _options;
    private readonly FakeAlmacenamientoJustificantes _fakeStorage;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public InscriptionServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<WebScoutDbContext>()
            .UseSqlite(_connection)
            .Options;

        _fakeStorage = new FakeAlmacenamientoJustificantes();

        var initContext = new TenantContext();
        using var db = new WebScoutDbContext(_options, initContext);
        db.Database.EnsureCreated();
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
    public async Task InscribirEducando_ActividadGratuita_QuedaConfirmadaInmediatamente()
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
            Titulo = "Jornada de Juegos en el Parque",
            Lugar = "Parque Abelardo Sánchez",
            Tipo = TipoActividad.Salida,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Manada],
            Precio = 0.00m,
            FechaInicio = DateTime.UtcNow.AddDays(5),
            FechaFin = DateTime.UtcNow.AddDays(5).AddHours(4)
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);

        // Act
        var inscripcion = await servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId, "Primera actividad gratuita");

        // Assert
        Assert.NotNull(inscripcion);
        Assert.Equal(EstadoInscripcion.Confirmada, inscripcion.Estado);
        Assert.Equal(0.00m, inscripcion.ImporteEuros);
        Assert.True(inscripcion.AutorizacionFirmada);
        Assert.False(inscripcion.TieneJustificanteSubido);
    }

    [Fact]
    public async Task InscribirEducando_ActividadConCuota_QuedaPendienteJustificante()
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
            Nombre = "Marcos",
            Apellidos = "García",
            RamaActual = RamaScout.Tropa,
            FechaNacimiento = new DateOnly(2014, 9, 20),
            Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Acampada de Tropa",
            Lugar = "Sierra de Alcaraz",
            Tipo = TipoActividad.Acampada,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Tropa],
            Precio = 28.50m,
            IbanCuentaBancaria = "ES91 2100 0418 4502 0005 1330",
            FechaInicio = DateTime.UtcNow.AddDays(15),
            FechaFin = DateTime.UtcNow.AddDays(17)
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);

        // Act
        var inscripcion = await servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId);

        // Assert
        Assert.NotNull(inscripcion);
        Assert.Equal(EstadoInscripcion.PendienteJustificante, inscripcion.Estado);
        Assert.Equal(28.50m, inscripcion.ImporteEuros);
        Assert.True(inscripcion.AutorizacionFirmada);
        Assert.False(inscripcion.TieneJustificanteSubido);
    }

    [Fact]
    public async Task InscribirEducando_RamaNoCoincidente_LanzaExcepcion()
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
            RamaActual = RamaScout.Castores, // Rama Castores
            FechaNacimiento = new DateOnly(2018, 5, 12),
            Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Salida de Manada",
            Lugar = "Laguna del Arquillo",
            Tipo = TipoActividad.Salida,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Manada], // Convocada solo para Manada
            Precio = 12.00m,
            FechaInicio = DateTime.UtcNow.AddDays(10),
            FechaFin = DateTime.UtcNow.AddDays(10).AddHours(8)
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId));

        Assert.Contains("no está dirigida a la rama", ex.Message);
    }

    [Fact]
    public async Task InscribirEducando_InscripcionDuplicada_LanzaExcepcion()
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
            Lugar = "Albacete",
            Tipo = TipoActividad.Salida,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Manada],
            Precio = 10.00m,
            FechaInicio = DateTime.UtcNow.AddDays(10),
            FechaFin = DateTime.UtcNow.AddDays(10).AddHours(6)
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);

        // First enrollment succeeds
        await servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId);

        // Second enrollment must fail
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId));

        Assert.Contains("ya cuenta con una inscripción activa", ex.Message);
    }

    [Fact]
    public async Task SubirJustificantePago_TransicionaA_EnRevision()
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
            Nombre = "Marcos",
            Apellidos = "García",
            RamaActual = RamaScout.Tropa,
            FechaNacimiento = new DateOnly(2014, 9, 20),
            Activo = true
        });
        db.Actividades.Add(new ActividadScout
        {
            Id = actividadId,
            OrganizacionId = OrgLestonnac,
            Titulo = "Acampada Tropa",
            Lugar = "Sierra",
            Tipo = TipoActividad.Acampada,
            Estado = EstadoActividad.Publicada,
            Precio = 30.00m,
            FechaInicio = DateTime.UtcNow.AddDays(20),
            FechaFin = DateTime.UtcNow.AddDays(22)
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
        var inscripcion = await servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId);

        Assert.Equal(EstadoInscripcion.PendienteJustificante, inscripcion.Estado);

        // Act: Subida del justificante de pago en PDF
        var dummyBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        using var stream = new MemoryStream(dummyBytes);
        await servicio.SubirJustificantePagoAsync(inscripcion.Id, "justificante_banco.pdf", stream);

        // Assert
        var inscripcionActualizada = await servicio.ObtenerPorIdAsync(inscripcion.Id);
        Assert.NotNull(inscripcionActualizada);
        Assert.Equal(EstadoInscripcion.EnRevision, inscripcionActualizada.Estado);
        Assert.Equal("justificante_banco.pdf", inscripcionActualizada.NombreArchivoJustificante);
        Assert.NotNull(inscripcionActualizada.RutaArchivoJustificante);
        Assert.NotNull(inscripcionActualizada.FechaSubidaJustificanteUtc);
        Assert.True(inscripcionActualizada.TieneJustificanteSubido);
    }

    [Fact]
    public async Task RevisarInscripcion_PorKraal_CambiaEstadoAConfirmada()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);
        var tutorId = Guid.NewGuid();
        var scouterId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenant);
        db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Carlos García", "carlos@test.es"));
        db.Users.Add(CrearUsuario(scouterId, OrgLestonnac, "Elena Akela", "akela@test.es"));
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
            Lugar = "Albacete",
            Tipo = TipoActividad.Salida,
            Estado = EstadoActividad.Publicada,
            Precio = 15.00m,
            FechaInicio = DateTime.UtcNow.AddDays(10),
            FechaFin = DateTime.UtcNow.AddDays(10).AddHours(6)
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
        var inscripcion = await servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId);

        // Act: Scouter valida el pago
        await servicio.RevisarInscripcionAsync(inscripcion.Id, EstadoInscripcion.Confirmada, "Comprobante verificado", scouterId);

        // Assert
        var inscripcionActualizada = await servicio.ObtenerPorIdAsync(inscripcion.Id);
        Assert.NotNull(inscripcionActualizada);
        Assert.Equal(EstadoInscripcion.Confirmada, inscripcionActualizada.Estado);
        Assert.Equal("Comprobante verificado", inscripcionActualizada.NotasKraal);
        Assert.Equal(scouterId, inscripcionActualizada.RevisadoPorUsuarioId);
        Assert.NotNull(inscripcionActualizada.FechaRevisionUtc);
    }

    [Fact]
    public async Task CancelarInscripcion_PorTutorLegal_CambiaEstadoACancelada()
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
            Lugar = "Albacete",
            Tipo = TipoActividad.Salida,
            Estado = EstadoActividad.Publicada,
            Precio = 0.00m,
            FechaInicio = DateTime.UtcNow.AddDays(10),
            FechaFin = DateTime.UtcNow.AddDays(10).AddHours(6)
        });
        await db.SaveChangesAsync();

        var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
        var inscripcion = await servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId);

        // Act
        await servicio.CancelarInscripcionAsync(inscripcion.Id, tutorId);

        // Assert
        var cancelada = await servicio.ObtenerPorIdAsync(inscripcion.Id);
        Assert.NotNull(cancelada);
        Assert.Equal(EstadoInscripcion.Cancelada, cancelada.Estado);
    }

    [Fact]
    public async Task AislamientoMultitenant_InscripcionesNoVisiblesEntreGruposScout()
    {
        // Arrange
        var tenantLestonnac = new TenantContext();
        tenantLestonnac.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tenantSanVicente = new TenantContext();
        tenantSanVicente.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 412", 412);

        var tutorLestonnacId = Guid.NewGuid();
        var educandoLestonnacId = Guid.NewGuid();
        var actividadLestonnacId = Guid.NewGuid();

        using (var dbLestonnac = new WebScoutDbContext(_options, tenantLestonnac))
        {
            dbLestonnac.Users.Add(CrearUsuario(tutorLestonnacId, OrgLestonnac, "Carlos García", "carlos@test.es"));
            dbLestonnac.Educandos.Add(new Educando
            {
                Id = educandoLestonnacId,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorLestonnacId,
                Nombre = "Lucía",
                Apellidos = "García",
                RamaActual = RamaScout.Manada,
                FechaNacimiento = new DateOnly(2016, 5, 12),
                Activo = true
            });
            dbLestonnac.Actividades.Add(new ActividadScout
            {
                Id = actividadLestonnacId,
                OrganizacionId = OrgLestonnac,
                Titulo = "Salida Manada Lestonnac",
                Lugar = "Masegoso",
                Tipo = TipoActividad.Salida,
                Estado = EstadoActividad.Publicada,
                Precio = 10.00m,
                FechaInicio = DateTime.UtcNow.AddDays(10),
                FechaFin = DateTime.UtcNow.AddDays(10).AddHours(6)
            });
            await dbLestonnac.SaveChangesAsync();

            var servicioLestonnac = new ServicioInscripciones(dbLestonnac, tenantLestonnac, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
            await servicioLestonnac.InscribirEducandoAsync(actividadLestonnacId, educandoLestonnacId, tutorLestonnacId);
        }

        // Act: Consultar desde San Vicente 412
        using (var dbSanVicente = new WebScoutDbContext(_options, tenantSanVicente))
        {
            var servicioSanVicente = new ServicioInscripciones(dbSanVicente, tenantSanVicente, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
            var inscripcionesActividad = await servicioSanVicente.ObtenerInscripcionesActividadAsync(actividadLestonnacId);
            var inscripcionesFamilia = await servicioSanVicente.ObtenerInscripcionesFamiliaAsync(tutorLestonnacId);

            // Assert: Multitenancy filter completely isolates the data
            Assert.Empty(inscripcionesActividad);
            Assert.Empty(inscripcionesFamilia);
        }
    }

    [Fact]
    public async Task PA02_MesaKraal_ScouterLimitadoASuSeccion_SoloObtieneInscripcionesDeSuRama()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var lobatoId = Guid.NewGuid();
        var scoutId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenant))
        {
            db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Familia Test", "tutor@test.es"));
            
            // Educando de Manada
            db.Educandos.Add(new Educando
            {
                Id = lobatoId,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorId,
                Nombre = "Hugo",
                Apellidos = "Test",
                RamaActual = RamaScout.Manada,
                FechaNacimiento = new DateOnly(2015, 3, 10),
                Activo = true
            });

            // Educando de Tropa
            db.Educandos.Add(new Educando
            {
                Id = scoutId,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorId,
                Nombre = "Lucía",
                Apellidos = "Test",
                RamaActual = RamaScout.Tropa,
                FechaNacimiento = new DateOnly(2012, 7, 20),
                Activo = true
            });

            // Actividad conjunta para Manada y Tropa
            var act = new ActividadScout
            {
                Id = actividadId,
                OrganizacionId = OrgLestonnac,
                Titulo = "Campamento de Primavera",
                Lugar = "Riópar",
                Tipo = TipoActividad.Campamento,
                Estado = EstadoActividad.Publicada,
                Precio = 30.00m,
                FechaInicio = DateTime.UtcNow.AddDays(15),
                FechaFin = DateTime.UtcNow.AddDays(18)
            };
            act.RamasDestinatarias = new List<RamaScout> { RamaScout.Manada, RamaScout.Tropa };
            db.Actividades.Add(act);
            await db.SaveChangesAsync();

            var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
            await servicio.InscribirEducandoAsync(actividadId, lobatoId, tutorId);
            await servicio.InscribirEducandoAsync(actividadId, scoutId, tutorId);
        }

        // Act & Assert: Scouter de Manada solo obtiene la inscripción de Hugo (PA02)
        using (var db = new WebScoutDbContext(_options, tenant))
        {
            var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
            var inscripcionesManada = await servicio.ObtenerInscripcionesMesaKraalAsync(ramaFiltro: RamaScout.Manada);
            var inscripcionesTropa = await servicio.ObtenerInscripcionesMesaKraalAsync(ramaFiltro: RamaScout.Tropa);
            var inscripcionesCoordinacion = await servicio.ObtenerInscripcionesMesaKraalAsync(ramaFiltro: null);

            Assert.Single(inscripcionesManada);
            Assert.Equal("Hugo", inscripcionesManada[0].Educando?.Nombre);
            Assert.Equal(RamaScout.Manada, inscripcionesManada[0].Educando?.RamaActual);

            Assert.Single(inscripcionesTropa);
            Assert.Equal("Lucía", inscripcionesTropa[0].Educando?.Nombre);
            Assert.Equal(RamaScout.Tropa, inscripcionesTropa[0].Educando?.RamaActual);

            Assert.Equal(2, inscripcionesCoordinacion.Count);
        }
    }

    [Fact]
    public async Task PA04_DosHermanos_JustificanteCompartido_SubsanacionEnUnoConservaElOtro()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var scouterId = Guid.NewGuid();
        var hermano1Id = Guid.NewGuid();
        var hermano2Id = Guid.NewGuid();
        var actividadId = Guid.NewGuid();
        Guid inscripcion1Id;
        Guid inscripcion2Id;

        using (var db = new WebScoutDbContext(_options, tenant))
        {
            db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Familia Gómez", "gomez@test.es"));
            db.Users.Add(CrearUsuario(scouterId, OrgLestonnac, "Elena Akela", "akela@test.es"));

            db.Educandos.Add(new Educando
            {
                Id = hermano1Id,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorId,
                Nombre = "Pablo",
                Apellidos = "Gómez",
                RamaActual = RamaScout.Manada,
                FechaNacimiento = new DateOnly(2015, 1, 1),
                Activo = true
            });

            db.Educandos.Add(new Educando
            {
                Id = hermano2Id,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorId,
                Nombre = "Sara",
                Apellidos = "Gómez",
                RamaActual = RamaScout.Manada,
                FechaNacimiento = new DateOnly(2016, 2, 2),
                Activo = true
            });

            var act = new ActividadScout
            {
                Id = actividadId,
                OrganizacionId = OrgLestonnac,
                Titulo = "Acampada de San Jorge",
                Lugar = "Alcaraz",
                Tipo = TipoActividad.Acampada,
                Estado = EstadoActividad.Publicada,
                Precio = 25.00m,
                FechaInicio = DateTime.UtcNow.AddDays(20),
                FechaFin = DateTime.UtcNow.AddDays(22)
            };
            act.RamasDestinatarias = new List<RamaScout> { RamaScout.Manada };
            db.Actividades.Add(act);
            await db.SaveChangesAsync();

            var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);
            var ins1 = await servicio.InscribirEducandoAsync(actividadId, hermano1Id, tutorId);
            var ins2 = await servicio.InscribirEducandoAsync(actividadId, hermano2Id, tutorId);
            inscripcion1Id = ins1.Id;
            inscripcion2Id = ins2.Id;

            // Subir justificante común
            using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            await servicio.SubirJustificantePagoAsync(inscripcion1Id, "transferencia_50euros.pdf", stream);

            // Reutilizar el mismo justificante para el hermano 2
            var ins2Db = await db.Inscripciones.FindAsync(inscripcion2Id);
            ins2Db!.RutaArchivoJustificante = ins1.RutaArchivoJustificante;
            ins2Db.NombreArchivoJustificante = "transferencia_50euros.pdf";
            ins2Db.Estado = EstadoInscripcion.EnRevision;
            await db.SaveChangesAsync();
        }

        // Act: El scouter valida a Sara (Hermano 2) pero solicita subsanación a Pablo (Hermano 1)
        using (var db = new WebScoutDbContext(_options, tenant))
        {
            var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);

            // Confirmar a Sara
            await servicio.RevisarInscripcionAsync(inscripcion2Id, EstadoInscripcion.Confirmada, "Plaza confirmada.", scouterId);

            // Solicitar subsanación a Pablo
            await servicio.RevisarInscripcionAsync(inscripcion1Id, EstadoInscripcion.RequiereSubsanacion, "Falta indicar DNI del tutor en el concepto.", scouterId);
        }

        // Assert (PA04): La subsanación de uno NO altera el estado confirmado del otro
        using (var db = new WebScoutDbContext(_options, tenant))
        {
            var ins1 = await db.Inscripciones.FindAsync(inscripcion1Id);
            var ins2 = await db.Inscripciones.FindAsync(inscripcion2Id);

            Assert.NotNull(ins1);
            Assert.NotNull(ins2);
            Assert.Equal(EstadoInscripcion.RequiereSubsanacion, ins1.Estado);
            Assert.Equal("Falta indicar DNI del tutor en el concepto.", ins1.NotasKraal);

            Assert.Equal(EstadoInscripcion.Confirmada, ins2.Estado);
            Assert.Equal("Plaza confirmada.", ins2.NotasKraal);
            Assert.Equal(ins1.RutaArchivoJustificante, ins2.RutaArchivoJustificante);
        }
    }

    [Fact]
    public async Task PA05_InscripcionFueraDePlazo_EsRechazadaSinCrearRegistrosParciales()
    {
        // Arrange
        var tenant = new TenantContext();
        tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenant))
        {
            db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Tutor Plazo", "plazo@test.es"));
            db.Educandos.Add(new Educando
            {
                Id = educandoId,
                OrganizacionId = OrgLestonnac,
                TutorLegalId = tutorId,
                Nombre = "Carlos",
                Apellidos = "Plazo",
                RamaActual = RamaScout.Castores,
                FechaNacimiento = new DateOnly(2018, 5, 5),
                Activo = true
            });

            // Actividad cuyo plazo venció ayer
            var act = new ActividadScout
            {
                Id = actividadId,
                OrganizacionId = OrgLestonnac,
                Titulo = "Salida Castores Vencida",
                Lugar = "Los Chorros",
                Tipo = TipoActividad.Salida,
                Estado = EstadoActividad.Publicada,
                Precio = 15.00m,
                FechaInicio = DateTime.UtcNow.AddDays(5),
                FechaFin = DateTime.UtcNow.AddDays(6),
                FechaLimiteInscripcion = DateTime.UtcNow.AddDays(-1) // Plazo expirado
            };
            act.RamasDestinatarias = new List<RamaScout> { RamaScout.Castores };
            db.Actividades.Add(act);
            await db.SaveChangesAsync();
        }

        // Act & Assert (PA05): Se rechaza con InvalidOperationException y no se guarda ninguna inscripción
        using (var db = new WebScoutDbContext(_options, tenant))
        {
            var servicio = new ServicioInscripciones(db, tenant, _fakeStorage, NullLogger<ServicioInscripciones>.Instance);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                servicio.InscribirEducandoAsync(actividadId, educandoId, tutorId));

            Assert.Contains("ha expirado", ex.Message, StringComparison.OrdinalIgnoreCase);

            var inscripcionesEnDb = await db.Inscripciones.CountAsync();
            Assert.Equal(0, inscripcionesEnDb);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
