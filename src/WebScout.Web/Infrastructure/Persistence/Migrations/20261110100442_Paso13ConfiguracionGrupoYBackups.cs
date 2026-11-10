using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso13ConfiguracionGrupoYBackups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesGrupo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cif = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DireccionBase = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmailOficial = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RondaSolarActiva = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaInicioRonda = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFinRonda = table.Column<DateOnly>(type: "date", nullable: false),
                    IbanCuentaBancaria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TitularCuenta = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ConceptoCuotaSugerido = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AdmisionesAbiertas = table.Column<bool>(type: "boolean", nullable: false),
                    MensajeBienvenidaAdmisiones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaUltimoBackupUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstadoUltimoBackup = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ObjetivoRpoHoras = table.Column<int>(type: "integer", nullable: false),
                    ObjetivoRtoHoras = table.Column<int>(type: "integer", nullable: false),
                    FechaModificacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesGrupo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesGrupo_OrganizacionId",
                table: "ConfiguracionesGrupo",
                column: "OrganizacionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesGrupo");
        }
    }
}
