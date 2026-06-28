namespace AeroponicIOT.DTOs;

public class SensorDataDto
{
    public string MacAddress { get; set; } = string.Empty;
    public double? Ph { get; set; }
    public double? Tds { get; set; }
    public double? WaterTemperature { get; set; }
    public double? AirHumidity { get; set; }
    // Optional light intensity (e.g., lux). Currently used via MQTT/HTTP payloads;
    // persistence depends on database schema support.
    public double? LightIntensity { get; set; }
}