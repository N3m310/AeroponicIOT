using AeroponicIOT.Data;
using AeroponicIOT.DTOs;
using AeroponicIOT.Models;
using AeroponicIOT.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AeroponicIOT.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GardenController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GardenController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GardenController(
        ApplicationDbContext context,
        ILogger<GardenController> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGardens()
    {
        try
        {
            var currentUser = _currentUserService.GetCurrentUser();

            IQueryable<Garden> query = _context.Gardens
                .Include(g => g.Devices)
                .Include(g => g.CurrentCrop)
                .Include(g => g.Owners);

            if (!currentUser.IsAdministrator)
            {
                if (!currentUser.UserId.HasValue)
                {
                    return ApiProblem(StatusCodes.Status401Unauthorized, "Unauthorized", "User not authenticated");
                }
                query = query.Where(g => g.Owners.Any(o => o.Id == currentUser.UserId.Value));
            }

            var gardens = await query.ToListAsync();

            var dtos = gardens.Select(g => new GardenDto
            {
                Id = g.Id,
                Name = g.Name,
                Location = g.Location,
                Description = g.Description,
                CreatedAt = g.CreatedAt,
                DeviceCount = g.Devices?.Count ?? 0,
                CurrentCropId = g.CurrentCropId,
                CropName = g.CurrentCrop?.Name,
                OwnerIds = g.Owners.Select(o => o.Id).ToList(),
                OwnerNames = g.Owners.Select(o => o.Username ?? string.Empty).ToList()
            }).ToList();

            return Ok(ApiResponse.Success(dtos, "Gardens retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving gardens");
            return ApiProblem(StatusCodes.Status500InternalServerError, "Internal Server Error", "Error retrieving gardens");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGardenById(int id)
    {
        try
        {
            var currentUser = _currentUserService.GetCurrentUser();

            var garden = await _context.Gardens
                .Include(g => g.Devices)
                .Include(g => g.CurrentCrop)
                .Include(g => g.Owners)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (garden == null)
            {
                return ApiProblem(StatusCodes.Status404NotFound, "Not Found", "Garden not found");
            }

            if (!currentUser.IsAdministrator)
            {
                if (!currentUser.UserId.HasValue || !garden.Owners.Any(o => o.Id == currentUser.UserId.Value))
                {
                    return Forbid();
                }
            }

            var dto = new GardenDto
            {
                Id = garden.Id,
                Name = garden.Name,
                Location = garden.Location,
                Description = garden.Description,
                CreatedAt = garden.CreatedAt,
                DeviceCount = garden.Devices?.Count ?? 0,
                CurrentCropId = garden.CurrentCropId,
                CropName = garden.CurrentCrop?.Name,
                OwnerIds = garden.Owners.Select(o => o.Id).ToList(),
                OwnerNames = garden.Owners.Select(o => o.Username ?? string.Empty).ToList()
            };

            return Ok(ApiResponse.Success(dto, "Garden retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving garden {GardenId}", id);
            return ApiProblem(StatusCodes.Status500InternalServerError, "Internal Server Error", "Error retrieving garden");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateGarden([FromBody] GardenDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var garden = new Garden
            {
                Name = dto.Name,
                Location = dto.Location,
                Description = dto.Description,
                CurrentCropId = dto.CurrentCropId,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.OwnerIds != null && dto.OwnerIds.Count > 0)
            {
                var users = await _context.Users.Where(u => dto.OwnerIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    garden.Owners.Add(user);
                }
            }

            _context.Gardens.Add(garden);
            await _context.SaveChangesAsync();

            dto.Id = garden.Id;
            dto.CreatedAt = garden.CreatedAt;
            dto.DeviceCount = 0;
            if (garden.CurrentCropId.HasValue)
            {
                var crop = await _context.Crops.FindAsync(garden.CurrentCropId.Value);
                dto.CropName = crop?.Name;
            }

            return CreatedAtAction(nameof(GetGardenById), new { id = garden.Id }, ApiResponse.Success(dto, "Garden created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating garden");
            return ApiProblem(StatusCodes.Status500InternalServerError, "Internal Server Error", "Error creating garden");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateGarden(int id, [FromBody] GardenDto dto)
    {
        try
        {
            var garden = await _context.Gardens.Include(g => g.Owners).FirstOrDefaultAsync(g => g.Id == id);
            if (garden == null)
            {
                return ApiProblem(StatusCodes.Status404NotFound, "Not Found", "Garden not found");
            }

            var cropChanged = garden.CurrentCropId != dto.CurrentCropId;
            garden.Name = dto.Name;
            garden.Location = dto.Location;
            garden.Description = dto.Description;
            garden.CurrentCropId = dto.CurrentCropId;

            // Sync owners
            garden.Owners.Clear();
            if (dto.OwnerIds != null && dto.OwnerIds.Count > 0)
            {
                var users = await _context.Users.Where(u => dto.OwnerIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    garden.Owners.Add(user);
                }
            }

            _context.Gardens.Update(garden);
            await _context.SaveChangesAsync();

            if (cropChanged)
            {
                var devices = await _context.Devices.Where(d => d.GardenId == garden.Id).ToListAsync();
                foreach (var device in devices)
                {
                    if (device.CurrentCropId != garden.CurrentCropId)
                    {
                        device.CurrentCropId = garden.CurrentCropId;
                        device.CropAssignedAt = garden.CurrentCropId.HasValue ? DateTime.UtcNow : null;
                    }
                }
                await _context.SaveChangesAsync();
            }

            dto.Id = garden.Id;
            dto.CreatedAt = garden.CreatedAt;
            dto.DeviceCount = await _context.Devices.CountAsync(d => d.GardenId == garden.Id);
            if (garden.CurrentCropId.HasValue)
            {
                var crop = await _context.Crops.FindAsync(garden.CurrentCropId.Value);
                dto.CropName = crop?.Name;
            }

            return Ok(ApiResponse.Success(dto, "Garden updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating garden {GardenId}", id);
            return ApiProblem(StatusCodes.Status500InternalServerError, "Internal Server Error", "Error updating garden");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteGarden(int id)
    {
        try
        {
            var garden = await _context.Gardens
                .Include(g => g.Devices)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (garden == null)
            {
                return ApiProblem(StatusCodes.Status404NotFound, "Not Found", "Garden not found");
            }

            // Detach devices but do not delete them.
            foreach (var device in garden.Devices)
            {
                device.GardenId = null;
            }

            _context.Gardens.Remove(garden);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Success<object?>(null, "Garden deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting garden {GardenId}", id);
            return ApiProblem(StatusCodes.Status500InternalServerError, "Internal Server Error", "Error deleting garden");
        }
    }

    [HttpGet("{id}/devices")]
    public async Task<IActionResult> GetGardenDevices(int id)
    {
        try
        {
            var currentUser = _currentUserService.GetCurrentUser();

            var garden = await _context.Gardens.Include(g => g.Owners).FirstOrDefaultAsync(g => g.Id == id);
            if (garden == null)
            {
                return ApiProblem(StatusCodes.Status404NotFound, "Not Found", "Garden not found");
            }

            if (!currentUser.IsAdministrator)
            {
                if (!currentUser.UserId.HasValue || !garden.Owners.Any(o => o.Id == currentUser.UserId.Value))
                {
                    return Forbid();
                }
            }

            var devicesQuery = _context.Devices.Where(d => d.GardenId == id).Include(d => d.Crop).AsQueryable();
            var devices = await devicesQuery.ToListAsync();

            var dtos = devices.Select(d => new DeviceDto
            {
                Id = d.Id,
                Name = d.DeviceName ?? "Unknown Device",
                MacAddress = d.MacAddress,
                Status = d.Status,
                IsActive = d.IsActive,
                CurrentCropId = d.CurrentCropId,
                CropName = d.Crop?.Name,
                CreatedAt = d.CreatedAt,
                LastSeen = d.LastSeen
            }).ToList();

            return Ok(ApiResponse.Success(dtos, "Garden devices retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving devices for garden {GardenId}", id);
            return ApiProblem(StatusCodes.Status500InternalServerError, "Internal Server Error", "Error retrieving garden devices");
        }
    }

    private ObjectResult ApiProblem(int statusCode, string title, string detail)
    {
        return ProblemResponseFactory.Create(this, statusCode, title, detail);
    }
}

