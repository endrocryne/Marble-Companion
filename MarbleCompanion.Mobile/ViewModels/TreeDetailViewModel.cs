using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
[QueryProperty(nameof(FriendId), "friendId")]
public partial class TreeDetailViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string? _friendId;

    [ObservableProperty]
    private TreeDto? _tree;

    [ObservableProperty]
    private TreeSpecies _species;

    [ObservableProperty]
    private int _stage;

    [ObservableProperty]
    private string _stageName = string.Empty;

    [ObservableProperty]
    private int _healthScore;

    [ObservableProperty]
    private TreeHealthState _healthState;

    [ObservableProperty]
    private int _totalLP;

    [ObservableProperty]
    private double _totalCO2eAvoided;

    [ObservableProperty]
    private int _nextMilestoneLP;

    [ObservableProperty]
    private double _progressToNextMilestone;

    [ObservableProperty]
    private List<string> _activeCosmetics = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public TreeDetailViewModel(IApiService apiService)
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

            TreeDto tree;
            if (!string.IsNullOrEmpty(FriendId))
                tree = await _apiService.GetFriendTreeAsync(FriendId);
            else
                tree = await _apiService.GetTreeAsync();

            Tree = tree;
            Species = tree.Species;
            Stage = tree.Stage;
            StageName = tree.StageName;
            HealthScore = tree.HealthScore;
            HealthState = tree.HealthState;
            TotalLP = tree.TotalLP;
            TotalCO2eAvoided = tree.TotalCO2eAvoided;
            NextMilestoneLP = tree.NextMilestoneLP;
            ActiveCosmetics = tree.ActiveCosmetics.ToList();

            ProgressToNextMilestone = tree.NextMilestoneLP > 0
                ? (double)tree.TotalLP / tree.NextMilestoneLP
                : 1.0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load tree: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
