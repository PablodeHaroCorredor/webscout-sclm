using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantsAndHealthRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalGuardianId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrentBranch = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_AspNetUsers_LegalGuardianId",
                        column: x => x.LegalGuardianId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthInsuranceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HealthInsuranceEntity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FoodAllergies = table.Column<string>(type: "text", nullable: true),
                    MedicalAllergies = table.Column<string>(type: "text", nullable: true),
                    EnvironmentalAllergies = table.Column<string>(type: "text", nullable: true),
                    DietaryRequirements = table.Column<string>(type: "text", nullable: true),
                    ActiveMedication = table.Column<string>(type: "text", nullable: true),
                    ChronicConditions = table.Column<string>(type: "text", nullable: true),
                    VaccinationsUpToDate = table.Column<bool>(type: "boolean", nullable: false),
                    VaccinationObservations = table.Column<string>(type: "text", nullable: true),
                    CanSwim = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryEmergencyPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PrimaryEmergencyContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SecondaryEmergencyPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SecondaryEmergencyContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdditionalObservations = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthRecords_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_OrganizationId_ParticipantId",
                table: "HealthRecords",
                columns: new[] { "OrganizationId", "ParticipantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_ParticipantId",
                table: "HealthRecords",
                column: "ParticipantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_LegalGuardianId",
                table: "Participants",
                column: "LegalGuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_OrganizationId_CurrentBranch",
                table: "Participants",
                columns: new[] { "OrganizationId", "CurrentBranch" });

            migrationBuilder.CreateIndex(
                name: "IX_Participants_OrganizationId_IsActive",
                table: "Participants",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Participants_OrganizationId_LegalGuardianId",
                table: "Participants",
                columns: new[] { "OrganizationId", "LegalGuardianId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthRecords");

            migrationBuilder.DropTable(
                name: "Participants");
        }
    }
}
