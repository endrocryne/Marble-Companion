using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record HabitLibraryItemDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("frequency")]
    public HabitFrequency Frequency { get; init; }

    [JsonPropertyName("estimatedCO2ePerAction")]
    public double EstimatedCO2ePerAction { get; init; }
}

public record ActiveHabitDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("habitLibraryItemId")]
    public Guid HabitLibraryItemId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("frequency")]
    public HabitFrequency Frequency { get; init; }

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; init; }

    [JsonPropertyName("longestStreak")]
    public int LongestStreak { get; init; }

    [JsonPropertyName("lastCheckinAt")]
    public DateTime? LastCheckinAt { get; init; }

    [JsonPropertyName("isCheckedInToday")]
    public bool IsCheckedInToday { get; init; }
}

public record CreateHabitDto
{
    [JsonPropertyName("habitLibraryItemId")]
    public Guid HabitLibraryItemId { get; init; }
}

public record HabitCheckinDto
{
    [JsonPropertyName("habitId")]
    public Guid HabitId { get; init; }

    [JsonPropertyName("checkedInAt")]
    public DateTime CheckedInAt { get; init; }

    [JsonPropertyName("lpAwarded")]
    public int LPAwarded { get; init; }

    [JsonPropertyName("co2eSaved")]
    public double CO2eSaved { get; init; }

    [JsonPropertyName("newStreak")]
    public int NewStreak { get; init; }
}

public record HabitHistoryDto
{
    [JsonPropertyName("habitId")]
    public Guid HabitId { get; init; }

    [JsonPropertyName("habitName")]
    public string HabitName { get; init; } = string.Empty;

    [JsonPropertyName("checkins")]
    public List<HabitCheckinEntryDto> Checkins { get; init; } = [];
}

public record HabitCheckinEntryDto
{
    [JsonPropertyName("date")]
    public DateTime Date { get; init; }

    [JsonPropertyName("completed")]
    public bool Completed { get; init; }
}
