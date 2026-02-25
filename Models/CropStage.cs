using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HydroponicIOT.Models;

[Table("crop_stages")]
public class CropStage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("crop_id")]
    public int CropId { get; set; }

    [Column("stage_name")]
    [MaxLength(100)]
    public string? StageName { get; set; }

    [Column("day_start")]
    public int? DayStart { get; set; }

    [Column("day_end")]
    public int? DayEnd { get; set; }

    [Column("ppm_min")]
    public int? PpmMin { get; set; }

    [Column("ppm_max")]
    public int? PpmMax { get; set; }

    [Column("ph_min")]
    public decimal? PhMin { get; set; }

    [Column("ph_max")]
    public decimal? PhMax { get; set; }

    [Column("water_temp_min")]
    public int? WaterTempMin { get; set; }

    [Column("water_temp_max")]
    public int? WaterTempMax { get; set; }

    [Column("humidity_min")]
    public int? HumidityMin { get; set; }

    [Column("humidity_max")]
    public int? HumidityMax { get; set; }

    [Column("pump_on_minutes")]
    public int? PumpOnMinutes { get; set; }

    [Column("pump_off_minutes")]
    public int? PumpOffMinutes { get; set; }

    // For backward compatibility - map to existing properties
    [NotMapped]
    public string Name => StageName ?? "Unknown";

    [NotMapped]
    public string? Description => $"Days {DayStart}-{DayEnd}";

    [NotMapped]
    public int StageOrder => DayStart ?? 0;

    [NotMapped]
    public double MinPh => (double)(PhMin ?? 0);

    [NotMapped]
    public double MaxPh => (double)(PhMax ?? 0);

    [NotMapped]
    public double MinTds => PpmMin ?? 0;

    [NotMapped]
    public double MaxTds => PpmMax ?? 0;

    [NotMapped]
    public double MinTemperature => WaterTempMin ?? 0;

    [NotMapped]
    public double MaxTemperature => WaterTempMax ?? 0;

    [NotMapped]
    public double MinHumidity => HumidityMin ?? 0;

    [NotMapped]
    public double MaxHumidity => HumidityMax ?? 0;

    // Actuator timing rules
    [NotMapped]
    public int PumpOnTime => PumpOnMinutes ?? 5;
    [NotMapped]
    public int PumpOffTime => PumpOffMinutes ?? 10;
    [NotMapped]
    public int FanOnTime => 10; // Default since not in schema
    [NotMapped]
    public int FanOffTime => 20; // Default since not in schema

    // Foreign key to Crop
    public Crop Crop { get; set; } = null!;
}