using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Services;

public class SyncService
{
    private readonly IApiService _api;
    private readonly LocalDatabaseService _db;
    private readonly AuthService _auth;
    private bool _isSyncing;

    public SyncService(IApiService api, LocalDatabaseService db, AuthService auth)
    {
        _api = api;
        _db = db;
        _auth = auth;
    }

    public event EventHandler? SyncCompleted;

    public async Task SyncAllAsync()
    {
        if (_isSyncing || !_auth.IsAuthenticated) return;
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

        _isSyncing = true;
        try
        {
            await SyncActionsAsync();
            await SyncHabitsAsync();
            SyncCompleted?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private async Task SyncActionsAsync()
    {
        var unsynced = _db.GetUnsyncedActions();
        foreach (var action in unsynced)
        {
            try
            {
                var request = new LogActionRequest
                {
                    Category = action.Category,
                    TemplateId = action.TemplateId ?? string.Empty,
                    DetailedData = action.DetailedData
                };
                var response = await _api.LogActionAsync(request);
                _db.MarkActionSynced(action.Id, response.ActionId);
            }
            catch (Exception)
            {
                // Will retry on next sync
            }
        }
    }

    private async Task SyncHabitsAsync()
    {
        var unsyncedHabits = _db.GetUnsyncedHabits();
        foreach (var habit in unsyncedHabits)
        {
            try
            {
                if (!string.IsNullOrEmpty(habit.ServerId))
                {
                    await _api.CheckinHabitAsync(habit.ServerId);
                    _db.MarkHabitSynced(habit.Id, habit.ServerId);
                }
            }
            catch (Exception)
            {
                // Will retry on next sync
            }
        }
    }

    public void StartBackgroundSync()
    {
        Connectivity.Current.ConnectivityChanged += async (_, _) =>
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                await SyncAllAsync();
        };
    }
}
