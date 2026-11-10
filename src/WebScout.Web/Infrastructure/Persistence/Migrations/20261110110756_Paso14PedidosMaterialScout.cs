using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso14PedidosMaterialScout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidosMaterial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoPedido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitanteNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SolicitanteEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SolicitanteTelefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EducandoId = table.Column<Guid>(type: "uuid", nullable: true),
                    EducandoNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RamaEducativa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoEstado = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ObservacionesFamilia = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NotasInternasKraal = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TotalEstimado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaSolicitudUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaUltimoCambioEstadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModificadoPor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ComprobantePagoRuta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ComprobantePagoNombreOriginal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ComprobantePagoFechaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosMaterial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductosMaterial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TallasDisponiblesCsv = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    OrdenVisualizacion = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosMaterial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineasPedidoMaterial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoMaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductoNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Talla = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasPedidoMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasPedidoMaterial_PedidosMaterial_PedidoMaterialId",
                        column: x => x.PedidoMaterialId,
                        principalTable: "PedidosMaterial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineasPedidoMaterial_ProductosMaterial_ProductoMaterialId",
                        column: x => x.ProductoMaterialId,
                        principalTable: "ProductosMaterial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineasPedidoMaterial_OrganizacionId_PedidoMaterialId",
                table: "LineasPedidoMaterial",
                columns: new[] { "OrganizacionId", "PedidoMaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasPedidoMaterial_OrganizacionId_ProductoMaterialId",
                table: "LineasPedidoMaterial",
                columns: new[] { "OrganizacionId", "ProductoMaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasPedidoMaterial_PedidoMaterialId",
                table: "LineasPedidoMaterial",
                column: "PedidoMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasPedidoMaterial_ProductoMaterialId",
                table: "LineasPedidoMaterial",
                column: "ProductoMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosMaterial_OrganizacionId_CodigoPedido",
                table: "PedidosMaterial",
                columns: new[] { "OrganizacionId", "CodigoPedido" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidosMaterial_OrganizacionId_Estado",
                table: "PedidosMaterial",
                columns: new[] { "OrganizacionId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosMaterial_OrganizacionId_FechaSolicitudUtc",
                table: "PedidosMaterial",
                columns: new[] { "OrganizacionId", "FechaSolicitudUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosMaterial_OrganizacionId_UsuarioId",
                table: "PedidosMaterial",
                columns: new[] { "OrganizacionId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductosMaterial_OrganizacionId_Activo",
                table: "ProductosMaterial",
                columns: new[] { "OrganizacionId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductosMaterial_OrganizacionId_Categoria",
                table: "ProductosMaterial",
                columns: new[] { "OrganizacionId", "Categoria" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineasPedidoMaterial");

            migrationBuilder.DropTable(
                name: "PedidosMaterial");

            migrationBuilder.DropTable(
                name: "ProductosMaterial");
        }
    }
}
