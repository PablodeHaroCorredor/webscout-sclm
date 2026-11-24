using Microsoft.EntityFrameworkCore;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Activities.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class ActivityServiceTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ActivityServiceTests()
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
    public async Task CrearActividad_ConPrecioEnEurosYCentimos_GuardaCorrectamente()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);
        var scouterId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenantContext);
        db.Users.Add(CrearUsuario(scouterId, OrgLestonnac, "Elena Akela", "akela@test.es"));
        await db.SaveChangesAsync();

        var servicio = new ServicioActividades(db, tenantContext);

        var actividad = new ActividadScout
        {
            Titulo = "Acampada de Otoño",
            Descripcion = "Fin de semana en la sierra",
            Lugar = "Sierra de Alcaraz",
            Tipo = TipoActividad.Acampada,
            Estado = EstadoActividad.Borrador,
            RamasDestinatarias = [RamaScout.Manada, RamaScout.Tropa],
            FechaInicio = DateTime.UtcNow.AddDays(10),
            FechaFin = DateTime.UtcNow.AddDays(12),
            FechaLimiteInscripcion = DateTime.UtcNow.AddDays(5),
            Precio = 18.50m, // Euros con céntimos
            IbanCuentaBancaria = "ES91 2100 0000 0000 0000 0000",
            ConceptoPagoSugerido = "Acampada Otoño - Nombre"
        };

        // Act
        var creada = await servicio.CrearActividadAsync(actividad, scouterId);

        // Assert
        Assert.NotEqual(Guid.Empty, creada.Id);
        Assert.Equal(OrgLestonnac, creada.OrganizacionId);
        Assert.Equal(18.50m, creada.Precio);
        Assert.Equal(1850, creada.PrecioEnCentimos); // retrocompatibilidad
        Assert.Equal(2, creada.RamasDestinatarias.Count);
        Assert.Contains(RamaScout.Manada, creada.RamasDestinatarias);
        Assert.Contains(RamaScout.Tropa, creada.RamasDestinatarias);
    }

    [Fact]
    public async Task ObtenerActividadesParaFamilia_SoloMuestraActividadesDeSusRamasOCompletas()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);
        var tutorId = Guid.NewGuid();

        using var db = new WebScoutDbContext(_options, tenantContext);
        db.Users.Add(CrearUsuario(tutorId, OrgLestonnac, "Carlos García", "carlos@test.es"));
        await db.SaveChangesAsync();

        // Crear educando de la familia en Rama Manada
        var educando = new Educando
        {
            OrganizacionId = OrgLestonnac,
            TutorLegalId = tutorId,
            Nombre = "Lucía",
            Apellidos = "García",
            FechaNacimiento = new DateOnly(2017, 5, 1),
            RamaActual = RamaScout.Manada,
            Activo = true
        };
        db.Educandos.Add(educando);

        // Actividad 1: Para Manada (Publicada) -> Debe verse
        var actManada = new ActividadScout
        {
            OrganizacionId = OrgLestonnac,
            Titulo = "Salida Manada",
            Lugar = "Riópar",
            Tipo = TipoActividad.Salida,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Manada],
            FechaInicio = DateTime.UtcNow.AddDays(5),
            FechaFin = DateTime.UtcNow.AddDays(5).AddHours(8),
            Precio = 10.00m
        };

        // Actividad 2: Para Clan (Publicada) -> NO debe verse (familia solo tiene Manada)
        var actClan = new ActividadScout
        {
            OrganizacionId = OrgLestonnac,
            Titulo = "Ruta del Clan",
            Lugar = "Gredos",
            Tipo = TipoActividad.Acampada,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [RamaScout.Clan],
            FechaInicio = DateTime.UtcNow.AddDays(7),
            FechaFin = DateTime.UtcNow.AddDays(9),
            Precio = 25.00m
        };

        // Actividad 3: Grupo Completo (Publicada) -> Debe verse
        var actGrupo = new ActividadScout
        {
            OrganizacionId = OrgLestonnac,
            Titulo = "San Jorge de Grupo",
            Lugar = "Parque Abelardo Sánchez",
            Tipo = TipoActividad.Reunion,
            Estado = EstadoActividad.Publicada,
            RamasDestinatarias = [], // Todas
            FechaInicio = DateTime.UtcNow.AddDays(15),
            FechaFin = DateTime.UtcNow.AddDays(15).AddHours(4),
            Precio = 0.00m // Gratuita
        };

        // Actividad 4: Para Manada pero en Borrador -> NO debe verse por familias
        var actBorrador = new ActividadScout
        {
            OrganizacionId = OrgLestonnac,
            Titulo = "Borrador secreto",
            Lugar = "Local",
            Tipo = TipoActividad.Reunion,
            Estado = EstadoActividad.Borrador,
            RamasDestinatarias = [RamaScout.Manada],
            FechaInicio = DateTime.UtcNow.AddDays(20),
            FechaFin = DateTime.UtcNow.AddDays(20).AddHours(2),
            Precio = 0m
        };

        db.Actividades.AddRange(actManada, actClan, actGrupo, actBorrador);
        await db.SaveChangesAsync();

        var servicio = new ServicioActividades(db, tenantContext);

        // Act
        var actividadesFamilia = await servicio.ObtenerActividadesParaFamiliaAsync(tutorId);

        // Assert
        Assert.Equal(2, actividadesFamilia.Count);
        Assert.Contains(actividadesFamilia, a => a.Titulo == "Salida Manada");
        Assert.Contains(actividadesFamilia, a => a.Titulo == "San Jorge de Grupo");
        Assert.DoesNotContain(actividadesFamilia, a => a.Titulo == "Ruta del Clan");
        Assert.DoesNotContain(actividadesFamilia, a => a.Titulo == "Borrador secreto");
    }

    [Fact]
    public async Task ObtenerActividadesKraal_AislamientoMultitenant_NoMuestraActividadesDeOtroGrupo()
    {
        // Arrange
        // Lestonnac
        var ctxLestonnac = new TenantContext();
        ctxLestonnac.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        // San Vicente
        var ctxSanVicente = new TenantContext();
        ctxSanVicente.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 412", 412);

        using (var db = new WebScoutDbContext(_options, ctxLestonnac))
        {
            db.Actividades.Add(new ActividadScout
            {
                OrganizacionId = OrgLestonnac,
                Titulo = "Actividad Lestonnac",
                Lugar = "Albacete",
                FechaInicio = DateTime.UtcNow.AddDays(2),
                FechaFin = DateTime.UtcNow.AddDays(3),
                Precio = 15m
            });
            await db.SaveChangesAsync();
        }

        using (var db = new WebScoutDbContext(_options, ctxSanVicente))
        {
            db.Actividades.Add(new ActividadScout
            {
                OrganizacionId = OrgSanVicente,
                Titulo = "Actividad San Vicente",
                Lugar = "Toledo",
                FechaInicio = DateTime.UtcNow.AddDays(2),
                FechaFin = DateTime.UtcNow.AddDays(3),
                Precio = 20m
            });
            await db.SaveChangesAsync();
        }

        // Act & Assert para Lestonnac
        using (var db = new WebScoutDbContext(_options, ctxLestonnac))
        {
            var servicio = new ServicioActividades(db, ctxLestonnac);
            var actividades = await servicio.ObtenerActividadesKraalAsync();
            Assert.Single(actividades);
            Assert.Equal("Actividad Lestonnac", actividades[0].Titulo);
        }

        // Act & Assert para San Vicente
        using (var db = new WebScoutDbContext(_options, ctxSanVicente))
        {
            var servicio = new ServicioActividades(db, ctxSanVicente);
            var actividades = await servicio.ObtenerActividadesKraalAsync();
            Assert.Single(actividades);
            Assert.Equal("Actividad San Vicente", actividades[0].Titulo);
        }
    }

    [Fact]
    public async Task ValidacionFechas_LanzaExcepcion_SiFechaFinMenorQueFechaInicio()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);
        using var db = new WebScoutDbContext(_options, tenantContext);
        var servicio = new ServicioActividades(db, tenantContext);

        var actividadInvalida = new ActividadScout
        {
            Titulo = "Fecha Invalida",
            Lugar = "Lugar",
            FechaInicio = DateTime.UtcNow.AddDays(5),
            FechaFin = DateTime.UtcNow.AddDays(2), // Fin antes que inicio
            Precio = 10m
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.CrearActividadAsync(actividadInvalida, Guid.NewGuid()));
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
    }
}
