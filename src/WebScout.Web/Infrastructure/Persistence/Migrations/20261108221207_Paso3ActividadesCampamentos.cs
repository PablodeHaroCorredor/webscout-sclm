using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso3ActividadesCampamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Publicada",
                table: "Actividades");

            migrationBuilder.DropColumn(name: "PrecioEnCentimos", table: "Actividades");
            migrationBuilder.AddColumn<int>(name: "Tipo", table: "Actividades", type: "integer", nullable: false, defaultValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "IbanCuentaBancaria",
                table: "Actividades",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Actividades",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConceptoPagoSugerido",
                table: "Actividades",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                table: "Actividades",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Actividades",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacionUtc",
                table: "Actividades",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialRequerido",
                table: "Actividades",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesSanitarias",
                table: "Actividades",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Actividades",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PuntoEncuentroRegreso",
                table: "Actividades",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PuntoEncuentroSalida",
                table: "Actividades",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RamasDestinatarias",
                table: "Actividades",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Actividades_CreadoPorUsuarioId",
                table: "Actividades",
                column: "CreadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Actividades_OrganizacionId_Estado",
                table: "Actividades",
                columns: new[] { "OrganizacionId", "Estado" });

            migrationBuilder.AddForeignKey(
                name: "FK_Actividades_AspNetUsers_CreadoPorUsuarioId",
                table: "Actividades",
                column: "CreadoPorUsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actividades_AspNetUsers_CreadoPorUsuarioId",
                table: "Actividades");

            migrationBuilder.DropIndex(
                name: "IX_Actividades_CreadoPorUsuarioId",
                table: "Actividades");

            migrationBuilder.DropIndex(
                name: "IX_Actividades_OrganizacionId_Estado",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "ConceptoPagoSugerido",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "FechaModificacionUtc",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "MaterialRequerido",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "ObservacionesSanitarias",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "PuntoEncuentroRegreso",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "PuntoEncuentroSalida",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "RamasDestinatarias",
                table: "Actividades");

            migrationBuilder.DropColumn(name: "Tipo", table: "Actividades");
            migrationBuilder.AddColumn<int>(name: "PrecioEnCentimos", table: "Actividades", type: "integer", nullable: false, defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "IbanCuentaBancaria",
                table: "Actividades",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Actividades",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Publicada",
                table: "Actividades",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}


