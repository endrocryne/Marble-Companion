using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class FriendsViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<FriendDto> _friends = [];

    [ObservableProperty]
    private ObservableCollection<FriendRequestDto> _pendingRequests = [];

    [ObservableProperty]
    private ObservableCollection<UserSearchResultDto> _searchResults = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _hasPendingRequests;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public FriendsViewModel(IApiService apiService, NavigationService navigationService)
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

            var friends = await _apiService.GetFriendsAsync();
            Friends = new ObservableCollection<FriendDto>(friends);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load friends: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        try
        {
            IsSearching = true;
            ErrorMessage = null;

            var results = await _apiService.SearchUsersAsync(SearchQuery.Trim());
            SearchResults = new ObservableCollection<UserSearchResultDto>(results);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SendRequestAsync(string userId)
    {
        try
        {
            ErrorMessage = null;
            await _apiService.SendFriendRequestAsync(new { userId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to send request: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AcceptRequestAsync(string requestId)
    {
        try
        {
            ErrorMessage = null;
            await _apiService.AcceptFriendRequestAsync(requestId);

            // Remove from pending and refresh friends
            var pending = PendingRequests.FirstOrDefault(r => r.Id.ToString() == requestId);
            if (pending is not null)
                PendingRequests.Remove(pending);

            HasPendingRequests = PendingRequests.Count > 0;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to accept request: {ex.Message}";
        }
    }
}
