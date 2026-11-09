using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Paso5DossierAsistenciaCampamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCheckInRegresoUtc",
                table: "Inscripciones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCheckInSalidaUtc",
                table: "Inscripciones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesAsistencia",
                table: "Inscripciones",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PresenteEnRegreso",
                table: "Inscripciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PresenteEnSalida",
                table: "Inscripciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCheckInRegresoUtc",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "FechaCheckInSalidaUtc",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "ObservacionesAsistencia",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "PresenteEnRegreso",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "PresenteEnSalida",
                table: "Inscripciones");
        }
    }
}
