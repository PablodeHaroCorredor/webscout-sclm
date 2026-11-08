using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class IdentityMultitenantTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public IdentityMultitenantTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<WebScoutDbContext>()
            .UseSqlite(_connection)
            .Options;

        var initContext = new TenantContext();
        using var db = new WebScoutDbContext(_options, initContext);
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Insert_ScoutUser_WithActiveTenant_ShouldStampOrganizationId()
    {
        var ctx = new TenantContext();
        ctx.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        using (var db = new WebScoutDbContext(_options, ctx))
        {
            var user = new UsuarioScout
            {
                UserName = "familia.garcia@lestonnac.es",
                Email = "familia.garcia@lestonnac.es",
                NombreCompleto = "Familia García Pérez"
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            Assert.Equal(OrgLestonnac, user.OrganizacionId);
        }
    }

    [Fact]
    public async Task Query_ScoutUser_ShouldIsolateUsersBetweenTenants_PA01_PA02()
    {
        // 1. Crear usuario en Lestonnac
        var ctxLestonnac = new TenantContext();
        ctxLestonnac.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        using (var db = new WebScoutDbContext(_options, ctxLestonnac))
        {
            db.Users.Add(new UsuarioScout
            {
                UserName = "scouter.manada@lestonnac.es",
                Email = "scouter.manada@lestonnac.es",
                NombreCompleto = "Elena Akela",
                RamaAsignada = RamaScout.Manada
            });
            await db.SaveChangesAsync();
        }

        // 2. Crear usuario en San Vicente
        var ctxSanVicente = new TenantContext();
        ctxSanVicente.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 412", 412);

        using (var db = new WebScoutDbContext(_options, ctxSanVicente))
        {
            db.Users.Add(new UsuarioScout
            {
                UserName = "scouter.tropa@sanvicente.org",
                Email = "scouter.tropa@sanvicente.org",
                NombreCompleto = "David Baloo",
                RamaAsignada = RamaScout.Tropa
            });
            await db.SaveChangesAsync();
        }

        // 3. Verificar que Lestonnac solo ve a su propio scouter
        using (var db = new WebScoutDbContext(_options, ctxLestonnac))
        {
            var users = await db.Users.ToListAsync();
            Assert.Single(users);
            Assert.Equal("Elena Akela", users[0].NombreCompleto);
            Assert.Equal(RamaScout.Manada, users[0].RamaAsignada);
            Assert.Equal(OrgLestonnac, users[0].OrganizacionId);
        }

        // 4. Verificar que San Vicente solo ve a su propio scouter
        using (var db = new WebScoutDbContext(_options, ctxSanVicente))
        {
            var users = await db.Users.ToListAsync();
            Assert.Single(users);
            Assert.Equal("David Baloo", users[0].NombreCompleto);
            Assert.Equal(RamaScout.Tropa, users[0].RamaAsignada);
            Assert.Equal(OrgSanVicente, users[0].OrganizacionId);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
