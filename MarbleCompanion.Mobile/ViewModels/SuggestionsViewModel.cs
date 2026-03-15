using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.ML;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class SuggestionsViewModel
{
    private readonly IApiService _apiService;
    private readonly RecommendationEngine _recommendationEngine;
    private readonly Dictionary<string, string> _feedback = new();

    [ObservableProperty]
    private ObservableCollection<RankedSuggestion> _suggestions = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public SuggestionsViewModel(IApiService apiService, RecommendationEngine recommendationEngine)
    {
        _apiService = apiService;
        _recommendationEngine = recommendationEngine;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var profile = await BuildUserProfileAsync();
            var ranked = _recommendationEngine.GetRecommendations(profile, 3);
            Suggestions = new ObservableCollection<RankedSuggestion>(ranked);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load suggestions: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task MarkDoingAsync(string suggestionId)
    {
        _feedback[suggestionId] = "doing";
        await LoadAsync();
    }

    [RelayCommand]
    private async Task MarkNotRelevantAsync(string suggestionId)
    {
        _feedback[suggestionId] = "not_relevant";
        await LoadAsync();
    }

    [RelayCommand]
    private async Task MarkAlreadyDoingAsync(string suggestionId)
    {
        _feedback[suggestionId] = "already_doing";
        await LoadAsync();
    }

    private async Task<UserBehaviorProfile> BuildUserProfileAsync()
    {
        var profileTask = _apiService.GetMyProfileAsync();
        var summaryTask = _apiService.GetActionSummaryAsync();
        var habitsTask = _apiService.GetActiveHabitsAsync();

        await Task.WhenAll(profileTask, summaryTask, habitsTask);

        var profile = profileTask.Result;
        var summary = summaryTask.Result;
        var habits = habitsTask.Result;

        var actionCounts = summary.TotalsByCategory
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ActionCount);

        var co2eByCategory = summary.TotalsByCategory
            .ToDictionary(kvp => kvp.Key, kvp => (decimal)kvp.Value.CO2eSaved);

        var activeHabitCategories = habits
            .Select(h => h.Category.ToString())
            .Distinct()
            .ToList();

        // Estimate baseline from quiz data (~4,600 kg average per year)
        decimal baseline = 4600m;

        return new UserBehaviorProfile(
            actionCounts,
            co2eByCategory,
            baseline,
            profile.CurrentStreak,
            activeHabitCategories,
            _feedback,
            profile.RegionContinent ?? string.Empty);
    }
}
