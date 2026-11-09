using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso7SeguridadRgpdAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumeroSeguridadSocial",
                table: "FichasMedicas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmailUsuario = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Accion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoRecurso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecursoId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Detalles = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DireccionIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_OrganizacionId_Accion",
                table: "RegistrosAuditoria",
                columns: new[] { "OrganizacionId", "Accion" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_OrganizacionId_FechaCreacionUtc",
                table: "RegistrosAuditoria",
                columns: new[] { "OrganizacionId", "FechaCreacionUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroSeguridadSocial",
                table: "FichasMedicas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
