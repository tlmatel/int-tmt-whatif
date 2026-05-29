namespace CoreWhatIf.Models;

public class HubRdAccessResponse
{
    public string App { get; set; } = "";
    public string Email { get; set; } = "";
    public bool HasAccess { get; set; }
    public string Reason { get; set; } = "";
    public bool IsAdmin { get; set; }
    public string? Role { get; set; }

    /// <summary>
    /// Returns the effective role as-is from HubRD (e.g. "admin", "gestor", "user").
    /// Falls back to IsAdmin for backward compatibility.
    /// </summary>
    public string GetEffectiveRole()
    {
        if (!string.IsNullOrWhiteSpace(Role))
            return Role.Trim().ToLowerInvariant();

        return IsAdmin ? "admin" : "user";
    }
}
