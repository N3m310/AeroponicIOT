using AeroponicIOT.Models;

namespace AeroponicIOT.DTOs;

public class GardenDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int DeviceCount { get; set; }
}

