using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

/// <summary>
/// Garantías de la propia infraestructura de pruebas: el esquema procede de las migraciones Npgsql reales
/// y PostgreSQL hace cumplir las restricciones que SQLite en memoria no validaba igual (AGENTS.md §3.1).
/// </summary>
public class PostgreSqlInfraestructuraTests : IDisposable
{
    private const string CodigoViolacionUnicidad = "23505";

    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public PostgreSqlInfraestructuraTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();
        _options = _baseDatos.Opciones;
    }

    [Fact]
    public async Task Esquema_ProcedeDeMigracionesNpgsql_SinMigracionesPendientes()
    {
        using var db = new WebScoutDbContext(_options, new TenantContext());

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", db.Database.ProviderName);
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(db.Database.GetMigrations(), await db.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task BasesDePrueba_EstanAisladasEntreInstancias()
    {
        using var otraBase = BaseDatosPostgresPruebas.Crear();
        Assert.NotEqual(_baseDatos.Nombre, otraBase.Nombre);

        var tenant = ContextoTenant(OrgLestonnac);
        using (var db = new WebScoutDbContext(_options, tenant))
        {
            db.Users.Add(CrearTutor(Guid.NewGuid(), OrgLestonnac, "Tutor Sintético", "tutor.aislamiento@test.es"));
            await db.SaveChangesAsync();
        }

        using var dbOtra = new WebScoutDbContext(otraBase.Opciones, ContextoTenant(OrgLestonnac));
        Assert.Equal(0, await dbOtra.Users.CountAsync());
    }

    [Fact]
    public async Task FichaMedica_SegundaFichaParaElMismoEducando_LaRechazaElIndiceUnicoDePostgreSql()
    {
        var tenant = ContextoTenant(OrgLestonnac);
        var tutorId = Guid.NewGuid();
        var educandoId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenant))
        {
            db.Users.Add(CrearTutor(tutorId, OrgLestonnac, "Tutor Sintético", "tutor.unicidad@test.es"));
            db.Educandos.Add(new Educando
            {
                Id = educandoId,
                TutorLegalId = tutorId,
                Nombre = "Educando",
                Apellidos = "Sintético",
                FechaNacimiento = new DateOnly(2016, 3, 1),
                RamaActual = RamaScout.Manada,
                FichaMedica = CrearFicha("CIP-TEST-0001")
            });
            await db.SaveChangesAsync();
        }

        // Un segundo contexto simula una petición concurrente que no ve la ficha ya guardada.
        using (var db = new WebScoutDbContext(_options, tenant))
        {
            var duplicada = CrearFicha("CIP-TEST-0002");
            duplicada.EducandoId = educandoId;
            db.FichasMedicas.Add(duplicada);

            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(ex.InnerException);
            Assert.Equal(CodigoViolacionUnicidad, postgres.SqlState);
        }
    }

    [Fact]
    public async Task FiltroMultitenant_SeTraduceASqlYAislaOrganizacionesEnPostgreSql()
    {
        using (var db = new WebScoutDbContext(_options, ContextoTenant(OrgLestonnac)))
        {
            db.Users.Add(CrearTutor(Guid.NewGuid(), OrgLestonnac, "Tutor Lestonnac", "tutor.lestonnac@test.es"));
            await db.SaveChangesAsync();
        }

        using (var db = new WebScoutDbContext(_options, ContextoTenant(OrgSanVicente)))
        {
            db.Users.Add(CrearTutor(Guid.NewGuid(), OrgSanVicente, "Tutor San Vicente", "tutor.sanvicente@test.es"));
            await db.SaveChangesAsync();
        }

        using (var db = new WebScoutDbContext(_options, ContextoTenant(OrgLestonnac)))
        {
            var sql = db.Users.ToQueryString();
            Assert.Contains("\"OrganizacionId\"", sql);

            var usuarios = await db.Users.ToListAsync();
            var usuario = Assert.Single(usuarios);
            Assert.Equal(OrgLestonnac, usuario.OrganizacionId);
        }
    }

    private static TenantContext ContextoTenant(Guid organizacionId)
    {
        var tenant = new TenantContext();
        if (organizacionId == OrgLestonnac)
        {
            tenant.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);
        }
        else
        {
            tenant.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 572", 572);
        }

        return tenant;
    }

    private static FichaMedica CrearFicha(string numeroSeguridadSocial) => new()
    {
        NumeroSeguridadSocial = numeroSeguridadSocial,
        TelefonoEmergenciaPrincipal = "+34 600 000 000"
    };

    private static UsuarioScout CrearTutor(Guid id, Guid organizacionId, string nombre, string email) => new()
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

    public void Dispose()
    {
        _baseDatos.Dispose();
    }
}
