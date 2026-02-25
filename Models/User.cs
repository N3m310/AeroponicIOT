using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HydroponicIOT.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    [MaxLength(100)]
    public string? Username { get; set; }

    [Column("email")]
    [MaxLength(256)]
    public string? Email { get; set; }

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("role")]
    [MaxLength(50)]
    public string? Role { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    // For backward compatibility
    [NotMapped]
    public bool IsActive => true; // Default to active

    // Navigation properties
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}