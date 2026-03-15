using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class ContentLibraryViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;
    private List<ContentSummaryDto> _allContent = [];

    [ObservableProperty]
    private ObservableCollection<ContentSummaryDto> _articles = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ActionCategory? _filterTopic;

    [ObservableProperty]
    private ContentDifficulty? _filterDifficulty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ContentLibraryViewModel(IApiService apiService, NavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilters();
    partial void OnFilterTopicChanged(ActionCategory? value) => ApplyFilters();
    partial void OnFilterDifficultyChanged(ContentDifficulty? value) => ApplyFilters();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            _allContent = await _apiService.GetContentListAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load content: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BookmarkAsync(string contentId)
    {
        try
        {
            ErrorMessage = null;
            await _apiService.BookmarkContentAsync(contentId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to bookmark: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NavigateToDetailAsync(string contentId)
    {
        await _navigationService.GoToAsync("contentdetail",
            new Dictionary<string, object> { ["contentId"] = contentId });
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchQuery = string.Empty;
        FilterTopic = null;
        FilterDifficulty = null;
    }

    private void ApplyFilters()
    {
        var filtered = _allContent.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.Trim();
            filtered = filtered.Where(c =>
                c.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.Summary.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        if (FilterTopic.HasValue)
            filtered = filtered.Where(c => c.Category == FilterTopic.Value);

        if (FilterDifficulty.HasValue)
            filtered = filtered.Where(c => c.Difficulty == FilterDifficulty.Value);

        Articles = new ObservableCollection<ContentSummaryDto>(filtered);
    }
}
