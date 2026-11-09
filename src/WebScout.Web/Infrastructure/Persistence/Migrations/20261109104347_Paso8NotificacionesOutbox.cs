using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso8NotificacionesOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MensajesOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoEvento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClaveDeduplicacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DestinatarioEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DestinatarioNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Asunto = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ReplyToEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RemitenteNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CuerpoHtml = table.Column<string>(type: "text", nullable: false),
                    CuerpoTextoPlano = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Reintentos = table.Column<int>(type: "integer", nullable: false),
                    MaxReintentos = table.Column<int>(type: "integer", nullable: false),
                    ProximoReintentoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorUltimoReintento = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaProcesadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesOutbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesOutbox_Estado_ProximoReintentoUtc",
                table: "MensajesOutbox",
                columns: new[] { "Estado", "ProximoReintentoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesOutbox_FechaCreacionUtc",
                table: "MensajesOutbox",
                column: "FechaCreacionUtc");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesOutbox_OrganizacionId_ClaveDeduplicacion",
                table: "MensajesOutbox",
                columns: new[] { "OrganizacionId", "ClaveDeduplicacion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensajesOutbox");
        }
    }
}
