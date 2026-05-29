using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreWhatIf.Models;

public class SavedSimulation
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string UserEmail { get; set; } = "";

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Column(TypeName = "jsonb")]
    public string StateJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
