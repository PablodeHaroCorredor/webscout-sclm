using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebScoutDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<WebScoutDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ScoutRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ScoutUser>>();

        try
        {
            logger.LogInformation("Verificando conexión con PostgreSQL...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones de PostgreSQL aplicadas correctamente.");

            // 1. Seed inicial de los dos tenants de referencia (Lestonnac 330 y San Vicente 412)
            var lestonnacId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var sanVicenteId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            if (!await db.Tenants.AnyAsync())
            {
                logger.LogInformation("Poblando tenants iniciales de SCLM...");

                var lestonnac = new ScoutTenant
                {
                    Id = lestonnacId,
                    Slug = "lestonnac",
                    Name = "Grupo Scout Lestonnac 330",
                    GroupNumber = 330,
                    Motto = "Siempre Listos para Servir",
                    ContactEmail = "coordinacion@scoutslestonnac.es",
                    IsActive = true,
                    Panoleta = PanoletaConfig.Lestonnac330,
                    Palette = PaletteConfig.DefaultLestonnac
                };

                var sanVicente = new ScoutTenant
                {
                    Id = sanVicenteId,
                    Slug = "sanvicente",
                    Name = "Grupo Scout San Vicente 412",
                    GroupNumber = 412,
                    Motto = "Por la senda del deber",
                    ContactEmail = "coordinacion@scoutssanvicente.org",
                    IsActive = true,
                    Panoleta = PanoletaConfig.SanVicente412,
                    Palette = new PaletteConfig
                    {
                        PrimaryHex = "#0D47A1",
                        SecondaryHex = "#D32F2F",
                        AccentHex = "#F57C00",
                        TextOnPrimaryHex = "#FFFFFF",
                        TextOnSecondaryHex = "#FFFFFF",
                        SurfaceHeaderHex = "#0D47A1"
                    }
                };

                db.Tenants.AddRange(lestonnac, sanVicente);
                await db.SaveChangesAsync();
                logger.LogInformation("Seed de tenants completado con éxito.");
            }

            // 2. Seed de Roles de Identity
            foreach (var roleName in ScoutRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ScoutRole(roleName));
                    logger.LogInformation("Rol creado: {RoleName}", roleName);
                }
            }

            // 3. Seed de Usuarios Demo para el Tenant Piloto (Lestonnac 330)
            await SeedDemoUserAsync(userManager, logger, lestonnacId, "familia.garcia@scoutslestonnac.es", "Familia García Pérez", "12345678Z", ScoutRoles.Familia, ScoutBranch.None);
            await SeedDemoUserAsync(userManager, logger, lestonnacId, "scouter.manada@scoutslestonnac.es", "Elena Akela", "87654321A", ScoutRoles.Scouter, ScoutBranch.Manada);
            await SeedDemoUserAsync(userManager, logger, lestonnacId, "coordinador@scoutslestonnac.es", "Roberto Coordinador", "45678901B", ScoutRoles.Coordinacion, ScoutBranch.None);

            // 4. Seed de Usuarios Demo para el Tenant de Contraste (San Vicente 412)
            await SeedDemoUserAsync(userManager, logger, sanVicenteId, "familia.lopez@scoutssanvicente.org", "Familia López Ruiz", "23456789C", ScoutRoles.Familia, ScoutBranch.None);
            await SeedDemoUserAsync(userManager, logger, sanVicenteId, "scouter.tropa@scoutssanvicente.org", "David Baloo", "98765432D", ScoutRoles.Scouter, ScoutBranch.Tropa);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Advertencia durante la inicialización de base de datos ({Message}).", ex.Message);
        }
    }

    private static async Task SeedDemoUserAsync(
        UserManager<ScoutUser> userManager,
        ILogger logger,
        Guid organizationId,
        string email,
        string fullName,
        string dni,
        string role,
        ScoutBranch branch)
    {
        // En Identity, buscamos si el usuario ya existe ignorando el query filter
        var existingUser = await userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.OrganizationId == organizationId && u.NormalizedEmail == email.ToUpperInvariant());
        if (existingUser != null)
        {
            return;
        }

        var user = new ScoutUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            Dni = dni,
            AssignedBranch = branch,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, "Scout123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("Usuario demo creado [{TenantId}]: {Email} ({Role})", organizationId, email, role);
        }
        else
        {
            logger.LogWarning("No se pudo crear el usuario {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
