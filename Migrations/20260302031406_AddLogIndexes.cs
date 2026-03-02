using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddLogIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sensor_logs_device_id",
                table: "sensor_logs");

            migrationBuilder.DropIndex(
                name: "IX_actuator_logs_device_id",
                table: "actuator_logs");

            migrationBuilder.CreateIndex(
                name: "IX_sensor_logs_device_id_timestamp",
                table: "sensor_logs",
                columns: new[] { "device_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_actuator_logs_device_id_timestamp",
                table: "actuator_logs",
                columns: new[] { "device_id", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sensor_logs_device_id_timestamp",
                table: "sensor_logs");

            migrationBuilder.DropIndex(
                name: "IX_actuator_logs_device_id_timestamp",
                table: "actuator_logs");

            migrationBuilder.CreateIndex(
                name: "IX_sensor_logs_device_id",
                table: "sensor_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_actuator_logs_device_id",
                table: "actuator_logs",
                column: "device_id");
        }
    }
}
