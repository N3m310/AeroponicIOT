using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddGardenCropRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "current_crop_id",
                table: "gardens",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_gardens_current_crop_id",
                table: "gardens",
                column: "current_crop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_gardens_crops_current_crop_id",
                table: "gardens",
                column: "current_crop_id",
                principalTable: "crops",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gardens_crops_current_crop_id",
                table: "gardens");

            migrationBuilder.DropIndex(
                name: "IX_gardens_current_crop_id",
                table: "gardens");

            migrationBuilder.DropColumn(
                name: "current_crop_id",
                table: "gardens");
        }
    }
}
