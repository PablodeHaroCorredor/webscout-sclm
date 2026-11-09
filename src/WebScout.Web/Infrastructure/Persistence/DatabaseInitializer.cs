using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
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

            // 3. Seed de SuperAdmin Federativo (SCLM / Desarrolladores)
            await SeedDemoUserAsync(userManager, logger, Guid.Empty, "superadmin@scoutsclm.org", "Administrador Federación SCLM", "00000000T", RolesScout.SuperAdmin, RamaScout.Ninguna);

            // 4. Seed de Usuarios Demo para el Grupo Piloto (Lestonnac 330)
            var garciaUser = await SeedDemoUserAsync(userManager, logger, lestonnacId, "familia.garcia@scoutslestonnac.es", "Familia García Pérez", "12345678Z", RolesScout.Familia, RamaScout.Ninguna);
            await SeedDemoUserAsync(userManager, logger, lestonnacId, "scouter.manada@scoutslestonnac.es", "Elena Akela", "87654321A", RolesScout.Scouter, RamaScout.Manada);
            await SeedDemoUserAsync(userManager, logger, lestonnacId, "coordinador@scoutslestonnac.es", "Roberto Coordinador", "45678901B", RolesScout.Coordinacion, RamaScout.Ninguna);

            // 5. Seed de Usuarios Demo para el Grupo de Contraste (San Vicente 412)
            var lopezUser = await SeedDemoUserAsync(userManager, logger, sanVicenteId, "familia.lopez@scoutssanvicente.org", "Familia López Ruiz", "23456789C", RolesScout.Familia, RamaScout.Ninguna);
            await SeedDemoUserAsync(userManager, logger, sanVicenteId, "scouter.tropa@scoutssanvicente.org", "David Baloo", "98765432D", RolesScout.Scouter, RamaScout.Tropa);

            // 6. Seed de Educandos y Fichas Médicas Anuales
            await SeedParticipantsAndHealthRecordsAsync(db, logger, lestonnacId, sanVicenteId, garciaUser, lopezUser);

            // 7. Seed de Actividades y Campamentos por Ramas
            await SeedActivitiesAsync(db, logger, lestonnacId, sanVicenteId);

            // 8. Seed de Inscripciones y Justificantes de Pago
            await SeedInscriptionsAsync(db, logger, lestonnacId, sanVicenteId, garciaUser, lopezUser);
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

    private static async Task SeedActivitiesAsync(
        WebScoutDbContext db,
        ILogger logger,
        Guid lestonnacId,
        Guid sanVicenteId)
    {
        // 1. Actividades para Lestonnac 330
        if (!await db.Actividades.IgnoreQueryFilters().AnyAsync(a => a.OrganizacionId == lestonnacId))
        {
            logger.LogInformation("Poblando actividades y campamentos para Lestonnac 330...");

            var ahora = DateTime.UtcNow;
            var sabadoManada = DateTime.SpecifyKind(ahora.Date.AddDays(7).AddHours(9), DateTimeKind.Utc);
            var acampadaInicio = DateTime.SpecifyKind(ahora.Date.AddDays(20).AddHours(8), DateTimeKind.Utc);
            var acampadaFin = DateTime.SpecifyKind(ahora.Date.AddDays(22).AddHours(18), DateTimeKind.Utc);
            var campamentoVeranoInicio = DateTime.SpecifyKind(new DateTime(2027, 7, 15, 8, 0, 0), DateTimeKind.Utc);
            var campamentoVeranoFin = DateTime.SpecifyKind(new DateTime(2027, 7, 27, 19, 0, 0), DateTimeKind.Utc);

            var salidaManada = new ActividadScout
            {
                Id = Guid.NewGuid(),
                OrganizacionId = lestonnacId,
                Titulo = "Salida a la Laguna del Arquillo: Rastreos y Naturaleza",
                Descripcion = "Jornada de día completo para la Manada para trabajar la técnica scout de rastreo, reconocimiento de huellas de la fauna autóctona y juego de pistas en el entorno protegido de la laguna.",
                Tipo = TipoActividad.Salida,
                Estado = EstadoActividad.Publicada,
                RamasDestinatarias = [RamaScout.Manada],
                Lugar = "Paraje Natural Laguna del Arquillo (Masegoso)",
                PuntoEncuentroSalida = "Local Scout Lestonnac a las 09:00h",
                PuntoEncuentroRegreso = "Local Scout Lestonnac a las 18:00h",
                FechaInicio = sabadoManada,
                FechaFin = sabadoManada.AddHours(9),
                FechaLimiteInscripcion = sabadoManada.AddDays(-3),
                Precio = 12.50m,
                IbanCuentaBancaria = "ES91 2100 0418 4502 0005 1330",
                ConceptoPagoSugerido = "Salida Manada - Nombre Educando",
                MaterialRequerido = "Mochila pequeña, cantimplora llena (1L mínimo), almuerzo y comida de media mañana, gorra, protección solar, pañoleta y calzado deportivo adecuado para caminar.",
                ObservacionesSanitarias = "Revisar previamente si el educando es sensible a picaduras de avispa.",
                FechaCreacionUtc = ahora
            };

            var acampadaOtono = new ActividadScout
            {
                Id = Guid.NewGuid(),
                OrganizacionId = lestonnacId,
                Titulo = "Acampada de Manada y Tropa en Sierra de Alcaraz",
                Descripcion = "Gran acampada de fin de semana con pernocta en cabañas y tiendas de campaña en el albergue juvenil. Talleres de cabuyería, fuego de campamento y marchas adaptadas por ramas.",
                Tipo = TipoActividad.Acampada,
                Estado = EstadoActividad.Publicada,
                RamasDestinatarias = [RamaScout.Manada, RamaScout.Tropa],
                Lugar = "Albergue Juvenil Los Batanes, Riópar (Albacete)",
                PuntoEncuentroSalida = "Estación de Autobuses (Andén 12) a las 08:00h",
                PuntoEncuentroRegreso = "Estación de Autobuses a las 18:30h",
                FechaInicio = acampadaInicio,
                FechaFin = acampadaFin,
                FechaLimiteInscripcion = acampadaInicio.AddDays(-6),
                Precio = 28.50m,
                IbanCuentaBancaria = "ES91 2100 0418 4502 0005 1330",
                ConceptoPagoSugerido = "Acampada Alcaraz - Nombre Educando",
                MaterialRequerido = "Saco de dormir (confort 5°C), esterilla aislante, muda completa en bolsa estanca, neceser scout con toalla pequeña, botas de montaña, linterna frontal con pilas, cubiertos, plato hondo de aluminio o plástico y pañoleta.",
                ObservacionesSanitarias = "Bajas temperaturas nocturnas. Familias con educandos asmáticos o alérgicos deben verificar que la medicación de rescate figure en la ficha médica anual.",
                FechaCreacionUtc = ahora
            };

            var campamentoVerano = new ActividadScout
            {
                Id = Guid.NewGuid(),
                OrganizacionId = lestonnacId,
                Titulo = "Campamento de Verano Pirineos 2027 (Grupo Completo)",
                Descripcion = "Campamento de verano de 12 días en alta montaña para todas las ramas del grupo scout. Grandes construcciones, ascensiones y vivacs.",
                Tipo = TipoActividad.Campamento,
                Estado = EstadoActividad.Borrador,
                RamasDestinatarias = [], // Grupo completo
                Lugar = "Valle de Pineta, Bielsa (Huesca)",
                PuntoEncuentroSalida = "Patio del Colegio Lestonnac a las 06:30h",
                PuntoEncuentroRegreso = "Patio del Colegio Lestonnac a las 20:00h",
                FechaInicio = campamentoVeranoInicio,
                FechaFin = campamentoVeranoFin,
                FechaLimiteInscripcion = campamentoVeranoInicio.AddMonths(-1),
                Precio = 245.00m,
                IbanCuentaBancaria = "ES91 2100 0418 4502 0005 1330",
                ConceptoPagoSugerido = "Campamento Pirineos - Nombre Educando",
                MaterialRequerido = "Dossier completo de campamento de verano disponible en la reunión de familias.",
                FechaCreacionUtc = ahora
            };

            db.Actividades.AddRange(salidaManada, acampadaOtono, campamentoVerano);
            await db.SaveChangesAsync();
            logger.LogInformation("Actividades demo de Lestonnac 330 creadas con éxito.");
        }

        // 2. Actividades para San Vicente 412
        if (!await db.Actividades.IgnoreQueryFilters().AnyAsync(a => a.OrganizacionId == sanVicenteId))
        {
            logger.LogInformation("Poblando actividades demo para San Vicente 412...");

            var ahora = DateTime.UtcNow;
            var jornadaCastores = DateTime.SpecifyKind(ahora.Date.AddDays(10).AddHours(10), DateTimeKind.Utc);

            var actividadCastores = new ActividadScout
            {
                Id = Guid.NewGuid(),
                OrganizacionId = sanVicenteId,
                Titulo = "Jornada de la Presa en el Bosque: Gran Juego de Castores",
                Descripcion = "Juegos cooperativos al aire libre para la Colonia de Castores. Descubriremos cómo construyen sus presas los castores y aprenderemos a cuidar nuestro entorno natural.",
                Tipo = TipoActividad.Salida,
                Estado = EstadoActividad.Publicada,
                RamasDestinatarias = [RamaScout.Castores],
                Lugar = "Parque Forestal La Pulgosa",
                PuntoEncuentroSalida = "Entrada principal del parque a las 10:00h",
                PuntoEncuentroRegreso = "Entrada principal del parque a las 14:00h",
                FechaInicio = jornadaCastores,
                FechaFin = jornadaCastores.AddHours(4),
                FechaLimiteInscripcion = jornadaCastores.AddDays(-2),
                Precio = 0.00m, // Actividad gratuita
                MaterialRequerido = "Ropa deportiva cómoda que se pueda ensuciar, zapatillas de deporte, gorra, cantimplora con agua y almuerzo saludable.",
                FechaCreacionUtc = ahora
            };

            db.Actividades.Add(actividadCastores);
            await db.SaveChangesAsync();
            logger.LogInformation("Actividad demo de San Vicente 412 creada: Castores (Gratuita).");
        }
    }

    private static async Task SeedInscriptionsAsync(
        WebScoutDbContext db,
        ILogger logger,
        Guid lestonnacId,
        Guid sanVicenteId,
        UsuarioScout? garciaUser,
        UsuarioScout? lopezUser)
    {
        if (await db.Inscripciones.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        logger.LogInformation("Poblando inscripciones de prueba y justificantes bancarios...");

        // 1. Inscripciones en Lestonnac 330
        if (garciaUser != null)
        {
            var salidaManada = await db.Actividades.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.OrganizacionId == lestonnacId && a.Titulo.Contains("Laguna del Arquillo"));
            var acampadaAlcaraz = await db.Actividades.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.OrganizacionId == lestonnacId && a.Titulo.Contains("Sierra de Alcaraz"));

            var lucia = await db.Educandos.IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TutorLegalId == garciaUser.Id && e.Nombre == "Lucía");
            var marcos = await db.Educandos.IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TutorLegalId == garciaUser.Id && e.Nombre == "Marcos");

            if (salidaManada != null && lucia != null)
            {
                // Crear archivo físico demo para el comprobante de Lucía
                var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "justificantes", lestonnacId.ToString());
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                var nombreFisico = $"demo_justificante_lucia_{Guid.NewGuid():N}.pdf";
                var rutaFisica = Path.Combine(baseDir, nombreFisico);
                var rutaRelativa = $"{lestonnacId}/{nombreFisico}";

                // PDF mínimo válido
                var pdfBytes = "%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R/Resources<<>>>>endobj\nxref\n0 4\n0000000000 65535 f\n0000000009 00000 n\n0000000052 00000 n\n0000000101 00000 n\ntrailer<</Size 4/Root 1 0 R>>\nstartxref\n178\n%%EOF"u8.ToArray();
                await File.WriteAllBytesAsync(rutaFisica, pdfBytes);

                var inscripcionLucia = new InscripcionActividad
                {
                    Id = Guid.NewGuid(),
                    OrganizacionId = lestonnacId,
                    ActividadId = salidaManada.Id,
                    EducandoId = lucia.Id,
                    TutorLegalId = garciaUser.Id,
                    Estado = EstadoInscripcion.EnRevision,
                    ImporteEuros = salidaManada.Precio,
                    AutorizacionFirmada = true,
                    FechaAutorizacionUtc = DateTime.UtcNow.AddDays(-1),
                    FechaInscripcionUtc = DateTime.UtcNow.AddDays(-1),
                    NombreArchivoJustificante = "justificante_transferencia_santander_lucia.pdf",
                    RutaArchivoJustificante = rutaRelativa,
                    FechaSubidaJustificanteUtc = DateTime.UtcNow.AddDays(-1).AddHours(1),
                    ObservacionesFamilia = "Transferencia bancaria efectuada desde el Banco Santander. Referencia: Salida Manada Lucia Garcia."
                };

                db.Inscripciones.Add(inscripcionLucia);
            }

            if (acampadaAlcaraz != null && marcos != null)
            {
                var inscripcionMarcos = new InscripcionActividad
                {
                    Id = Guid.NewGuid(),
                    OrganizacionId = lestonnacId,
                    ActividadId = acampadaAlcaraz.Id,
                    EducandoId = marcos.Id,
                    TutorLegalId = garciaUser.Id,
                    Estado = EstadoInscripcion.PendienteJustificante,
                    ImporteEuros = acampadaAlcaraz.Precio,
                    AutorizacionFirmada = true,
                    FechaAutorizacionUtc = DateTime.UtcNow.AddHours(-5),
                    FechaInscripcionUtc = DateTime.UtcNow.AddHours(-5),
                    ObservacionesFamilia = "Llegará en el turno matinal. Pendiente de adjuntar comprobante."
                };

                db.Inscripciones.Add(inscripcionMarcos);
            }
        }

        // 2. Inscripciones en San Vicente 412
        if (lopezUser != null)
        {
            var jornadaCastores = await db.Actividades.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.OrganizacionId == sanVicenteId && a.Titulo.Contains("Jornada de la Presa"));
            var hugo = await db.Educandos.IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TutorLegalId == lopezUser.Id && e.Nombre == "Hugo");

            if (jornadaCastores != null && hugo != null)
            {
                var inscripcionHugo = new InscripcionActividad
                {
                    Id = Guid.NewGuid(),
                    OrganizacionId = sanVicenteId,
                    ActividadId = jornadaCastores.Id,
                    EducandoId = hugo.Id,
                    TutorLegalId = lopezUser.Id,
                    Estado = EstadoInscripcion.Confirmada,
                    ImporteEuros = 0.00m,
                    AutorizacionFirmada = true,
                    FechaAutorizacionUtc = DateTime.UtcNow.AddHours(-3),
                    FechaInscripcionUtc = DateTime.UtcNow.AddHours(-3),
                    ObservacionesFamilia = "Primera salida con la Colonia de Castores."
                };

                db.Inscripciones.Add(inscripcionHugo);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Inscripciones de demostración creadas con éxito.");
    }
}
