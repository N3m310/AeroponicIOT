using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HydroponicIOT.Models;

[Table("crops")]
public class Crop
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("total_days_est")]
    public int? TotalDaysEst { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    // Navigation properties
    public ICollection<CropStage> CropStages { get; set; } = new List<CropStage>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}