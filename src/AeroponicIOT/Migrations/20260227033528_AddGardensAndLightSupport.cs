using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddGardensAndLightSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "light_intensity",
                table: "sensor_logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "garden_id",
                table: "devices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gardens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gardens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_devices_garden_id",
                table: "devices",
                column: "garden_id");

            migrationBuilder.AddForeignKey(
                name: "FK_devices_gardens_garden_id",
                table: "devices",
                column: "garden_id",
                principalTable: "gardens",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_devices_gardens_garden_id",
                table: "devices");

            migrationBuilder.DropTable(
                name: "gardens");

            migrationBuilder.DropIndex(
                name: "IX_devices_garden_id",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "light_intensity",
                table: "sensor_logs");

            migrationBuilder.DropColumn(
                name: "garden_id",
                table: "devices");
        }
    }
}
