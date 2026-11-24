using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso15AsignacionesSeccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsignacionesSeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rama = table.Column<int>(type: "integer", nullable: false),
                    Funcion = table.Column<int>(type: "integer", nullable: false),
                    AsignadaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AsignadaPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocadaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocadaPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesSeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesSeccion_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesSeccion_OrganizacionId_Rama",
                table: "AsignacionesSeccion",
                columns: new[] { "OrganizacionId", "Rama" });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesSeccion_OrganizacionId_UsuarioId",
                table: "AsignacionesSeccion",
                columns: new[] { "OrganizacionId", "UsuarioId" },
                unique: true,
                filter: "\"RevocadaUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesSeccion_UsuarioId",
                table: "AsignacionesSeccion",
                column: "UsuarioId");

            // Traspaso de datos: cada usuario con rol Scouter y RamaAsignada pasa a tener una asignación
            // vigente de función Scouter (0) en esa rama. RamaAsignada se conserva por compatibilidad,
            // pero la autorización ya solo consulta AsignacionesSeccion.
            migrationBuilder.Sql("""
                INSERT INTO "AsignacionesSeccion" ("Id", "OrganizacionId", "UsuarioId", "Rama", "Funcion", "AsignadaUtc")
                SELECT gen_random_uuid(), u."OrganizacionId", u."Id", u."RamaAsignada", 0, NOW()
                FROM "AspNetUsers" u
                WHERE u."RamaAsignada" <> 0
                  AND u."OrganizacionId" <> '00000000-0000-0000-0000-000000000000'
                  AND EXISTS (
                      SELECT 1
                      FROM "AspNetUserRoles" ur
                      JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
                      WHERE ur."UserId" = u."Id" AND r."Name" = 'Scouter');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesSeccion");
        }
    }
}
