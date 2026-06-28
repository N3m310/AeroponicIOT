using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AeroponicIOT.Models;

public enum AlertType
{
    Warning,
    Error,
    Info
}

public enum AlertStatus
{
    Active,
    Resolved
}

[Table("alerts")]
public class Alert
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("alert_type")]
    [MaxLength(50)]
    public string? AlertType { get; set; }

    [Column("message")]
    [MaxLength(500)]
    public string? Message { get; set; }

    [Column("severity")]
    [MaxLength(20)]
    public string? Severity { get; set; }

    [Column("is_resolved")]
    public bool IsResolved { get; set; }

    // For backward compatibility
    [NotMapped]
    public AlertType Type => Enum.TryParse<AlertType>(AlertType, true, out var result) ? result : Models.AlertType.Warning;

    [NotMapped]
    public string Title => AlertType ?? "Alert";

    [NotMapped]
    public AlertStatus Status => IsResolved ? AlertStatus.Resolved : AlertStatus.Active;

    [NotMapped]
    public DateTime? ResolvedAt => IsResolved ? Timestamp : null;

    // Foreign key to Device (optional, can be system-wide)
    public Device? Device { get; set; }
}