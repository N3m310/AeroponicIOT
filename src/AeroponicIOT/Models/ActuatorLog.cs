using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AeroponicIOT.Models;

public enum ActuatorType
{
    Pump,
    Fan,
    Light,
    Heater
}

public enum ControlType
{
    Automatic,
    Manual
}

[Table("actuator_logs")]
public class ActuatorLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int DeviceId { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("actuator_type")]
    [MaxLength(50)]
    public string? ActuatorType { get; set; }

    [Column("action")]
    [MaxLength(10)]
    public string? Action { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    // For backward compatibility
    [NotMapped]
    public ActuatorType ActuatorTypeEnum => Enum.TryParse<ActuatorType>(ActuatorType, true, out var result) ? result : Models.ActuatorType.Pump;

    [NotMapped]
    public ControlType ControlType => ControlType.Automatic; // Default to automatic

    [NotMapped]
    public string? Reason => $"Actuator {ActuatorType} turned {Action}";

    // Foreign key to Device
    public Device Device { get; set; } = null!;
}