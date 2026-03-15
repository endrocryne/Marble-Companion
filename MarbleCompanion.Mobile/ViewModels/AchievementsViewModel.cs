using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class AchievementsViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<AchievementDisplayItem> _allAchievements = [];

    [ObservableProperty]
    private int _totalAchievements;

    [ObservableProperty]
    private int _unlockedCount;

    [ObservableProperty]
    private double _completionPercent;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public AchievementsViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var allTask = _apiService.GetAchievementsAsync();
            var unlockedTask = _apiService.GetUnlockedAchievementsAsync();

            await Task.WhenAll(allTask, unlockedTask);

            var all = allTask.Result;
            var unlocked = unlockedTask.Result;

            var unlockedIds = unlocked
                .Select(u => u.Achievement.Id)
                .ToHashSet();

            var unlockedLookup = unlocked
                .ToDictionary(u => u.Achievement.Id);

            var displayItems = all.Select(a =>
            {
                bool isUnlocked = unlockedIds.Contains(a.Id);
                DateTime? unlockedAt = isUnlocked
                    ? unlockedLookup[a.Id].UnlockedAt
                    : null;

                return new AchievementDisplayItem(
                    a.Id,
                    a.Name,
                    a.Description,
                    a.IconUrl,
                    a.LPReward,
                    a.Category,
                    isUnlocked,
                    unlockedAt);
            })
            .OrderByDescending(a => a.IsUnlocked)
            .ThenBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToList();

            AllAchievements = new ObservableCollection<AchievementDisplayItem>(displayItems);
            TotalAchievements = all.Count;
            UnlockedCount = unlocked.Count;
            CompletionPercent = all.Count > 0
                ? (double)unlocked.Count / all.Count * 100
                : 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load achievements: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public record AchievementDisplayItem(
    Guid Id,
    string Name,
    string Description,
    string? IconUrl,
    int LPReward,
    string Category,
    bool IsUnlocked,
    DateTime? UnlockedAt);
