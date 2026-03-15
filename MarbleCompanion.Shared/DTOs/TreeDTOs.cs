using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record TreeDto
{
    [JsonPropertyName("species")]
    public TreeSpecies Species { get; init; }

    [JsonPropertyName("stage")]
    public int Stage { get; init; }

    [JsonPropertyName("stageName")]
    public string StageName { get; init; } = string.Empty;

    [JsonPropertyName("healthScore")]
    public int HealthScore { get; init; }

    [JsonPropertyName("healthState")]
    public TreeHealthState HealthState { get; init; }

    [JsonPropertyName("totalLP")]
    public int TotalLP { get; init; }

    [JsonPropertyName("totalCO2eAvoided")]
    public double TotalCO2eAvoided { get; init; }

    [JsonPropertyName("nextMilestoneLP")]
    public int NextMilestoneLP { get; init; }

    [JsonPropertyName("activeCosmetics")]
    public List<string> ActiveCosmetics { get; init; } = [];
}
