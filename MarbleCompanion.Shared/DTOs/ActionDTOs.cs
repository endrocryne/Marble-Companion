using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record LogActionRequest
{
    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("actionTemplateId")]
    public Guid ActionTemplateId { get; init; }

    [JsonPropertyName("isDetailed")]
    public bool IsDetailed { get; init; }

    [JsonPropertyName("detailedData")]
    public Dictionary<string, string>? DetailedData { get; init; }
}

public record LogActionResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("lpAwarded")]
    public int LPAwarded { get; init; }

    [JsonPropertyName("co2eSaved")]
    public double CO2eSaved { get; init; }
}

public record ActionSummaryDto
{
    [JsonPropertyName("periodStart")]
    public DateTime PeriodStart { get; init; }

    [JsonPropertyName("periodEnd")]
    public DateTime PeriodEnd { get; init; }

    [JsonPropertyName("totalLP")]
    public int TotalLP { get; init; }

    [JsonPropertyName("totalCO2eSaved")]
    public double TotalCO2eSaved { get; init; }

    [JsonPropertyName("totalsByCategory")]
    public Dictionary<ActionCategory, CategoryTotalDto> TotalsByCategory { get; init; } = new();
}

public record CategoryTotalDto
{
    [JsonPropertyName("actionCount")]
    public int ActionCount { get; init; }

    [JsonPropertyName("lpEarned")]
    public int LPEarned { get; init; }

    [JsonPropertyName("co2eSaved")]
    public double CO2eSaved { get; init; }
}

public record CarbonActionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("templateName")]
    public string TemplateName { get; init; } = string.Empty;

    [JsonPropertyName("co2eSaved")]
    public double CO2eSaved { get; init; }

    [JsonPropertyName("lpAwarded")]
    public int LPAwarded { get; init; }

    [JsonPropertyName("loggedAt")]
    public DateTime LoggedAt { get; init; }

    [JsonPropertyName("isDetailed")]
    public bool IsDetailed { get; init; }
}
