using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record OffsetCreditDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("tier")]
    public OffsetTier Tier { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("co2eOffsetKg")]
    public double CO2eOffsetKg { get; init; }

    [JsonPropertyName("lpCost")]
    public int LPCost { get; init; }
}

public record RedeemOffsetRequest
{
    [JsonPropertyName("offsetCreditId")]
    public Guid OffsetCreditId { get; init; }
}

public record OffsetHistoryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("offsetCreditName")]
    public string OffsetCreditName { get; init; } = string.Empty;

    [JsonPropertyName("tier")]
    public OffsetTier Tier { get; init; }

    [JsonPropertyName("co2eOffsetKg")]
    public double CO2eOffsetKg { get; init; }

    [JsonPropertyName("lpSpent")]
    public int LPSpent { get; init; }

    [JsonPropertyName("redeemedAt")]
    public DateTime RedeemedAt { get; init; }
}
