using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class FeedViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<FeedEventDto> _feedEvents = [];

    [ObservableProperty]
    private bool _hasEvents;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public FeedViewModel(IApiService apiService)
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

            var events = await _apiService.GetFeedAsync();
            FeedEvents = new ObservableCollection<FeedEventDto>(events);
            HasEvents = events.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load feed: {ex.Message}";
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
    private async Task ReactAsync(FeedReactionParameter parameter)
    {
        try
        {
            ErrorMessage = null;
            var reaction = new FeedReactionDto
            {
                FeedEventId = Guid.Parse(parameter.EventId),
                ReactionType = parameter.ReactionType
            };

            await _apiService.ReactToFeedEventAsync(parameter.EventId, reaction);

            // Refresh to show updated reactions
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to react: {ex.Message}";
        }
    }
}

public record FeedReactionParameter(string EventId, ReactionType ReactionType);
