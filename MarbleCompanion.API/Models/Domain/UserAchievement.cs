namespace MarbleCompanion.API.Models.Domain;

public class UserAchievement
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public int AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
