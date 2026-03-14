using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class CarbonAction
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public ActionCategory Category { get; set; }
    public string ActionTemplateId { get; set; } = string.Empty;
    public bool IsDetailed { get; set; }
    public decimal CO2eSavedKg { get; set; }
    public int LeafPointsAwarded { get; set; }
    public string? DetailedDataJson { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public int? EmissionFactorId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public EmissionFactor? EmissionFactor { get; set; }
}
