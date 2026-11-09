using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso10AdmisionesBuzonContacto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MensajesContacto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreRemitente = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EmailRemitente = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TelefonoRemitente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Asunto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ConsentimientoPrivacidad = table.Column<bool>(type: "boolean", nullable: false),
                    Leido = table.Column<bool>(type: "boolean", nullable: false),
                    Respondido = table.Column<bool>(type: "boolean", nullable: false),
                    NotasKraal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaEnvioUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesContacto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesAdmision",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreEducando = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApellidosEducando = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    RamaSugerida = table.Column<int>(type: "integer", nullable: false),
                    NombreTutor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RelacionTutor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailTutor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TelefonoTutor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConsentimientoPrivacidad = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentimientoComunicaciones = table.Column<bool>(type: "boolean", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    NotasKraal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaSolicitudUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtendidoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAdmision", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesContacto_OrganizacionId_FechaEnvioUtc",
                table: "MensajesContacto",
                columns: new[] { "OrganizacionId", "FechaEnvioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesContacto_OrganizacionId_Leido",
                table: "MensajesContacto",
                columns: new[] { "OrganizacionId", "Leido" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAdmision_OrganizacionId_Estado",
                table: "SolicitudesAdmision",
                columns: new[] { "OrganizacionId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAdmision_OrganizacionId_FechaSolicitudUtc",
                table: "SolicitudesAdmision",
                columns: new[] { "OrganizacionId", "FechaSolicitudUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAdmision_OrganizacionId_RamaSugerida",
                table: "SolicitudesAdmision",
                columns: new[] { "OrganizacionId", "RamaSugerida" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensajesContacto");

            migrationBuilder.DropTable(
                name: "SolicitudesAdmision");
        }
    }
}
