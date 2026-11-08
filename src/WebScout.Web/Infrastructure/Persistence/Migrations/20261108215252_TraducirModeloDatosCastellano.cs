using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TraducirModeloDatosCastellano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "HealthRecords");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "AspNetUsers",
                newName: "OrganizacionId");

            migrationBuilder.RenameColumn(
                name: "LastLoginAtUtc",
                table: "AspNetUsers",
                newName: "UltimoAccesoUtc");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AspNetUsers",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "AspNetUsers",
                newName: "NombreCompleto");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "AspNetUsers",
                newName: "FechaCreacionUtc");

            migrationBuilder.RenameColumn(
                name: "AssignedBranch",
                table: "AspNetUsers",
                newName: "RamaAsignada");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_OrganizationId_NormalizedEmail",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_OrganizacionId_NormalizedEmail");

            migrationBuilder.CreateTable(
                name: "Actividades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Lugar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaLimiteInscripcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrecioEnCentimos = table.Column<int>(type: "integer", nullable: false),
                    IbanCuentaBancaria = table.Column<string>(type: "text", nullable: true),
                    Publicada = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actividades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Educandos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TutorLegalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Sexo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RamaActual = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educandos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Educandos_AspNetUsers_TutorLegalId",
                        column: x => x.TutorLegalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GruposScout",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NumeroGrupo = table.Column<int>(type: "integer", nullable: false),
                    Lema = table.Column<string>(type: "text", nullable: true),
                    EmailContacto = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Panoleta_BaseColorHex = table.Column<string>(type: "text", nullable: false),
                    Panoleta_BorderColorHex = table.Column<string>(type: "text", nullable: false),
                    Panoleta_CentralStripeColorHex = table.Column<string>(type: "text", nullable: true),
                    Panoleta_SecondaryBorderColorHex = table.Column<string>(type: "text", nullable: true),
                    Panoleta_HasEmblem = table.Column<bool>(type: "boolean", nullable: false),
                    Panoleta_ShieldUrl = table.Column<string>(type: "text", nullable: true),
                    Paleta_PrimaryHex = table.Column<string>(type: "text", nullable: false),
                    Paleta_SecondaryHex = table.Column<string>(type: "text", nullable: false),
                    Paleta_AccentHex = table.Column<string>(type: "text", nullable: false),
                    Paleta_TextOnPrimaryHex = table.Column<string>(type: "text", nullable: false),
                    Paleta_TextOnSecondaryHex = table.Column<string>(type: "text", nullable: false),
                    Paleta_SurfaceHeaderHex = table.Column<string>(type: "text", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GroupNumber = table.Column<int>(type: "integer", nullable: false),
                    Motto = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposScout", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FichasMedicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducandoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroSeguridadSocial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntidadAseguradora = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AlergiasAlimentarias = table.Column<string>(type: "text", nullable: true),
                    AlergiasMedicamentosas = table.Column<string>(type: "text", nullable: true),
                    AlergiasAmbientales = table.Column<string>(type: "text", nullable: true),
                    RegimenAlimenticio = table.Column<string>(type: "text", nullable: true),
                    MedicacionActiva = table.Column<string>(type: "text", nullable: true),
                    EnfermedadesCronicas = table.Column<string>(type: "text", nullable: true),
                    VacunacionAlDia = table.Column<bool>(type: "boolean", nullable: false),
                    ObservacionesVacunacion = table.Column<string>(type: "text", nullable: true),
                    SabeNadar = table.Column<bool>(type: "boolean", nullable: false),
                    TelefonoEmergenciaPrincipal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NombreContactoEmergenciaPrincipal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelefonoEmergenciaSecundario = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NombreContactoEmergenciaSecundario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ObservacionesAdicionales = table.Column<string>(type: "text", nullable: true),
                    UltimaActualizacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoPorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasMedicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FichasMedicas_Educandos_EducandoId",
                        column: x => x.EducandoId,
                        principalTable: "Educandos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actividades_OrganizacionId_FechaInicio",
                table: "Actividades",
                columns: new[] { "OrganizacionId", "FechaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Educandos_OrganizacionId_Activo",
                table: "Educandos",
                columns: new[] { "OrganizacionId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Educandos_OrganizacionId_RamaActual",
                table: "Educandos",
                columns: new[] { "OrganizacionId", "RamaActual" });

            migrationBuilder.CreateIndex(
                name: "IX_Educandos_OrganizacionId_TutorLegalId",
                table: "Educandos",
                columns: new[] { "OrganizacionId", "TutorLegalId" });

            migrationBuilder.CreateIndex(
                name: "IX_Educandos_TutorLegalId",
                table: "Educandos",
                column: "TutorLegalId");

            migrationBuilder.CreateIndex(
                name: "IX_FichasMedicas_EducandoId",
                table: "FichasMedicas",
                column: "EducandoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FichasMedicas_OrganizacionId_EducandoId",
                table: "FichasMedicas",
                columns: new[] { "OrganizacionId", "EducandoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GruposScout_Slug",
                table: "GruposScout",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actividades");

            migrationBuilder.DropTable(
                name: "FichasMedicas");

            migrationBuilder.DropTable(
                name: "GruposScout");

            migrationBuilder.DropTable(
                name: "Educandos");

            migrationBuilder.RenameColumn(
                name: "UltimoAccesoUtc",
                table: "AspNetUsers",
                newName: "LastLoginAtUtc");

            migrationBuilder.RenameColumn(
                name: "RamaAsignada",
                table: "AspNetUsers",
                newName: "AssignedBranch");

            migrationBuilder.RenameColumn(
                name: "OrganizacionId",
                table: "AspNetUsers",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "NombreCompleto",
                table: "AspNetUsers",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "FechaCreacionUtc",
                table: "AspNetUsers",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "AspNetUsers",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_OrganizacionId_NormalizedEmail",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_OrganizationId_NormalizedEmail");

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiresPaymentReceipt = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalGuardianId = table.Column<Guid>(type: "uuid", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentBranch = table.Column<int>(type: "integer", nullable: false),
                    Dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomDomain = table.Column<string>(type: "text", nullable: true),
                    GroupNumber = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Motto = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Palette_AccentHex = table.Column<string>(type: "text", nullable: false),
                    Palette_PrimaryHex = table.Column<string>(type: "text", nullable: false),
                    Palette_SecondaryHex = table.Column<string>(type: "text", nullable: false),
                    Palette_SurfaceHeaderHex = table.Column<string>(type: "text", nullable: false),
                    Palette_TextOnPrimaryHex = table.Column<string>(type: "text", nullable: false),
                    Palette_TextOnSecondaryHex = table.Column<string>(type: "text", nullable: false),
                    Panoleta_BaseColorHex = table.Column<string>(type: "text", nullable: false),
                    Panoleta_BorderColorHex = table.Column<string>(type: "text", nullable: false),
                    Panoleta_CentralStripeColorHex = table.Column<string>(type: "text", nullable: true),
                    Panoleta_HasEmblem = table.Column<bool>(type: "boolean", nullable: false),
                    Panoleta_SecondaryBorderColorHex = table.Column<string>(type: "text", nullable: true),
                    Panoleta_ShieldUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveMedication = table.Column<string>(type: "text", nullable: true),
                    AdditionalObservations = table.Column<string>(type: "text", nullable: true),
                    CanSwim = table.Column<bool>(type: "boolean", nullable: false),
                    ChronicConditions = table.Column<string>(type: "text", nullable: true),
                    DietaryRequirements = table.Column<string>(type: "text", nullable: true),
                    EnvironmentalAllergies = table.Column<string>(type: "text", nullable: true),
                    FoodAllergies = table.Column<string>(type: "text", nullable: true),
                    HealthInsuranceEntity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HealthInsuranceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    MedicalAllergies = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryEmergencyContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrimaryEmergencyPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SecondaryEmergencyContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SecondaryEmergencyPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VaccinationObservations = table.Column<string>(type: "text", nullable: true),
                    VaccinationsUpToDate = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "IX_Activities_OrganizationId_StartDate",
                table: "Activities",
                columns: new[] { "OrganizationId", "StartDate" });

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

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);
        }
    }
}
