namespace MarbleCompanion.Mobile.Models;

public class UserSession
{
    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime TokenExpiry { get; set; }

    public bool IsSetupComplete { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken) && TokenExpiry > DateTime.UtcNow;

    public int CurrentStreak { get; set; }

    public int TotalLP { get; set; }
}
