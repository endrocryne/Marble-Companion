using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class Habit
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public ActionCategory Category { get; set; }
    public HabitFrequency Frequency { get; set; }
    public int LeafPointsReward { get; set; }
    public decimal EstimatedCO2eImpact { get; set; }
    public bool IsCustom { get; set; }
    public bool IsActive { get; set; } = true;
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LibraryItemId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<HabitCheckin> Checkins { get; set; } = new List<HabitCheckin>();
}
