using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class FeedReaction
{
    public Guid Id { get; set; }
    public Guid FeedEventId { get; set; }
    public string UserId { get; set; } = null!;
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FeedEvent FeedEvent { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
