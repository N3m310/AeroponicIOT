using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddGardenDeviceOneToOneConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_devices_garden_id",
                table: "devices");

            migrationBuilder.CreateIndex(
                name: "IX_devices_garden_id",
                table: "devices",
                column: "garden_id",
                unique: true,
                filter: "[garden_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_devices_garden_id",
                table: "devices");

            migrationBuilder.CreateIndex(
                name: "IX_devices_garden_id",
                table: "devices",
                column: "garden_id");
        }
    }
}
