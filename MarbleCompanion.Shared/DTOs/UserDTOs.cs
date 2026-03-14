using System.Text.Json.Serialization;

namespace MarbleCompanion.Shared.DTOs;

public record UserProfileDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("avatarIndex")]
    public int AvatarIndex { get; init; }

    [JsonPropertyName("regionContinent")]
    public string? RegionContinent { get; init; }

    [JsonPropertyName("regionCountry")]
    public string? RegionCountry { get; init; }

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; init; }

    [JsonPropertyName("longestStreak")]
    public int LongestStreak { get; init; }

    [JsonPropertyName("totalLP")]
    public int TotalLP { get; init; }

    [JsonPropertyName("joinedAt")]
    public DateTime JoinedAt { get; init; }
}

public record UpdateUserDto
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("avatarIndex")]
    public int? AvatarIndex { get; init; }

    [JsonPropertyName("regionContinent")]
    public string? RegionContinent { get; init; }

    [JsonPropertyName("regionCountry")]
    public string? RegionCountry { get; init; }
}

public record UserSearchResultDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarIndex")]
    public int AvatarIndex { get; init; }

    [JsonPropertyName("totalLP")]
    public int TotalLP { get; init; }
}

public record UserSetupDto
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarIndex")]
    public int AvatarIndex { get; init; }

    [JsonPropertyName("regionContinent")]
    public string? RegionContinent { get; init; }

    [JsonPropertyName("regionCountry")]
    public string? RegionCountry { get; init; }

    [JsonPropertyName("baselineQuizAnswers")]
    public Dictionary<string, string> BaselineQuizAnswers { get; init; } = new();
}
