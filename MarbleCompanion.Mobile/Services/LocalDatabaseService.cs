using LiteDB;
using MarbleCompanion.Mobile.Models;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Services;

public class LocalDatabaseService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<LocalAction> _actions;
    private readonly ILiteCollection<LocalHabit> _habits;

    public LocalDatabaseService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "marble_companion.db");
        _db = new LiteDatabase($"Filename={dbPath};Connection=shared");
        _actions = _db.GetCollection<LocalAction>("actions");
        _habits = _db.GetCollection<LocalHabit>("habits");

        _actions.EnsureIndex(x => x.LoggedAt);
        _actions.EnsureIndex(x => x.Synced);
        _habits.EnsureIndex(x => x.IsActive);
    }

    // Actions
    public void InsertAction(LocalAction action) => _actions.Insert(action);

    public List<LocalAction> GetActions(DateTime from, DateTime to) =>
        _actions.Find(a => a.LoggedAt >= from && a.LoggedAt <= to).OrderByDescending(a => a.LoggedAt).ToList();

    public List<LocalAction> GetRecentActions(int days = 30) =>
        GetActions(DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);

    public List<LocalAction> GetUnsyncedActions() =>
        _actions.Find(a => !a.Synced).ToList();

    public void MarkActionSynced(ObjectId id, string serverId)
    {
        var action = _actions.FindById(id);
        if (action != null)
        {
            action.Synced = true;
            action.ServerId = serverId;
            _actions.Update(action);
        }
    }

    public bool DeleteAction(ObjectId id) => _actions.Delete(id);

    public List<LocalAction> GetActionsByCategory(ActionCategory category, int days = 30) =>
        _actions.Find(a => a.Category == category && a.LoggedAt >= DateTime.UtcNow.AddDays(-days)).ToList();

    public double GetTotalCo2ESaved(int days = 30) =>
        GetRecentActions(days).Sum(a => a.Co2ESaved);

    public int GetTotalLP(int days = 30) =>
        GetRecentActions(days).Sum(a => a.LpAwarded);

    // Habits
    public void InsertHabit(LocalHabit habit) => _habits.Insert(habit);

    public List<LocalHabit> GetActiveHabits() =>
        _habits.Find(h => h.IsActive).ToList();

    public LocalHabit? GetHabit(ObjectId id) => _habits.FindById(id);

    public void UpdateHabit(LocalHabit habit) => _habits.Update(habit);

    public void DeleteHabit(ObjectId id) => _habits.Delete(id);

    public void CheckinHabit(ObjectId id)
    {
        var habit = _habits.FindById(id);
        if (habit == null) return;

        var today = DateTime.UtcNow.Date;
        if (habit.LastCheckinDate?.Date == today) return;

        habit.CheckinDates.Add(today);
        habit.LastCheckinDate = today;

        var yesterday = today.AddDays(-1);
        if (habit.LastCheckinDate?.Date == yesterday || habit.CurrentStreak == 0)
            habit.CurrentStreak++;
        else
            habit.CurrentStreak = 1;

        if (habit.CurrentStreak > habit.LongestStreak)
            habit.LongestStreak = habit.CurrentStreak;

        habit.Synced = false;
        _habits.Update(habit);
    }

    public List<LocalHabit> GetUnsyncedHabits() =>
        _habits.Find(h => !h.Synced).ToList();

    public void MarkHabitSynced(ObjectId id, string serverId)
    {
        var habit = _habits.FindById(id);
        if (habit != null)
        {
            habit.Synced = true;
            habit.ServerId = serverId;
            _habits.Update(habit);
        }
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}
