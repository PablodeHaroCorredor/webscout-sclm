using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;

namespace WebScout.Web.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebScoutDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<WebScoutDbContext>();

        try
        {
            logger.LogInformation("Verificando conexión con PostgreSQL...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones de PostgreSQL aplicadas correctamente.");

            // Seed inicial de los dos tenants de referencia (Lestonnac 330 y San Vicente 412)
            if (!await db.Tenants.AnyAsync())
            {
                logger.LogInformation("Poblando tenants iniciales de SCLM...");

                var lestonnac = new ScoutTenant
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
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
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
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
        }
        catch (Exception ex)
        {
            logger.LogWarning("No se pudo conectar a PostgreSQL en este momento ({Message}). La aplicación continuará funcionando con el contexto en memoria.", ex.Message);
        }
    }
}
