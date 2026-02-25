using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AeroponicIOT.Models;

[Table("automation_rules")]
public class AutomationRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int DeviceId { get; set; }

    [Column("rule_name")]
    [MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;

    [Column("rule_type")]
    // Type: 0 = Schedule, 1 = Threshold, 2 = Time-based
    public int RuleType { get; set; }

    [Column("actuator_type")]
    // 0 = Pump, 1 = Fan, 2 = Light, 3 = Heater
    public int ActuatorType { get; set; }

    [Column("action")]
    [MaxLength(10)]
    public string Action { get; set; } = "ON"; // ON, OFF, PULSE

    [Column("condition_parameter")]
    [MaxLength(50)]
    // pH, TDS, Temperature, Humidity
    public string? ConditionParameter { get; set; }

    [Column("condition_value")]
    public decimal? ConditionValue { get; set; }

    [Column("condition_operator")]
    [MaxLength(10)]
    // >, <, ==, >=, <=
    public string? ConditionOperator { get; set; }

    [Column("schedule_time")]
    public TimeOnly? ScheduleTime { get; set; }

    [Column("schedule_days")]
    [MaxLength(50)]
    // Comma-separated: Monday,Tuesday,Wednesday...
    public string? ScheduleDays { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("priority")]
    public int Priority { get; set; } = 1; // 1-10, higher = execute first

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_executed")]
    public DateTime? LastExecuted { get; set; }

    // Navigation properties
    [ForeignKey("DeviceId")]
    public Device? Device { get; set; }
}
