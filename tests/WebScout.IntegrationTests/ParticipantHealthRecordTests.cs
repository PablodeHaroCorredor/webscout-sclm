using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;
using Xunit;

namespace WebScout.IntegrationTests;

public class ParticipantHealthRecordTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<WebScoutDbContext> _options;

    private static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ParticipantHealthRecordTests()
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

    private static ScoutUser CreateGuardian(Guid id, Guid organizationId, string name, string email)
    {
        return new ScoutUser
        {
            Id = id,
            OrganizationId = organizationId,
            FullName = name,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    [Fact]
    public void CalculateAge_ShouldCalculateExactAgeAccurately()
    {
        // Arrange
        var reference = new DateOnly(2026, 11, 8);
        var participant1 = new ScoutParticipant
        {
            FirstName = "Lucía",
            LastName = "García",
            BirthDate = new DateOnly(2017, 4, 15) // Ya cumplió 9 años
        };
        var participant2 = new ScoutParticipant
        {
            FirstName = "Carlos",
            LastName = "García",
            BirthDate = new DateOnly(2017, 12, 1) // Todavía tiene 8 años a 8 de noviembre
        };

        // Act & Assert
        Assert.Equal(9, participant1.CalculateAge(reference));
        Assert.Equal(8, participant2.CalculateAge(reference));
    }

    [Fact]
    public async Task Insert_ParticipantAndHealthRecord_ShouldAutomaticallyStampOrganizationId()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var guardianId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var guardian = CreateGuardian(guardianId, OrgLestonnac, "María Pérez", "maria@test.com");
            db.Users.Add(guardian);
            await db.SaveChangesAsync();

            var participant = new ScoutParticipant
            {
                LegalGuardianId = guardianId,
                FirstName = "Lucía",
                LastName = "García Pérez",
                BirthDate = new DateOnly(2017, 4, 15),
                CurrentBranch = ScoutBranch.Manada,
                HealthRecord = new ParticipantHealthRecord
                {
                    HealthInsuranceNumber = "CIP-CLM-98234123",
                    FoodAllergies = "Frutos secos",
                    DietaryRequirements = "Celíaca",
                    CanSwim = true,
                    PrimaryEmergencyPhone = "+34 600 111 222"
                }
            };

            // Act
            db.Participants.Add(participant);
            await db.SaveChangesAsync();

            // Assert
            Assert.Equal(OrgLestonnac, participant.OrganizationId);
            Assert.NotNull(participant.HealthRecord);
            Assert.Equal(OrgLestonnac, participant.HealthRecord.OrganizationId);
            Assert.Equal(participant.Id, participant.HealthRecord.ParticipantId);
        }
    }

    [Fact]
    public async Task QueryFilter_ShouldStrictlyIsolateParticipantsAndHealthRecordsBetweenTenants()
    {
        // 1. Sembrar datos en Lestonnac 330
        var lestonnacContext = new TenantContext();
        lestonnacContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var guardianLestonnac = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, lestonnacContext))
        {
            var guardian = CreateGuardian(guardianLestonnac, OrgLestonnac, "María Pérez", "maria@lestonnac.com");
            db.Users.Add(guardian);

            db.Participants.AddRange(
                new ScoutParticipant
                {
                    LegalGuardianId = guardianLestonnac,
                    FirstName = "Lucía",
                    LastName = "García Pérez",
                    BirthDate = new DateOnly(2017, 4, 15),
                    CurrentBranch = ScoutBranch.Manada,
                    HealthRecord = new ParticipantHealthRecord
                    {
                        HealthInsuranceNumber = "CIP-CLM-98234123",
                        FoodAllergies = "Cacahuetes",
                        DietaryRequirements = "Celíaca",
                        PrimaryEmergencyPhone = "+34 600 111 222"
                    }
                },
                new ScoutParticipant
                {
                    LegalGuardianId = guardianLestonnac,
                    FirstName = "Marcos",
                    LastName = "García Pérez",
                    BirthDate = new DateOnly(2014, 9, 20),
                    CurrentBranch = ScoutBranch.Tropa,
                    HealthRecord = new ParticipantHealthRecord
                    {
                        HealthInsuranceNumber = "CIP-CLM-77123990",
                        ChronicConditions = "Asma",
                        PrimaryEmergencyPhone = "+34 600 111 222"
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // 2. Sembrar datos en San Vicente 412
        var sanVicenteContext = new TenantContext();
        sanVicenteContext.SetTenant(OrgSanVicente, "sanvicente", "San Vicente 412", 412);

        var guardianSanVicente = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, sanVicenteContext))
        {
            var guardian = CreateGuardian(guardianSanVicente, OrgSanVicente, "Laura Ruiz", "laura@sanvicente.com");
            db.Users.Add(guardian);

            db.Participants.Add(
                new ScoutParticipant
                {
                    LegalGuardianId = guardianSanVicente,
                    FirstName = "Hugo",
                    LastName = "López Ruiz",
                    BirthDate = new DateOnly(2019, 2, 10),
                    CurrentBranch = ScoutBranch.Castores,
                    HealthRecord = new ParticipantHealthRecord
                    {
                        HealthInsuranceNumber = "CIP-CLM-44321098",
                        FoodAllergies = "Huevo",
                        PrimaryEmergencyPhone = "+34 611 222 333"
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // 3. Verificar consulta desde el contexto de Lestonnac
        using (var db = new WebScoutDbContext(_options, lestonnacContext))
        {
            var participants = await db.Participants.Include(p => p.HealthRecord).ToListAsync();
            Assert.Equal(2, participants.Count);
            Assert.All(participants, p => Assert.Equal(OrgLestonnac, p.OrganizationId));
            Assert.Contains(participants, p => p.FirstName == "Lucía");
            Assert.Contains(participants, p => p.FirstName == "Marcos");
            Assert.DoesNotContain(participants, p => p.FirstName == "Hugo");

            var healthRecords = await db.HealthRecords.ToListAsync();
            Assert.Equal(2, healthRecords.Count);
            Assert.All(healthRecords, h => Assert.Equal(OrgLestonnac, h.OrganizationId));
        }

        // 4. Verificar consulta desde el contexto de San Vicente
        using (var db = new WebScoutDbContext(_options, sanVicenteContext))
        {
            var participants = await db.Participants.Include(p => p.HealthRecord).ToListAsync();
            Assert.Single(participants);
            Assert.Equal("Hugo", participants[0].FirstName);
            Assert.Equal(OrgSanVicente, participants[0].OrganizationId);
            Assert.Equal(ScoutBranch.Castores, participants[0].CurrentBranch);
        }
    }

    [Fact]
    public async Task KitchenAllergyReport_ShouldFilterNominalAllergensAccurately()
    {
        // Arrange: Tenant Lestonnac con 2 educandos
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var guardianId = Guid.NewGuid();

        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var guardian = CreateGuardian(guardianId, OrgLestonnac, "María Pérez", "maria@test.com");
            db.Users.Add(guardian);

            db.Participants.AddRange(
                new ScoutParticipant
                {
                    LegalGuardianId = guardianId,
                    FirstName = "Lucía",
                    LastName = "García Pérez",
                    BirthDate = new DateOnly(2017, 4, 15),
                    CurrentBranch = ScoutBranch.Manada,
                    HealthRecord = new ParticipantHealthRecord
                    {
                        HealthInsuranceNumber = "CIP-1",
                        FoodAllergies = "Cacahuetes",
                        DietaryRequirements = "Celíaca",
                        PrimaryEmergencyPhone = "+34 600 111 222"
                    }
                },
                new ScoutParticipant
                {
                    LegalGuardianId = guardianId,
                    FirstName = "Pablo",
                    LastName = "Navarro",
                    BirthDate = new DateOnly(2017, 6, 1),
                    CurrentBranch = ScoutBranch.Manada,
                    HealthRecord = new ParticipantHealthRecord
                    {
                        HealthInsuranceNumber = "CIP-2",
                        FoodAllergies = null,
                        DietaryRequirements = null,
                        PrimaryEmergencyPhone = "+34 600 222 333"
                    }
                }
            );
            await db.SaveChangesAsync();

            // Act: Simular la consulta del informe de cocina para Manada
            var kitchenReport = await db.Participants
                .Include(p => p.HealthRecord)
                .Where(p => p.CurrentBranch == ScoutBranch.Manada &&
                           (p.HealthRecord!.FoodAllergies != null || p.HealthRecord.DietaryRequirements != null))
                .ToListAsync();

            // Assert
            Assert.Single(kitchenReport);
            Assert.Equal("Lucía", kitchenReport[0].FirstName);
            Assert.Equal("Celíaca", kitchenReport[0].HealthRecord?.DietaryRequirements);
            Assert.Equal("Cacahuetes", kitchenReport[0].HealthRecord?.FoodAllergies);
        }
    }

    [Fact]
    public async Task Delete_Participant_ShouldCascadeDeleteAssociatedHealthRecord()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(OrgLestonnac, "lestonnac", "Lestonnac 330", 330);

        var guardianId = Guid.NewGuid();
        Guid participantId;
        Guid healthRecordId;

        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var guardian = CreateGuardian(guardianId, OrgLestonnac, "María Pérez", "maria@test.com");
            db.Users.Add(guardian);

            var participant = new ScoutParticipant
            {
                LegalGuardianId = guardianId,
                FirstName = "Lucía",
                LastName = "García",
                BirthDate = new DateOnly(2017, 4, 15),
                HealthRecord = new ParticipantHealthRecord
                {
                    HealthInsuranceNumber = "CIP-1",
                    PrimaryEmergencyPhone = "+34 600 111 222"
                }
            };

            db.Participants.Add(participant);
            await db.SaveChangesAsync();

            participantId = participant.Id;
            healthRecordId = participant.HealthRecord.Id;
        }

        // Act: Eliminar el educando
        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var p = await db.Participants.FindAsync(participantId);
            Assert.NotNull(p);
            db.Participants.Remove(p);
            await db.SaveChangesAsync();
        }

        // Assert: La ficha médica asociada debe haber desaparecido por cascada
        using (var db = new WebScoutDbContext(_options, tenantContext))
        {
            var healthRecord = await db.HealthRecords.FindAsync(healthRecordId);
            Assert.Null(healthRecord);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
