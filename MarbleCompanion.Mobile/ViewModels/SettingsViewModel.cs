using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class SettingsViewModel
{
    private readonly IApiService _apiService;
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    // Notification preferences
    [ObservableProperty]
    private bool _habitReminders = true;

    [ObservableProperty]
    private bool _challengeUpdates = true;

    [ObservableProperty]
    private bool _friendActivity = true;

    [ObservableProperty]
    private bool _achievementAlerts = true;

    [ObservableProperty]
    private bool _weeklyReport = true;

    // Units
    [ObservableProperty]
    private bool _useMetric = true;

    // Theme
    [ObservableProperty]
    private string _theme = "System";

    // Privacy
    [ObservableProperty]
    private bool _profilePublic = true;

    [ObservableProperty]
    private bool _showInLeaderboards = true;

    [ObservableProperty]
    private bool _shareFeedActivity = true;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public List<string> ThemeOptions { get; } = ["Light", "Dark", "System"];

    public SettingsViewModel(IApiService apiService, AuthService authService, NavigationService navigationService)
    {
        _apiService = apiService;
        _authService = authService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            // Restore persisted preferences
            UseMetric = Preferences.Get("units_metric", true);
            Theme = Preferences.Get("theme", "System");
            ProfilePublic = Preferences.Get("privacy_profile_public", true);
            ShowInLeaderboards = Preferences.Get("privacy_show_leaderboards", true);
            ShareFeedActivity = Preferences.Get("privacy_share_feed", true);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load settings: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            SuccessMessage = null;

            // Save notification preferences to API
            var prefs = new NotificationPreferencesDto
            {
                HabitReminders = HabitReminders,
                ChallengeUpdates = ChallengeUpdates,
                FriendActivity = FriendActivity,
                AchievementAlerts = AchievementAlerts,
                WeeklyReport = WeeklyReport
            };
            await _apiService.UpdateNotificationPreferencesAsync(prefs);

            // Save local preferences
            Preferences.Set("units_metric", UseMetric);
            Preferences.Set("theme", Theme);
            Preferences.Set("privacy_profile_public", ProfilePublic);
            Preferences.Set("privacy_show_leaderboards", ShowInLeaderboards);
            Preferences.Set("privacy_share_feed", ShareFeedActivity);

            // Apply theme
            ApplyTheme();

            SuccessMessage = "Settings saved successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save settings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _authService.SignOutAsync();
            await _navigationService.GoToAsync("//onboarding");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Logout failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _apiService.DeleteAccountAsync();
            await _authService.SignOutAsync();
            await _navigationService.GoToAsync("//onboarding");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete account: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyTheme()
    {
        if (Application.Current is null) return;

        Application.Current.UserAppTheme = Theme switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
