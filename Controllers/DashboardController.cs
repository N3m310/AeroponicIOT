using HydroponicIOT.Data;
using HydroponicIOT.DTOs;
using HydroponicIOT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HydroponicIOT.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestData()
    {
        try
        {
            var devices = await _context.Devices
                .Include(d => d.Crop)
                .Include(d => d.SensorLogs.OrderByDescending(sl => sl.Timestamp).Take(1))
                .ToListAsync();

            var activeAlerts = await _context.Alerts
                .Where(a => a.Status == Models.AlertStatus.Active)
                .Include(a => a.Device)
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToListAsync();

            var deviceStatuses = devices.Select(d => new DeviceStatusDto
            {
                Id = d.Id,
                Name = d.Name,
                MacAddress = d.MacAddress,
                IsActive = d.IsActive,
                LastSeen = d.LastSeen,
                CropName = d.Crop?.Name,
                LatestSensorData = d.SensorLogs.FirstOrDefault() != null ? new SensorDataDto
                {
                    MacAddress = d.MacAddress,
                    Ph = (double?)d.SensorLogs.First().Ph,
                    Tds = d.SensorLogs.First().Tds,
                    WaterTemperature = d.SensorLogs.First().WaterTemperature,
                    AirHumidity = d.SensorLogs.First().AirHumidity
                } : null
            }).ToList();

            var dashboard = new DashboardDto
            {
                Devices = deviceStatuses,
                ActiveAlerts = activeAlerts,
                TotalDevices = devices.Count,
                ActiveDevices = devices.Count(d => d.IsActive)
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("kpi")]
    public async Task<IActionResult> GetKeyPerformanceIndicators()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            var userRole = User.FindFirst("role")?.Value;

            // Get user's devices or all if admin
            IQueryable<Device> devicesQuery;
            if (userRole == "Administrator")
            {
                devicesQuery = _context.Devices;
            }
            else if (int.TryParse(userId, out var userIdInt))
            {
                devicesQuery = _context.Devices.Where(d => d.UserId == userIdInt);
            }
            else
            {
                return Unauthorized();
            }

            var devices = await devicesQuery
                .Include(d => d.SensorLogs.OrderByDescending(sl => sl.Timestamp).Take(1))
                .ToListAsync();

            // Calculate averages from latest sensor data
            var latestSensors = devices
                .Where(d => d.SensorLogs.Any())
                .Select(d => d.SensorLogs.First())
                .ToList();

            var avgPh = latestSensors.Any(s => s.Ph.HasValue)
                ? latestSensors.Where(s => s.Ph.HasValue).Average(s => s.Ph.Value)
                : 0.0;

            var avgTds = latestSensors.Any(s => s.Tds.HasValue)
                ? latestSensors.Where(s => s.Tds.HasValue).Average(s => s.Tds.Value)
                : 0.0;

            var avgTemp = latestSensors.Any(s => s.WaterTemperature.HasValue)
                ? latestSensors.Where(s => s.WaterTemperature.HasValue).Average(s => s.WaterTemperature.Value)
                : 0.0;

            // Calculate system health percentage
            var activeDevices = devices.Count(d => d.IsActive);
            var totalDevices = devices.Count;
            var deviceHealth = totalDevices > 0 ? (activeDevices * 100.0) / totalDevices : 0;

            // Check for critical alerts
            var criticalAlerts = await _context.Alerts
                .Where(a => a.Status == Models.AlertStatus.Active)
                .CountAsync();

            // Adjust health based on alerts
            var adjustedHealth = Math.Max(0, deviceHealth - (criticalAlerts * 5));

            var kpi = new
            {
                SystemHealth = Math.Round(adjustedHealth, 1),
                SystemHealthStatus = adjustedHealth >= 80 ? "Excellent" : 
                                    adjustedHealth >= 60 ? "Good" : 
                                    adjustedHealth >= 40 ? "Fair" : "Poor",
                AveragePh = Math.Round(avgPh, 2),
                AverageTds = Math.Round(avgTds, 1),
                AverageTemperature = Math.Round(avgTemp, 1),
                ActiveDevices = activeDevices,
                TotalDevices = totalDevices,
                ActiveAlerts = criticalAlerts,
                LatestUpdate = DateTime.UtcNow
            };

            return Ok(kpi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating KPI");
            return StatusCode(500, new { detail = "Error calculating KPI" });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetDeviceHealth()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            var userRole = User.FindFirst("role")?.Value;

            // Get user's devices
            IQueryable<Device> devicesQuery;
            if (userRole == "Administrator")
            {
                devicesQuery = _context.Devices;
            }
            else if (int.TryParse(userId, out var userIdInt))
            {
                devicesQuery = _context.Devices.Where(d => d.UserId == userIdInt);
            }
            else
            {
                return Unauthorized();
            }

            var devices = await devicesQuery
                .Include(d => d.SensorLogs.OrderByDescending(sl => sl.Timestamp).Take(1))
                .ToListAsync();

            var deviceHealth = devices.Select(d =>
            {
                // Determine health status based on latest sensor data and activity
                var lastSensor = d.SensorLogs.FirstOrDefault();
                var lastSeenMinutesAgo = d.LastSeen.HasValue 
                    ? (DateTime.UtcNow - d.LastSeen.Value).TotalMinutes 
                    : double.MaxValue;

                string status = "offline";
                if (lastSeenMinutesAgo < 5)
                {
                    status = lastSensor != null && 
                             lastSensor.Ph >= 5 && lastSensor.Ph <= 8 &&
                             lastSensor.Tds >= 400 && lastSensor.Tds <= 3000
                        ? "healthy"
                        : "warning";
                }
                else if (lastSeenMinutesAgo < 30)
                {
                    status = "warning";
                }

                return new
                {
                    d.Id,
                    d.Name,
                    Status = status,
                    LastSeen = d.LastSeen,
                    MinutesSinceActive = (int)Math.Min(lastSeenMinutesAgo, int.MaxValue),
                    LatestPh = lastSensor?.Ph,
                    LatestTds = lastSensor?.Tds,
                    LatestTemp = lastSensor?.WaterTemperature
                };
            }).ToList();

            return Ok(deviceHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device health");
            return StatusCode(500, new { detail = "Error getting device health" });
        }
    }

    [HttpGet("history/{deviceId}")]
    public async Task<IActionResult> GetDeviceHistory(int deviceId, [FromQuery] int hours = 24)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            var sensorLogs = await _context.SensorLogs
                .Where(sl => sl.DeviceId == deviceId && sl.Timestamp >= cutoffTime)
                .OrderBy(sl => sl.Timestamp)
                .ToListAsync();

            return Ok(sensorLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving device history");
            return StatusCode(500, "Internal server error");
        }
    }
}