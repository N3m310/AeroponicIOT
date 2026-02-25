using AeroponicIOT.Data;
using AeroponicIOT.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AeroponicIOT.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CropController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CropController> _logger;

    public CropController(ApplicationDbContext context, ILogger<CropController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCrops()
    {
        try
        {
            var crops = await _context.Crops
                .Include(c => c.CropStages)
                .ToListAsync();

            var cropDtos = crops.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.TotalDaysEst,
                c.CreatedAt,
                StageCount = c.CropStages?.Count ?? 0
            }).ToList();

            return Ok(cropDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crops");
            return StatusCode(500, new { detail = "Error retrieving crops" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCropById(int id)
    {
        try
        {
            var crop = await _context.Crops
                .Include(c => c.CropStages)
                .Include(c => c.Devices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (crop == null)
            {
                _logger.LogWarning("Crop {CropId} not found", id);
                return NotFound(new { detail = "Crop not found" });
            }

            var cropDto = new
            {
                crop.Id,
                crop.Name,
                crop.Description,
                crop.TotalDaysEst,
                crop.CreatedAt,
                Stages = crop.CropStages?.Select(s => new { 
                    s.Id, 
                    s.StageName, 
                    s.DayStart,
                    s.DayEnd,
                    s.PhMin,
                    s.PhMax,
                    s.PpmMin,
                    s.PpmMax,
                    s.WaterTempMin,
                    s.WaterTempMax,
                    s.HumidityMin,
                    s.HumidityMax
                }).ToList(),
                DeviceCount = crop.Devices?.Count ?? 0
            };

            return Ok(cropDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crop {CropId}", id);
            return StatusCode(500, new { detail = "Error retrieving crop" });
        }
    }
}
