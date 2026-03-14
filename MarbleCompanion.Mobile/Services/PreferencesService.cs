namespace MarbleCompanion.Mobile.Services;

public class PreferencesService
{
    private const string ThemeKey = "theme";
    private const string UnitsKey = "units";
    private const string NotificationsEnabledKey = "notifications_enabled";
    private const string HabitReminderTimeKey = "habit_reminder_time";
    private const string WeeklyReportKey = "weekly_report";
    private const string RegionKey = "region";

    public AppTheme Theme
    {
        get => Enum.TryParse<AppTheme>(Preferences.Get(ThemeKey, "System"), out var t) ? t : AppTheme.Unspecified;
        set => Preferences.Set(ThemeKey, value.ToString());
    }

    public string Units
    {
        get => Preferences.Get(UnitsKey, "metric");
        set => Preferences.Set(UnitsKey, value);
    }

    public bool NotificationsEnabled
    {
        get => Preferences.Get(NotificationsEnabledKey, true);
        set => Preferences.Set(NotificationsEnabledKey, value);
    }

    public TimeSpan HabitReminderTime
    {
        get => TimeSpan.TryParse(Preferences.Get(HabitReminderTimeKey, "09:00"), out var ts) ? ts : new TimeSpan(9, 0, 0);
        set => Preferences.Set(HabitReminderTimeKey, value.ToString());
    }

    public bool WeeklyReportEnabled
    {
        get => Preferences.Get(WeeklyReportKey, true);
        set => Preferences.Set(WeeklyReportKey, value);
    }

    public string Region
    {
        get => Preferences.Get(RegionKey, "Global");
        set => Preferences.Set(RegionKey, value);
    }

    public void ClearAll() => Preferences.Clear();
}
