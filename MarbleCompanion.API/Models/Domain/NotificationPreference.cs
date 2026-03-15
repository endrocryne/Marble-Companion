namespace MarbleCompanion.API.Models.Domain;

public class NotificationPreference
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;

    public bool DailyReminder { get; set; } = true;
    public TimeOnly? DailyReminderTime { get; set; }
    public bool StreakAtRisk { get; set; } = true;
    public bool FriendMilestone { get; set; } = true;
    public bool ChallengeEnding { get; set; } = true;
    public bool NewChallenge { get; set; } = true;
    public bool AchievementUnlocked { get; set; } = true;
    public bool FriendRequest { get; set; } = true;

    public ApplicationUser User { get; set; } = null!;
}
