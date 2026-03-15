using System.Text.Json.Serialization;

namespace MarbleCompanion.Shared.DTOs;

public record AchievementDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; init; }

    [JsonPropertyName("lpReward")]
    public int LPReward { get; init; }

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;
}

public record UserAchievementDto
{
    [JsonPropertyName("achievement")]
    public AchievementDto Achievement { get; init; } = null!;

    [JsonPropertyName("unlockedAt")]
    public DateTime UnlockedAt { get; init; }

    [JsonPropertyName("isNew")]
    public bool IsNew { get; init; }
}
