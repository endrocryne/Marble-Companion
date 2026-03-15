using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
[QueryProperty(nameof(ContentId), "contentId")]
public partial class ContentDetailViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string? _contentId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private string _summary = string.Empty;

    [ObservableProperty]
    private string? _imageUrl;

    [ObservableProperty]
    private ActionCategory _category;

    [ObservableProperty]
    private ContentDifficulty _difficulty;

    [ObservableProperty]
    private int _lpReward;

    [ObservableProperty]
    private DateTime _publishedAt;

    [ObservableProperty]
    private int _estimatedReadingTimeMinutes;

    [ObservableProperty]
    private bool _isBookmarked;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ContentDetailViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(ContentId)) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var content = await _apiService.GetContentAsync(ContentId);

            Title = content.Title;
            Body = content.Body;
            Summary = content.Summary;
            ImageUrl = content.ImageUrl;
            Category = content.Category;
            Difficulty = content.Difficulty;
            LpReward = content.LPReward;
            PublishedAt = content.PublishedAt;

            // Estimate reading time (~200 words per minute)
            var wordCount = content.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            EstimatedReadingTimeMinutes = Math.Max(1, wordCount / 200);
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
    private async Task ToggleBookmarkAsync()
    {
        if (string.IsNullOrEmpty(ContentId)) return;

        try
        {
            ErrorMessage = null;
            await _apiService.BookmarkContentAsync(ContentId);
            IsBookmarked = !IsBookmarked;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to toggle bookmark: {ex.Message}";
        }
    }
}
