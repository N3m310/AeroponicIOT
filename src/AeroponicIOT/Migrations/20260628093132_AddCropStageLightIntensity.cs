using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddCropStageLightIntensity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "light_intensity_max",
                table: "crop_stages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "light_intensity_min",
                table: "crop_stages",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "light_intensity_max",
                table: "crop_stages");

            migrationBuilder.DropColumn(
                name: "light_intensity_min",
                table: "crop_stages");
        }
    }
}
