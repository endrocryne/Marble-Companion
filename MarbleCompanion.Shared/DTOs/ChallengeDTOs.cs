using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record ChallengeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public ChallengeType Type { get; init; }

    [JsonPropertyName("difficulty")]
    public ChallengeDifficulty Difficulty { get; init; }

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("targetValue")]
    public int TargetValue { get; init; }

    [JsonPropertyName("lpReward")]
    public int LPReward { get; init; }

    [JsonPropertyName("startsAt")]
    public DateTime StartsAt { get; init; }

    [JsonPropertyName("endsAt")]
    public DateTime EndsAt { get; init; }

    [JsonPropertyName("participantCount")]
    public int ParticipantCount { get; init; }
}

public record CreateChallengeDto
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public ChallengeType Type { get; init; }

    [JsonPropertyName("difficulty")]
    public ChallengeDifficulty Difficulty { get; init; }

    [JsonPropertyName("category")]
    public ActionCategory Category { get; init; }

    [JsonPropertyName("targetValue")]
    public int TargetValue { get; init; }

    [JsonPropertyName("durationDays")]
    public int DurationDays { get; init; }
}

public record ChallengeProgressDto
{
    [JsonPropertyName("challengeId")]
    public Guid ChallengeId { get; init; }

    [JsonPropertyName("currentValue")]
    public int CurrentValue { get; init; }

    [JsonPropertyName("targetValue")]
    public int TargetValue { get; init; }

    [JsonPropertyName("percentComplete")]
    public double PercentComplete { get; init; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; init; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; init; }
}

public record ChallengeParticipantDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarIndex")]
    public int AvatarIndex { get; init; }

    [JsonPropertyName("currentValue")]
    public int CurrentValue { get; init; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; init; }

    [JsonPropertyName("rank")]
    public int Rank { get; init; }
}
