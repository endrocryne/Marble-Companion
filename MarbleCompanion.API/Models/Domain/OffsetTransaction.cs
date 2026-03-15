using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class OffsetTransaction
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public OffsetTier Tier { get; set; }
    public int CreditsSpent { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
