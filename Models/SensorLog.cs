using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AeroponicIOT.Models;

[Table("sensor_logs")]
public class SensorLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int DeviceId { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("ph")]
    public decimal? Ph { get; set; }

    [Column("tds_ppm")]
    public int? TdsPpm { get; set; }

    [Column("water_temp")]
    public int? WaterTemp { get; set; }

    [Column("humidity")]
    public int? Humidity { get; set; }

    // For backward compatibility
    [NotMapped]
    public double? Tds => TdsPpm;

    [NotMapped]
    public double? WaterTemperature => WaterTemp;

    [NotMapped]
    public double? AirHumidity => Humidity;

    // Foreign key to Device
    public Device Device { get; set; } = null!;
}