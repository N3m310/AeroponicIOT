using AeroponicIOT.Models;

namespace AeroponicIOT.DTOs;

public class ActuatorControlDto
{
    public string MacAddress { get; set; } = string.Empty;
    public string ActuatorType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "ON" or "OFF"
    public string ControlType { get; set; } = "Manual";
    public string? Reason { get; set; }
}