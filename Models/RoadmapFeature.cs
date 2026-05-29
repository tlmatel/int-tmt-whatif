using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreWhatIf.Models;

[Table("roadmap_features")]
public class RoadmapFeature
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("feature_name")]
    public string FeatureName { get; set; } = "";

    [Column("satellite_product")]
    public string? SatelliteProduct { get; set; }

    [Column("planned_version")]
    public string? PlannedVersion { get; set; }

    [Column("planned_date")]
    public DateOnly? PlannedDate { get; set; }

    [Column("is_ready")]
    public bool IsReady { get; set; }

    [Column("effort_weeks")]
    public int EffortWeeks { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}
