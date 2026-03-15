using System.Text.Json.Serialization;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.DTOs;

public record FriendDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarIndex")]
    public int AvatarIndex { get; init; }

    [JsonPropertyName("totalLP")]
    public int TotalLP { get; init; }

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; init; }

    [JsonPropertyName("friendsSince")]
    public DateTime FriendsSince { get; init; }
}

public record FriendRequestDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("fromUserId")]
    public Guid FromUserId { get; init; }

    [JsonPropertyName("fromDisplayName")]
    public string FromDisplayName { get; init; } = string.Empty;

    [JsonPropertyName("fromAvatarIndex")]
    public int FromAvatarIndex { get; init; }

    [JsonPropertyName("status")]
    public FriendRequestStatus Status { get; init; }

    [JsonPropertyName("sentAt")]
    public DateTime SentAt { get; init; }
}

public record FeedEventDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarIndex")]
    public int AvatarIndex { get; init; }

    [JsonPropertyName("eventType")]
    public FeedEventType EventType { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("occurredAt")]
    public DateTime OccurredAt { get; init; }

    [JsonPropertyName("reactions")]
    public Dictionary<ReactionType, int> Reactions { get; init; } = new();

    [JsonPropertyName("userReaction")]
    public ReactionType? UserReaction { get; init; }
}

public record FeedReactionDto
{
    [JsonPropertyName("feedEventId")]
    public Guid FeedEventId { get; init; }

    [JsonPropertyName("reactionType")]
    public ReactionType ReactionType { get; init; }
}
