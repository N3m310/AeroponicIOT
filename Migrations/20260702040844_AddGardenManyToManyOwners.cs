using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddGardenManyToManyOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_gardens",
                columns: table => new
                {
                    garden_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_gardens", x => new { x.garden_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_user_gardens_gardens_garden_id",
                        column: x => x.garden_id,
                        principalTable: "gardens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_gardens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_gardens_user_id",
                table: "user_gardens",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_gardens");
        }
    }
}
