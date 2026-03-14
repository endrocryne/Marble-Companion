using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record ContentDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("difficulty")]
    public ContentDifficulty Difficulty { get; init; }

    [JsonPropertyName("lpReward")]
    public int LPReward { get; init; }

    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; init; }
}

public record CreateContentDto
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("difficulty")]
    public ContentDifficulty Difficulty { get; init; }

    [JsonPropertyName("lpReward")]
    public int LPReward { get; init; }
}

public record ContentSummaryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("difficulty")]
    public ContentDifficulty Difficulty { get; init; }

    [JsonPropertyName("isRead")]
    public bool IsRead { get; init; }
}
