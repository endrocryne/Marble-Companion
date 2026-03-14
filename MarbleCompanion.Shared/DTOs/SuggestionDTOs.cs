using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record SuggestionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("impact")]
    public SuggestionImpact Impact { get; init; }

    [JsonPropertyName("effort")]
    public SuggestionEffort Effort { get; init; }

    [JsonPropertyName("estimatedCO2eSaving")]
    public double EstimatedCO2eSaving { get; init; }
}

public record SuggestionFeedbackDto
{
    [JsonPropertyName("suggestionId")]
    public Guid SuggestionId { get; init; }

    [JsonPropertyName("isHelpful")]
    public bool IsHelpful { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
}
