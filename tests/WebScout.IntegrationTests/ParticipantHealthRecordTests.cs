using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class ParticipantHealthRecordTests : IDisposable
{
    private readonly BaseDatosPostgresPruebas _baseDatos;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ParticipantHealthRecordTests()
    {
        _baseDatos = BaseDatosPostgresPruebas.Crear();

        _options = _baseDatos.Opciones;
    }

    private static UsuarioScout CrearTutor(Guid id, Guid organizacionId, string nombre, string email)
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
    public void CalcularEdad_DebeCalcularEdadExactaSegunDiaCivil()
    {
        // Arrange
        var referencia = new DateOnly(2026, 11, 8);
        var educando1 = new Educando
        {
            Nombre = "Lucía",
            Apellidos = "García",
            FechaNacimiento = new DateOnly(2017, 4, 15) // Ya cumplió 9 años
        };
        var educando2 = new Educando
        {
            Nombre = "Carlos",
            Apellidos = "García",
            FechaNacimiento = new DateOnly(2017, 12, 1) // Todavía tiene 8 años a 8 de noviembre
        };

        // Act & Assert
        Assert.Equal(9, educando1.CalcularEdad(referencia));
        Assert.Equal(8, educando2.CalcularEdad(referencia));
    }

    [Fact]
    public async Task Insert_EducandoYFichaMedica_DebeEstamparOrganizacionIdAutomaticamente()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var tutor = CrearTutor(tutorId, OrgLestonnac, "María Pérez", "maria@test.com");
            db.Users.Add(tutor);
            await db.SaveChangesAsync();

            var educando = new Educando
            {
                TutorLegalId = tutorId,
                Nombre = "Lucía",
                Apellidos = "García Pérez",
                FechaNacimiento = new DateOnly(2017, 4, 15),
                RamaActual = RamaScout.Manada,
                FichaMedica = new FichaMedica
                {
                    NumeroSeguridadSocial = "CIP-CLM-98234123",
                    AlergiasAlimentarias = "Frutos secos",
                    RegimenAlimenticio = "Celíaca",
                    SabeNadar = true,
                    TelefonoEmergenciaPrincipal = "+34 600 111 222"
                }
            };

            // Act
            db.Educandos.Add(educando);
            await db.SaveChangesAsync();

            // Assert
            Assert.Equal(OrgLestonnac, educando.OrganizacionId);
            Assert.NotNull(educando.FichaMedica);
            Assert.Equal(OrgLestonnac, educando.FichaMedica.OrganizacionId);
            Assert.Equal(educando.Id, educando.FichaMedica.EducandoId);
        }
    }

    [Fact]
    public async Task QueryFilter_DebeAislarEstrictamenteEducandosYFichasEntreGruposScout()
    {
        // 1. Sembrar datos en Lestonnac 330
        var lestonnacContext = new TenantContext();
        lestonnacContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorLestonnac = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, lestonnacContext))
        {
            var tutor = CrearTutor(tutorLestonnac, OrgLestonnac, "María Pérez", "maria@lestonnac.com");
            db.Users.Add(tutor);

            db.Educandos.AddRange(
                new Educando
                {
                    TutorLegalId = tutorLestonnac,
                    Nombre = "Lucía",
                    Apellidos = "García Pérez",
                    FechaNacimiento = new DateOnly(2017, 4, 15),
                    RamaActual = RamaScout.Manada,
                    FichaMedica = new FichaMedica
                    {
                        NumeroSeguridadSocial = "CIP-CLM-98234123",
                        AlergiasAlimentarias = "Cacahuetes",
                        RegimenAlimenticio = "Celíaca",
                        TelefonoEmergenciaPrincipal = "+34 600 111 222"
                    }
                },
                new Educando
                {
                    TutorLegalId = tutorLestonnac,
                    Nombre = "Marcos",
                    Apellidos = "García Pérez",
                    FechaNacimiento = new DateOnly(2014, 9, 20),
                    RamaActual = RamaScout.Tropa,
                    FichaMedica = new FichaMedica
                    {
                        NumeroSeguridadSocial = "CIP-CLM-77123990",
                        EnfermedadesCronicas = "Asma",
                        TelefonoEmergenciaPrincipal = "+34 600 111 222"
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // 2. Sembrar datos en San Vicente 412
        var sanVicenteContext = new TenantContext();
        sanVicenteContext.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 412", 412);

        var tutorSanVicente = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, sanVicenteContext))
        {
            var tutor = CrearTutor(tutorSanVicente, OrgSanVicente, "Laura Ruiz", "laura@sanvicente.com");
            db.Users.Add(tutor);

            db.Educandos.Add(
                new Educando
                {
                    TutorLegalId = tutorSanVicente,
                    Nombre = "Hugo",
                    Apellidos = "López Ruiz",
                    FechaNacimiento = new DateOnly(2019, 2, 10),
                    RamaActual = RamaScout.Castores,
                    FichaMedica = new FichaMedica
                    {
                        NumeroSeguridadSocial = "CIP-CLM-44321098",
                        AlergiasAlimentarias = "Huevo",
                        TelefonoEmergenciaPrincipal = "+34 611 222 333"
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // 3. Verificar consulta desde el contexto de Lestonnac
        using (var db = new WebScoutDbContext(_options, lestonnacContext))
        {
            var educandos = await db.Educandos.Include(p => p.FichaMedica).ToListAsync();
            Assert.Equal(2, educandos.Count);
            Assert.All(educandos, p => Assert.Equal(OrgLestonnac, p.OrganizacionId));
            Assert.Contains(educandos, p => p.Nombre == "Lucía");
            Assert.Contains(educandos, p => p.Nombre == "Marcos");
            Assert.DoesNotContain(educandos, p => p.Nombre == "Hugo");

            var fichas = await db.FichasMedicas.ToListAsync();
            Assert.Equal(2, fichas.Count);
            Assert.All(fichas, h => Assert.Equal(OrgLestonnac, h.OrganizacionId));
        }

        // 4. Verificar consulta desde el contexto de San Vicente
        using (var db = new WebScoutDbContext(_options, sanVicenteContext))
        {
            var educandos = await db.Educandos.Include(p => p.FichaMedica).ToListAsync();
            Assert.Single(educandos);
            Assert.Equal("Hugo", educandos[0].Nombre);
            Assert.Equal(OrgSanVicente, educandos[0].OrganizacionId);
            Assert.Equal(RamaScout.Castores, educandos[0].RamaActual);
        }
    }

    [Fact]
    public async Task InformeCocina_DebeFiltrarAlergenosNominalesCorrectamente()
    {
        // Arrange: Tenant Lestonnac con 2 educandos
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var tutor = CrearTutor(tutorId, OrgLestonnac, "María Pérez", "maria@test.com");
            db.Users.Add(tutor);

            db.Educandos.AddRange(
                new Educando
                {
                    TutorLegalId = tutorId,
                    Nombre = "Lucía",
                    Apellidos = "García Pérez",
                    FechaNacimiento = new DateOnly(2017, 4, 15),
                    RamaActual = RamaScout.Manada,
                    FichaMedica = new FichaMedica
                    {
                        NumeroSeguridadSocial = "CIP-1",
                        AlergiasAlimentarias = "Cacahuetes",
                        RegimenAlimenticio = "Celíaca",
                        TelefonoEmergenciaPrincipal = "+34 600 111 222"
                    }
                },
                new Educando
                {
                    TutorLegalId = tutorId,
                    Nombre = "Pablo",
                    Apellidos = "Navarro",
                    FechaNacimiento = new DateOnly(2017, 6, 1),
                    RamaActual = RamaScout.Manada,
                    FichaMedica = new FichaMedica
                    {
                        NumeroSeguridadSocial = "CIP-2",
                        AlergiasAlimentarias = null,
                        RegimenAlimenticio = null,
                        TelefonoEmergenciaPrincipal = "+34 600 222 333"
                    }
                }
            );
            await db.SaveChangesAsync();

            // Act: Simular la consulta del informe de cocina para Manada
            var informeCocina = await db.Educandos
                .Include(p => p.FichaMedica)
                .Where(p => p.RamaActual == RamaScout.Manada &&
                           (p.FichaMedica!.AlergiasAlimentarias != null || p.FichaMedica.RegimenAlimenticio != null))
                .ToListAsync();

            // Assert
            Assert.Single(informeCocina);
            Assert.Equal("Lucía", informeCocina[0].Nombre);
            Assert.Equal("Celíaca", informeCocina[0].FichaMedica?.RegimenAlimenticio);
            Assert.Equal("Cacahuetes", informeCocina[0].FichaMedica?.AlergiasAlimentarias);
        }
    }

    [Fact]
    public async Task Delete_Educando_DebeEliminarEnCascadaFichaMedicaAsociada()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var tutorId = Guid.NewGuid();
        Guid educandoId;
        Guid fichaMedicaId;

        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var tutor = CrearTutor(tutorId, OrgLestonnac, "María Pérez", "maria@test.com");
            db.Users.Add(tutor);

            var educando = new Educando
            {
                TutorLegalId = tutorId,
                Nombre = "Lucía",
                Apellidos = "García",
                FechaNacimiento = new DateOnly(2017, 4, 15),
                FichaMedica = new FichaMedica
                {
                    NumeroSeguridadSocial = "CIP-1",
                    TelefonoEmergenciaPrincipal = "+34 600 111 222"
                }
            };

            db.Educandos.Add(educando);
            await db.SaveChangesAsync();

            educandoId = educando.Id;
            fichaMedicaId = educando.FichaMedica.Id;
        }

        // Act: Eliminar el educando
        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var p = await db.Educandos.FindAsync(educandoId);
            Assert.NotNull(p);
            db.Educandos.Remove(p);
            await db.SaveChangesAsync();
        }

        // Assert: La ficha médica asociada debe haber desaparecido por cascada
        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var ficha = await db.FichasMedicas.FindAsync(fichaMedicaId);
            Assert.Null(ficha);
        }
    }

    public void Dispose()
    {
        _baseDatos.Dispose();
    }
}
