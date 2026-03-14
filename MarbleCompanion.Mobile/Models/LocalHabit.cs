using LiteDB;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Models;

public class LocalHabit
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public string? ServerId { get; set; }

    public string LibraryItemId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ActionCategory Category { get; set; }

    public HabitFrequency Frequency { get; set; }

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }

    public DateTime? LastCheckinDate { get; set; }

    public List<DateTime> CheckinDates { get; set; } = new();

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Synced { get; set; }
}
