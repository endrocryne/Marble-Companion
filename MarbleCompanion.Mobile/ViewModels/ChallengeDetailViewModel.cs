using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
[QueryProperty(nameof(ChallengeId), "challengeId")]
public partial class ChallengeDetailViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string? _challengeId;

    [ObservableProperty]
    private ChallengeDto? _challenge;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ChallengeType _type;

    [ObservableProperty]
    private ChallengeDifficulty _difficulty;

    [ObservableProperty]
    private ActionCategory _category;

    [ObservableProperty]
    private int _targetValue;

    [ObservableProperty]
    private int _lpReward;

    [ObservableProperty]
    private DateTime _startsAt;

    [ObservableProperty]
    private DateTime _endsAt;

    [ObservableProperty]
    private int _participantCount;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private int _daysRemaining;

    [ObservableProperty]
    private ObservableCollection<ChallengeParticipantDto> _leaderboard = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ChallengeDetailViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(ChallengeId)) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var challenge = await _apiService.GetChallengeAsync(ChallengeId);
            Challenge = challenge;

            Title = challenge.Title;
            Description = challenge.Description;
            Type = challenge.Type;
            Difficulty = challenge.Difficulty;
            Category = challenge.Category;
            TargetValue = challenge.TargetValue;
            LpReward = challenge.LPReward;
            StartsAt = challenge.StartsAt;
            EndsAt = challenge.EndsAt;
            ParticipantCount = challenge.ParticipantCount;

            DaysRemaining = Math.Max(0, (int)(challenge.EndsAt - DateTime.UtcNow).TotalDays);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load challenge: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task JoinAsync()
    {
        if (string.IsNullOrEmpty(ChallengeId)) return;

        try
        {
            ErrorMessage = null;
            var participant = await _apiService.JoinChallengeAsync(ChallengeId);
            Leaderboard.Add(participant);
            ParticipantCount++;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to join: {ex.Message}";
        }
    }
}
