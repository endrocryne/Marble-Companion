using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class FeedEvent
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public FeedEventType EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<FeedReaction> Reactions { get; set; } = new List<FeedReaction>();
}
