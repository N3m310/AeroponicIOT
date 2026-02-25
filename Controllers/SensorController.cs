using HydroponicIOT.Data;
using HydroponicIOT.DTOs;
using HydroponicIOT.Models;
using HydroponicIOT.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HydroponicIOT.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SensorController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SensorController> _logger;

    public SensorController(ApplicationDbContext context, INotificationService notificationService, ILogger<SensorController> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous] // Allow IoT devices to send data without authentication
    public async Task<IActionResult> ReceiveSensorData([FromBody] SensorDataDto sensorData)
    {
        try
        {
            // Find device by MAC address
            var device = await _context.Devices
                .Include(d => d.Crop)
                .ThenInclude(c => c.CropStages)
                .FirstOrDefaultAsync(d => d.MacAddress == sensorData.MacAddress);

            if (device == null)
            {
                _logger.LogWarning("Device with MAC {MacAddress} not found", sensorData.MacAddress);
                return NotFound($"Device with MAC address {sensorData.MacAddress} not found");
            }

            // Update device last seen
            device.LastSeen = DateTime.UtcNow;
            _context.Devices.Update(device);

            // Create sensor log
            var sensorLog = new SensorLog
            {
                DeviceId = device.Id,
                Ph = sensorData.Ph != null ? (decimal?)sensorData.Ph : null,
                TdsPpm = sensorData.Tds != null ? (int?)sensorData.Tds : null,
                WaterTemp = sensorData.WaterTemperature != null ? (int?)sensorData.WaterTemperature : null,
                Humidity = sensorData.AirHumidity != null ? (int?)sensorData.AirHumidity : null,
                Timestamp = DateTime.UtcNow
            };

            _context.SensorLogs.Add(sensorLog);

            // Check for alerts and automatic control
            await CheckThresholdsAndControl(device, sensorLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sensor data received from device {DeviceName} ({MacAddress})",
                device.Name, device.MacAddress);

            return Ok(new { message = "Sensor data received successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor data");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task CheckThresholdsAndControl(Device device, SensorLog sensorLog)
    {
        if (device.Crop == null) return;

        // Get current crop stage (simplified - in real implementation, you'd track current stage)
        var currentStage = device.Crop?.CropStages.OrderBy(cs => cs.StageOrder).FirstOrDefault();
        if (currentStage == null) return;

        var alerts = new List<Alert>();

        // Check pH
        if (sensorLog.Ph.HasValue)
        {
            if ((double)sensorLog.Ph < currentStage.MinPh || (double)sensorLog.Ph > currentStage.MaxPh)
            {
                alerts.Add(new Alert
                {
                    DeviceId = device.Id,
                    AlertType = "Warning",
                    Message = $"pH level {sensorLog.Ph:F2} is outside acceptable range ({currentStage.MinPh:F1}-{currentStage.MaxPh:F1})",
                    Severity = "Medium",
                    IsResolved = false
                });
            }
        }

        // Check TDS
        if (sensorLog.TdsPpm.HasValue)
        {
            if (sensorLog.TdsPpm < currentStage.MinTds || sensorLog.TdsPpm > currentStage.MaxTds)
            {
                alerts.Add(new Alert
                {
                    DeviceId = device.Id,
                    AlertType = "Warning",
                    Message = $"TDS level {sensorLog.TdsPpm:F0} ppm is outside acceptable range ({currentStage.MinTds:F0}-{currentStage.MaxTds:F0} ppm)",
                    Severity = "Medium",
                    IsResolved = false
                });
            }
        }

        // Check temperature
        if (sensorLog.WaterTemp.HasValue)
        {
            if (sensorLog.WaterTemp < currentStage.MinTemperature || sensorLog.WaterTemp > currentStage.MaxTemperature)
            {
                alerts.Add(new Alert
                {
                    DeviceId = device.Id,
                    AlertType = "Warning",
                    Message = $"Water temperature {sensorLog.WaterTemp:F1}°C is outside acceptable range ({currentStage.MinTemperature:F1}-{currentStage.MaxTemperature:F1}°C)",
                    Severity = "High",
                    IsResolved = false
                });
            }
        }

        // Check humidity
        if (sensorLog.Humidity.HasValue)
        {
            if (sensorLog.Humidity < currentStage.MinHumidity || sensorLog.Humidity > currentStage.MaxHumidity)
            {
                alerts.Add(new Alert
                {
                    DeviceId = device.Id,
                    AlertType = "Warning",
                    Message = $"Air humidity {sensorLog.Humidity:F1}% is outside acceptable range ({currentStage.MinHumidity:F1}-{currentStage.MaxHumidity:F1}%)",
                    Severity = "Low",
                    IsResolved = false
                });
            }
        }

        // Add alerts to context
        foreach (var alert in alerts)
        {
            _context.Alerts.Add(alert);
            
            // Send notification for this alert
            await _notificationService.SendAlertNotificationAsync(
                device.Id,
                alert.Message ?? "Alert",
                alert.Message ?? "An alert has been triggered",
                alert.Severity ?? "Medium"
            );
        }

        // TODO: Implement automatic actuator control based on thresholds
        // This would involve checking timing rules and sending control commands
    }
}