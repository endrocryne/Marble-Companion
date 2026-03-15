using System.Text.Json.Serialization;

namespace MarbleCompanion.Shared.DTOs;

public record NotificationPreferencesDto
{
    [JsonPropertyName("habitReminders")]
    public bool HabitReminders { get; init; } = true;

    [JsonPropertyName("challengeUpdates")]
    public bool ChallengeUpdates { get; init; } = true;

    [JsonPropertyName("friendActivity")]
    public bool FriendActivity { get; init; } = true;

    [JsonPropertyName("achievementAlerts")]
    public bool AchievementAlerts { get; init; } = true;

    [JsonPropertyName("weeklyReport")]
    public bool WeeklyReport { get; init; } = true;
}

public record RegisterDeviceTokenDto
{
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; init; } = string.Empty;
}
