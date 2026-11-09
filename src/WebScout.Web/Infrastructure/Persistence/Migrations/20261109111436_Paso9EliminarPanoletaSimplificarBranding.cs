using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso9EliminarPanoletaSimplificarBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Panoleta_BaseColorHex",
                table: "GruposScout");

            migrationBuilder.DropColumn(
                name: "Panoleta_BorderColorHex",
                table: "GruposScout");

            migrationBuilder.DropColumn(
                name: "Panoleta_CentralStripeColorHex",
                table: "GruposScout");

            migrationBuilder.DropColumn(
                name: "Panoleta_HasEmblem",
                table: "GruposScout");

            migrationBuilder.DropColumn(
                name: "Panoleta_SecondaryBorderColorHex",
                table: "GruposScout");

            migrationBuilder.DropColumn(
                name: "Panoleta_ShieldUrl",
                table: "GruposScout");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Panoleta_BaseColorHex",
                table: "GruposScout",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Panoleta_BorderColorHex",
                table: "GruposScout",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Panoleta_CentralStripeColorHex",
                table: "GruposScout",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Panoleta_HasEmblem",
                table: "GruposScout",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Panoleta_SecondaryBorderColorHex",
                table: "GruposScout",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Panoleta_ShieldUrl",
                table: "GruposScout",
                type: "text",
                nullable: true);
        }
    }
}
