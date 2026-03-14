namespace MarbleCompanion.API.Models.Domain;

public class UserContentRead
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public Guid ContentId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    public bool LeafPointsAwarded { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Content Content { get; set; } = null!;
}
