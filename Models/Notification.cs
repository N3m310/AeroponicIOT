using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AeroponicIOT.Models;

[Table("notifications")]
public class Notification
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string? Title { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("type")]
    public int Type { get; set; } = 0; // NotificationType

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
}
