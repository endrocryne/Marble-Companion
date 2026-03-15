using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class ChallengesViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ChallengeDto> _curatedChallenges = [];

    [ObservableProperty]
    private ObservableCollection<ChallengeDto> _myChallenges = [];

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    // Create challenge form fields
    [ObservableProperty]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private ChallengeType _newType;

    [ObservableProperty]
    private ChallengeDifficulty _newDifficulty;

    [ObservableProperty]
    private ActionCategory _newCategory;

    [ObservableProperty]
    private int _newTargetValue;

    [ObservableProperty]
    private int _newDurationDays = 7;

    public ChallengesViewModel(IApiService apiService, NavigationService navigationService)
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

            var curatedTask = _apiService.GetCuratedChallengesAsync();
            var mineTask = _apiService.GetMyChallengesAsync();

            await Task.WhenAll(curatedTask, mineTask);

            CuratedChallenges = new ObservableCollection<ChallengeDto>(curatedTask.Result);
            MyChallenges = new ObservableCollection<ChallengeDto>(mineTask.Result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load challenges: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task JoinAsync(string challengeId)
    {
        try
        {
            ErrorMessage = null;
            await _apiService.JoinChallengeAsync(challengeId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to join challenge: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            ErrorMessage = "Please enter a challenge title.";
            return;
        }

        try
        {
            ErrorMessage = null;
            var dto = new CreateChallengeDto
            {
                Title = NewTitle.Trim(),
                Description = NewDescription.Trim(),
                Type = NewType,
                Difficulty = NewDifficulty,
                Category = NewCategory,
                TargetValue = NewTargetValue,
                DurationDays = NewDurationDays
            };

            await _apiService.CreateChallengeAsync(dto);

            // Reset form
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            NewTargetValue = 0;
            NewDurationDays = 7;

            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create challenge: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NavigateToDetailAsync(string challengeId)
    {
        await _navigationService.GoToAsync("challengedetail",
            new Dictionary<string, object> { ["challengeId"] = challengeId });
    }
}
