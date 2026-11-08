using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebScout.Web.Common.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class TenantDataIsolationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public TenantDataIsolationTests()
    {
        // Usar conexión SQLite en memoria viva durante todo el test
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<WebScoutDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Crear el esquema inicial con un contexto sin tenant
        var initContext = new TenantContext();
        using var db = new WebScoutDbContext(_options, initContext);
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Insert_WithActiveTenant_ShouldAutomaticallyStampOrganizationId()
    {
        // Arrange: Activar Lestonnac
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var activity = new ScoutActivity
            {
                Title = "Campamento de San Jorge",
                Location = "Sierra de Alcaraz"
            };

            // Act
            db.Activities.Add(activity);
            await db.SaveChangesAsync();

            // Assert: OrganizationId debe ser estampada automáticamente
            Assert.Equal(OrgLestonnac, activity.OrganizationId);
        }
    }

    [Fact]
    public async Task Insert_WithoutActiveTenant_ShouldThrowSecurityException()
    {
        // Arrange: Contexto vacío sin tenant resuelto
        var tenantContext = new TenantContext();

        using var db = new WebScoutDbContext(_options, tenantContext);
        var activity = new ScoutActivity
        {
            Title = "Salida no autorizada",
            Location = "Lugar desconocido"
        };

        db.Activities.Add(activity);

        // Act & Assert: Debe bloquear la persistencia
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Query_WithGlobalFilters_ShouldOnlyReturnCurrentTenantData_PA01_PA18()
    {
        // 1. Población con Lestonnac
        var ctxLestonnac = new TenantContext();
        ctxLestonnac.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        using (var db = new WebScoutDbContext(_options, ctxLestonnac))
        {
            db.Activities.Add(new ScoutActivity
            {
                Title = "Acampada de Tropa L330",
                Location = "Ruidera"
            });
            await db.SaveChangesAsync();
        }

        // 2. Población con San Vicente
        var ctxSanVicente = new TenantContext();
        ctxSanVicente.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 412", 412);

        using (var db = new WebScoutDbContext(_options, ctxSanVicente))
        {
            db.Activities.Add(new ScoutActivity
            {
                Title = "Paso de Secciones SV412",
                Location = "Toledo"
            });
            await db.SaveChangesAsync();
        }

        // 3. Verificación de aislamiento estricto: Lestonnac NUNCA ve a San Vicente
        using (var db = new WebScoutDbContext(_options, ctxLestonnac))
        {
            var activities = await db.Activities.ToListAsync();

            Assert.Single(activities);
            Assert.Equal("Acampada de Tropa L330", activities[0].Title);
            Assert.Equal(OrgLestonnac, activities[0].OrganizationId);
        }

        // 4. Verificación de aislamiento estricto: San Vicente NUNCA ve a Lestonnac
        using (var db = new WebScoutDbContext(_options, ctxSanVicente))
        {
            var activities = await db.Activities.ToListAsync();

            Assert.Single(activities);
            Assert.Equal("Paso de Secciones SV412", activities[0].Title);
            Assert.Equal(OrgSanVicente, activities[0].OrganizationId);
        }

        // 5. Verificación administrativa federativa (IgnoreQueryFilters)
        using (var db = new WebScoutDbContext(_options, ctxLestonnac))
        {
            var allActivitiesFederated = await db.Activities.IgnoreQueryFilters().ToListAsync();
            Assert.Equal(2, allActivitiesFederated.Count);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
