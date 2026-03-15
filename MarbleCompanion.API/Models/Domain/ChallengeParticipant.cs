namespace MarbleCompanion.API.Models.Domain;

public class ChallengeParticipant
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public decimal CurrentProgress { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Challenge Challenge { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
