using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HydroponicIOT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Crops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CropStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    MinPh = table.Column<double>(type: "float", nullable: false),
                    MaxPh = table.Column<double>(type: "float", nullable: false),
                    MinTds = table.Column<double>(type: "float", nullable: false),
                    MaxTds = table.Column<double>(type: "float", nullable: false),
                    MinTemperature = table.Column<double>(type: "float", nullable: false),
                    MaxTemperature = table.Column<double>(type: "float", nullable: false),
                    MinHumidity = table.Column<double>(type: "float", nullable: false),
                    MaxHumidity = table.Column<double>(type: "float", nullable: false),
                    PumpOnTime = table.Column<int>(type: "int", nullable: false),
                    PumpOffTime = table.Column<int>(type: "int", nullable: false),
                    FanOnTime = table.Column<int>(type: "int", nullable: false),
                    FanOffTime = table.Column<int>(type: "int", nullable: false),
                    CropId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CropStages_Crops_CropId",
                        column: x => x.CropId,
                        principalTable: "Crops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MacAddress = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CropId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Crops_CropId",
                        column: x => x.CropId,
                        principalTable: "Crops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Devices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ActuatorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActuatorType = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ControlType = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActuatorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActuatorLogs_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SensorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ph = table.Column<double>(type: "float", nullable: true),
                    Tds = table.Column<double>(type: "float", nullable: true),
                    WaterTemperature = table.Column<double>(type: "float", nullable: true),
                    AirHumidity = table.Column<double>(type: "float", nullable: true),
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorLogs_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Crops",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Leafy green vegetable", true, "Lettuce" },
                    { 2, "Fruit vegetable", true, "Tomato" },
                    { 3, "Berry fruit", true, "Strawberry" }
                });

            migrationBuilder.InsertData(
                table: "CropStages",
                columns: new[] { "Id", "CropId", "Description", "FanOffTime", "FanOnTime", "MaxHumidity", "MaxPh", "MaxTds", "MaxTemperature", "MinHumidity", "MinPh", "MinTds", "MinTemperature", "Name", "PumpOffTime", "PumpOnTime", "StageOrder" },
                values: new object[,]
                {
                    { 1, 1, "Initial sprouting phase", 30, 10, 80.0, 6.5, 800.0, 24.0, 60.0, 5.5, 500.0, 18.0, "Germination", 15, 5, 1 },
                    { 2, 1, "Leaf development phase", 25, 15, 75.0, 6.2000000000000002, 1200.0, 25.0, 65.0, 5.7999999999999998, 800.0, 20.0, "Vegetative", 20, 10, 2 },
                    { 3, 1, "Ready for harvest", 20, 10, 70.0, 6.5, 900.0, 22.0, 60.0, 6.0, 600.0, 18.0, "Harvest", 10, 5, 3 }
                });

            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "CreatedAt", "CropId", "Description", "IsActive", "LastSeen", "MacAddress", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 4, 7, 56, 9, 332, DateTimeKind.Utc).AddTicks(7591), 1, "Main hydroponic system for lettuce", true, null, "AA:BB:CC:DD:EE:01", "Hydroponic Unit 1", null },
                    { 2, new DateTime(2026, 2, 4, 7, 56, 9, 332, DateTimeKind.Utc).AddTicks(7836), 1, "Secondary hydroponic system", true, null, "AA:BB:CC:DD:EE:02", "Hydroponic Unit 2", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActuatorLogs_DeviceId",
                table: "ActuatorLogs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_DeviceId",
                table: "Alerts",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_CropStages_CropId",
                table: "CropStages",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_CropId",
                table: "Devices",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorLogs_DeviceId",
                table: "SensorLogs",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActuatorLogs");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "CropStages");

            migrationBuilder.DropTable(
                name: "SensorLogs");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Crops");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
