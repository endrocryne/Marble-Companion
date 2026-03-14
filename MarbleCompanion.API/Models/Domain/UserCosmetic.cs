namespace MarbleCompanion.API.Models.Domain;

public class UserCosmetic
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public int CosmeticId { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public TreeCosmetic Cosmetic { get; set; } = null!;
}
