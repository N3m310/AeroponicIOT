using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crops",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total_days_est = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crops", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crop_stages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    crop_id = table.Column<int>(type: "int", nullable: false),
                    stage_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    day_start = table.Column<int>(type: "int", nullable: true),
                    day_end = table.Column<int>(type: "int", nullable: true),
                    ppm_min = table.Column<int>(type: "int", nullable: true),
                    ppm_max = table.Column<int>(type: "int", nullable: true),
                    ph_min = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ph_max = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    water_temp_min = table.Column<int>(type: "int", nullable: true),
                    water_temp_max = table.Column<int>(type: "int", nullable: true),
                    humidity_min = table.Column<int>(type: "int", nullable: true),
                    humidity_max = table.Column<int>(type: "int", nullable: true),
                    pump_on_minutes = table.Column<int>(type: "int", nullable: true),
                    pump_off_minutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crop_stages", x => x.id);
                    table.ForeignKey(
                        name: "FK_crop_stages_crops_crop_id",
                        column: x => x.crop_id,
                        principalTable: "crops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    mac_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    current_crop_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_seen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                    table.ForeignKey(
                        name: "FK_devices_crops_current_crop_id",
                        column: x => x.current_crop_id,
                        principalTable: "crops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_devices_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_devices_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<int>(type: "int", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "actuator_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    actuator_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    action = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actuator_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_actuator_logs_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    alert_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_resolved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.id);
                    table.ForeignKey(
                        name: "FK_alerts_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "automation_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    rule_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    rule_type = table.Column<int>(type: "int", nullable: false),
                    actuator_type = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    condition_parameter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    condition_value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    condition_operator = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    schedule_time = table.Column<TimeOnly>(type: "time", nullable: true),
                    schedule_days = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_executed = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_rules_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sensor_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ph = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    tds_ppm = table.Column<int>(type: "int", nullable: true),
                    water_temp = table.Column<int>(type: "int", nullable: true),
                    humidity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensor_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_sensor_logs_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_actuator_logs_device_id",
                table: "actuator_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_device_id",
                table: "alerts",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_rules_device_id",
                table: "automation_rules",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_crop_stages_crop_id",
                table: "crop_stages",
                column: "crop_id");

            migrationBuilder.CreateIndex(
                name: "IX_devices_current_crop_id",
                table: "devices",
                column: "current_crop_id");

            migrationBuilder.CreateIndex(
                name: "IX_devices_user_id",
                table: "devices",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_devices_UserId1",
                table: "devices",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sensor_logs_device_id",
                table: "sensor_logs",
                column: "device_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "actuator_logs");

            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "automation_rules");

            migrationBuilder.DropTable(
                name: "crop_stages");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "sensor_logs");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "crops");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
