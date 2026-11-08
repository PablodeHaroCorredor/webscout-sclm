using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScout.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialTenancyAndActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiresPaymentReceipt = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GroupNumber = table.Column<int>(type: "integer", nullable: false),
                    CustomDomain = table.Column<string>(type: "text", nullable: true),
                    Motto = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Panoleta_BaseColorHex = table.Column<string>(type: "text", nullable: false),
                    Panoleta_BorderColorHex = table.Column<string>(type: "text", nullable: false),
                    Panoleta_CentralStripeColorHex = table.Column<string>(type: "text", nullable: true),
                    Panoleta_SecondaryBorderColorHex = table.Column<string>(type: "text", nullable: true),
                    Panoleta_HasEmblem = table.Column<bool>(type: "boolean", nullable: false),
                    Panoleta_ShieldUrl = table.Column<string>(type: "text", nullable: true),
                    Palette_PrimaryHex = table.Column<string>(type: "text", nullable: false),
                    Palette_SecondaryHex = table.Column<string>(type: "text", nullable: false),
                    Palette_AccentHex = table.Column<string>(type: "text", nullable: false),
                    Palette_TextOnPrimaryHex = table.Column<string>(type: "text", nullable: false),
                    Palette_TextOnSecondaryHex = table.Column<string>(type: "text", nullable: false),
                    Palette_SurfaceHeaderHex = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_OrganizationId_StartDate",
                table: "Activities",
                columns: new[] { "OrganizationId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
