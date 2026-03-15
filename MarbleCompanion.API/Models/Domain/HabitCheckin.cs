namespace MarbleCompanion.API.Models.Domain;

public class HabitCheckin
{
    public Guid Id { get; set; }
    public Guid HabitId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;

    public Habit Habit { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
