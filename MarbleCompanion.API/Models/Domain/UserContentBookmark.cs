namespace MarbleCompanion.API.Models.Domain;

public class UserContentBookmark
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public Guid ContentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Content Content { get; set; } = null!;
}
