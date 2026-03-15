using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Models;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class HomeViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private TreeRenderState _treeRenderState = new();

    [ObservableProperty]
    private ContentSummaryDto? _todaysFact;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _totalLP;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public HomeViewModel(IApiService apiService, NavigationService navigationService)
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

            var treeTask = _apiService.GetTreeAsync();
            var factTask = _apiService.GetTodayContentAsync();
            var profileTask = _apiService.GetMyProfileAsync();

            await Task.WhenAll(treeTask, factTask, profileTask);

            var tree = treeTask.Result;
            TreeRenderState = MapToRenderState(tree);

            TodaysFact = factTask.Result;

            var profile = profileTask.Result;
            CurrentStreak = profile.CurrentStreak;
            TotalLP = profile.TotalLP;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load home data: {ex.Message}";
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
    private async Task ShowQuickLogAsync()
    {
        await _navigationService.GoToAsync("quicklog");
    }

    private static TreeRenderState MapToRenderState(TreeDto tree)
    {
        var now = DateTime.Now;
        var season = now.Month switch
        {
            >= 3 and <= 5 => Season.Spring,
            >= 6 and <= 8 => Season.Summer,
            >= 9 and <= 11 => Season.Autumn,
            _ => Season.Winter
        };

        return new TreeRenderState
        {
            Species = tree.Species,
            Stage = tree.Stage,
            StageName = tree.StageName,
            HealthScore = tree.HealthScore,
            HealthState = tree.HealthState,
            TotalLP = tree.TotalLP,
            TotalCo2EAvoided = tree.TotalCO2eAvoided,
            NextMilestoneLP = tree.NextMilestoneLP,
            CurrentSeason = season,
            ActiveCosmetics = tree.ActiveCosmetics.ToList()
        };
    }
}
