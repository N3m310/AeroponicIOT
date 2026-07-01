using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddCropStageLightThresholds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "light_max",
                table: "crop_stages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "light_min",
                table: "crop_stages",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "light_max",
                table: "crop_stages");

            migrationBuilder.DropColumn(
                name: "light_min",
                table: "crop_stages");
        }
    }
}
