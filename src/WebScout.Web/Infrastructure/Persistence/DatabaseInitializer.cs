using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;

namespace WebScout.Web.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebScoutDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<WebScoutDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RolScout>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioScout>>();

        try
        {
            logger.LogInformation("Verificando conexión con PostgreSQL...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones de PostgreSQL aplicadas correctamente.");

            // 1. Seed inicial de los dos grupos scout de referencia (Lestonnac 330 y San Vicente 412)
            var lestonnacId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var sanVicenteId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            if (!await db.GruposScout.AnyAsync())
            {
                logger.LogInformation("Poblando grupos scout iniciales de SCLM...");

                var lestonnac = new GrupoScout
                {
                    Id = lestonnacId,
                    Slug = "lestonnac",
                    Nombre = "Grupo Scout Lestonnac 330",
                    NumeroGrupo = 330,
                    Lema = "Siempre Listos para Servir",
                    EmailContacto = "coordinacion@scoutslestonnac.es",
                    Activo = true,
                    Panoleta = PanoletaConfig.Lestonnac330,
                    Paleta = PaletteConfig.DefaultLestonnac
                };

                var sanVicente = new GrupoScout
                {
                    Id = sanVicenteId,
                    Slug = "sanvicente",
                    Nombre = "Grupo Scout San Vicente 412",
                    NumeroGrupo = 412,
                    Lema = "Por la senda del deber",
                    EmailContacto = "coordinacion@scoutssanvicente.org",
                    Activo = true,
                    Panoleta = PanoletaConfig.SanVicente412,
                    Paleta = new PaletteConfig
                    {
                        PrimaryHex = "#0D47A1",
                        SecondaryHex = "#D32F2F",
                        AccentHex = "#F57C00",
                        TextOnPrimaryHex = "#FFFFFF",
                        TextOnSecondaryHex = "#FFFFFF",
                        SurfaceHeaderHex = "#0D47A1"
                    }
                };

                db.GruposScout.AddRange(lestonnac, sanVicente);
                await db.SaveChangesAsync();
                logger.LogInformation("Seed de grupos scout completado con éxito.");
            }

            // 2. Seed de Roles de Identity
            foreach (var nombreRol in RolesScout.Todos)
            {
                if (!await roleManager.RoleExistsAsync(nombreRol))
                {
                    await roleManager.CreateAsync(new RolScout(nombreRol));
                    logger.LogInformation("Rol creado: {RoleName}", nombreRol);
                }
            }

            // 3. Seed de Usuarios Demo para el Grupo Piloto (Lestonnac 330)
            var garciaUser = await SeedDemoUserAsync(userManager, logger, lestonnacId, "familia.garcia@scoutslestonnac.es", "Familia García Pérez", "12345678Z", RolesScout.Familia, RamaScout.Ninguna);
            await SeedDemoUserAsync(userManager, logger, lestonnacId, "scouter.manada@scoutslestonnac.es", "Elena Akela", "87654321A", RolesScout.Scouter, RamaScout.Manada);
            await SeedDemoUserAsync(userManager, logger, lestonnacId, "coordinador@scoutslestonnac.es", "Roberto Coordinador", "45678901B", RolesScout.Coordinacion, RamaScout.Ninguna);

            // 4. Seed de Usuarios Demo para el Grupo de Contraste (San Vicente 412)
            var lopezUser = await SeedDemoUserAsync(userManager, logger, sanVicenteId, "familia.lopez@scoutssanvicente.org", "Familia López Ruiz", "23456789C", RolesScout.Familia, RamaScout.Ninguna);
            await SeedDemoUserAsync(userManager, logger, sanVicenteId, "scouter.tropa@scoutssanvicente.org", "David Baloo", "98765432D", RolesScout.Scouter, RamaScout.Tropa);

            // 5. Seed de Educandos y Fichas Médicas Anuales
            await SeedParticipantsAndHealthRecordsAsync(db, logger, lestonnacId, sanVicenteId, garciaUser, lopezUser);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Advertencia durante la inicialización de base de datos ({Message}).", ex.Message);
        }
    }

    private static async Task SeedParticipantsAndHealthRecordsAsync(
        WebScoutDbContext db,
        ILogger logger,
        Guid lestonnacId,
        Guid sanVicenteId,
        UsuarioScout? garciaUser,
        UsuarioScout? lopezUser)
    {
        // Educandos de Lestonnac 330 (Familia García Pérez)
        if (garciaUser != null && !await db.Educandos.IgnoreQueryFilters().AnyAsync(p => p.TutorLegalId == garciaUser.Id))
        {
            logger.LogInformation("Poblando educandos y fichas sanitarias para Lestonnac 330...");

            var lucia = new Educando
            {
                Id = Guid.NewGuid(),
                OrganizacionId = lestonnacId,
                TutorLegalId = garciaUser.Id,
                Nombre = "Lucía",
                Apellidos = "García Pérez",
                FechaNacimiento = new DateOnly(2017, 4, 15),
                Sexo = "Femenino",
                RamaActual = RamaScout.Manada,
                Activo = true,
                FechaCreacionUtc = DateTime.UtcNow,
                FichaMedica = new FichaMedica
                {
                    Id = Guid.NewGuid(),
                    OrganizacionId = lestonnacId,
                    NumeroSeguridadSocial = "CIP-CLM-98234123",
                    EntidadAseguradora = "SESCAM",
                    AlergiasAlimentarias = "Alérgica a los frutos secos (cacahuete y nuez)",
                    RegimenAlimenticio = "Celíaca (requiere menú 100% libre de gluten y sin trazas)",
                    SabeNadar = true,
                    VacunacionAlDia = true,
                    TelefonoEmergenciaPrincipal = "+34 600 111 222",
                    NombreContactoEmergenciaPrincipal = "María Pérez (Madre)",
                    TelefonoEmergenciaSecundario = "+34 600 333 444",
                    NombreContactoEmergenciaSecundario = "Carlos García (Padre)",
                    ObservacionesAdicionales = "Lleva su propio botiquín con medicación de rescate en caso de ingestión accidental de gluten."
                }
            };

            var marcos = new Educando
            {
                Id = Guid.NewGuid(),
                OrganizacionId = lestonnacId,
                TutorLegalId = garciaUser.Id,
                Nombre = "Marcos",
                Apellidos = "García Pérez",
                FechaNacimiento = new DateOnly(2014, 9, 20),
                Sexo = "Masculino",
                RamaActual = RamaScout.Tropa,
                Activo = true,
                FechaCreacionUtc = DateTime.UtcNow,
                FichaMedica = new FichaMedica
                {
                    Id = Guid.NewGuid(),
                    OrganizacionId = lestonnacId,
                    NumeroSeguridadSocial = "CIP-CLM-77123990",
                    EntidadAseguradora = "SESCAM",
                    EnfermedadesCronicas = "Asma bronquial estacional",
                    MedicacionActiva = "Ventolín (Salbutamol inhalador). Dos inhalaciones previas a ejercicio físico intenso si presenta sibilancias.",
                    SabeNadar = true,
                    VacunacionAlDia = true,
                    TelefonoEmergenciaPrincipal = "+34 600 111 222",
                    NombreContactoEmergenciaPrincipal = "María Pérez (Madre)",
                    TelefonoEmergenciaSecundario = "+34 600 333 444",
                    NombreContactoEmergenciaSecundario = "Carlos García (Padre)",
                    ObservacionesAdicionales = "Lleva el inhalador siempre consigo en la mochila de mano."
                }
            };

            db.Educandos.AddRange(lucia, marcos);
            await db.SaveChangesAsync();
            logger.LogInformation("Educandos de Lestonnac 330 creados: Lucía (Manada) y Marcos (Tropa).");
        }

        // Educando de San Vicente 412 (Familia López Ruiz)
        if (lopezUser != null && !await db.Educandos.IgnoreQueryFilters().AnyAsync(p => p.TutorLegalId == lopezUser.Id))
        {
            logger.LogInformation("Poblando educando y ficha sanitaria para San Vicente 412...");

            var hugo = new Educando
            {
                Id = Guid.NewGuid(),
                OrganizacionId = sanVicenteId,
                TutorLegalId = lopezUser.Id,
                Nombre = "Hugo",
                Apellidos = "López Ruiz",
                FechaNacimiento = new DateOnly(2019, 2, 10),
                Sexo = "Masculino",
                RamaActual = RamaScout.Castores,
                Activo = true,
                FechaCreacionUtc = DateTime.UtcNow,
                FichaMedica = new FichaMedica
                {
                    Id = Guid.NewGuid(),
                    OrganizacionId = sanVicenteId,
                    NumeroSeguridadSocial = "CIP-CLM-44321098",
                    EntidadAseguradora = "SESCAM",
                    AlergiasAlimentarias = "Alérgico al huevo y derivados",
                    RegimenAlimenticio = "Exclusión total de huevo",
                    SabeNadar = false,
                    VacunacionAlDia = true,
                    TelefonoEmergenciaPrincipal = "+34 611 222 333",
                    NombreContactoEmergenciaPrincipal = "Laura Ruiz (Madre)",
                    ObservacionesAdicionales = "Está aprendiendo a nadar, requiere manguitos o chaleco en piscina/río."
                }
            };

            db.Educandos.Add(hugo);
            await db.SaveChangesAsync();
            logger.LogInformation("Educando de San Vicente 412 creado: Hugo (Castores).");
        }
    }

    private static async Task<UsuarioScout?> SeedDemoUserAsync(
        UserManager<UsuarioScout> userManager,
        ILogger logger,
        Guid organizacionId,
        string email,
        string nombreCompleto,
        string dni,
        string rol,
        RamaScout rama)
    {
        var existingUser = await userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.OrganizacionId == organizacionId && u.NormalizedEmail == email.ToUpperInvariant());
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = new UsuarioScout
        {
            Id = Guid.NewGuid(),
            OrganizacionId = organizacionId,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NombreCompleto = nombreCompleto,
            Dni = dni,
            RamaAsignada = rama,
            Activo = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, "Scout123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, rol);
            logger.LogInformation("Usuario demo creado [{TenantId}]: {Email} ({Role})", organizacionId, email, rol);
            return user;
        }
        else
        {
            logger.LogWarning("No se pudo crear el usuario {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }
    }
}
