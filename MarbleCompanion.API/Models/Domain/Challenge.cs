using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class Challenge
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChallengeType Type { get; set; }
    public ChallengeDifficulty Difficulty { get; set; }
    public ActionCategory? Category { get; set; }
    public string MetricKey { get; set; } = string.Empty;
    public decimal GoalValue { get; set; }
    public int DurationDays { get; set; }
    public int LeafPointsReward { get; set; }
    public bool IsCurated { get; set; }
    public string? CreatorUserId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string? SeasonTag { get; set; }

    public ApplicationUser? Creator { get; set; }
    public ICollection<ChallengeParticipant> Participants { get; set; } = new List<ChallengeParticipant>();
}
