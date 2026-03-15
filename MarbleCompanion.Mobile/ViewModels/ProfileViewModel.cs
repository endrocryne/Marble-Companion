using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class ProfileViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private int _avatarIndex;

    [ObservableProperty]
    private string? _regionContinent;

    [ObservableProperty]
    private string? _regionCountry;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _longestStreak;

    [ObservableProperty]
    private int _totalLP;

    [ObservableProperty]
    private DateTime _joinedAt;

    [ObservableProperty]
    private int _friendsCount;

    [ObservableProperty]
    private int _achievementsUnlocked;

    [ObservableProperty]
    private double _totalCO2eSaved;

    [ObservableProperty]
    private ObservableCollection<UserAchievementDto> _pinnedAchievements = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ProfileViewModel(IApiService apiService, NavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var profileTask = _apiService.GetMyProfileAsync();
            var friendsTask = _apiService.GetFriendsAsync();
            var achievementsTask = _apiService.GetUnlockedAchievementsAsync();
            var summaryTask = _apiService.GetActionSummaryAsync();

            await Task.WhenAll(profileTask, friendsTask, achievementsTask, summaryTask);

            var profile = profileTask.Result;
            DisplayName = profile.DisplayName;
            Email = profile.Email;
            AvatarIndex = profile.AvatarIndex;
            RegionContinent = profile.RegionContinent;
            RegionCountry = profile.RegionCountry;
            CurrentStreak = profile.CurrentStreak;
            LongestStreak = profile.LongestStreak;
            TotalLP = profile.TotalLP;
            JoinedAt = profile.JoinedAt;

            var friends = friendsTask.Result;
            FriendsCount = friends.Count;

            var achievements = achievementsTask.Result;
            AchievementsUnlocked = achievements.Count;

            // Show up to 3 most recent achievements as pinned
            var pinned = achievements
                .OrderByDescending(a => a.UnlockedAt)
                .Take(3)
                .ToList();
            PinnedAchievements = new ObservableCollection<UserAchievementDto>(pinned);

            var summary = summaryTask.Result;
            TotalCO2eSaved = summary.TotalCO2eSaved;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load profile: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await _navigationService.GoToAsync("settings");
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        try
        {
            ErrorMessage = null;
            var content = await _apiService.ExportDataAsync();
            var stream = await content.ReadAsStreamAsync();

            var filePath = Path.Combine(FileSystem.CacheDirectory, "marble-companion-export.json");
            using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export My Data",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export data: {ex.Message}";
        }
    }
}
