using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso4InscripcionesJustificantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActividadId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducandoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TutorLegalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaInscripcionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AutorizacionFirmada = table.Column<bool>(type: "boolean", nullable: false),
                    FechaAutorizacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImporteEuros = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    NombreArchivoJustificante = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RutaArchivoJustificante = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaSubidaJustificanteUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObservacionesFamilia = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NotasKraal = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RevisadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaRevisionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Actividades_ActividadId",
                        column: x => x.ActividadId,
                        principalTable: "Actividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_AspNetUsers_RevisadoPorUsuarioId",
                        column: x => x.RevisadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Inscripciones_AspNetUsers_TutorLegalId",
                        column: x => x.TutorLegalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Educandos_EducandoId",
                        column: x => x.EducandoId,
                        principalTable: "Educandos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_ActividadId",
                table: "Inscripciones",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_EducandoId",
                table: "Inscripciones",
                column: "EducandoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_OrganizacionId_ActividadId_EducandoId",
                table: "Inscripciones",
                columns: new[] { "OrganizacionId", "ActividadId", "EducandoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_OrganizacionId_Estado",
                table: "Inscripciones",
                columns: new[] { "OrganizacionId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_OrganizacionId_TutorLegalId",
                table: "Inscripciones",
                columns: new[] { "OrganizacionId", "TutorLegalId" });

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_RevisadoPorUsuarioId",
                table: "Inscripciones",
                column: "RevisadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_TutorLegalId",
                table: "Inscripciones",
                column: "TutorLegalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inscripciones");
        }
    }
}
