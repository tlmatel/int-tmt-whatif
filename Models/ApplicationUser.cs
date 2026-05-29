namespace CoreWhatIf.Models;

public class ApplicationUser
{
    public string Id { get; set; } = "";
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
