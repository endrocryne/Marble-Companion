using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class HabitLibraryViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;
    private List<HabitLibraryItemDto> _allItems = [];

    [ObservableProperty]
    private ObservableCollection<HabitLibraryItemDto> _libraryItems = [];

    [ObservableProperty]
    private ActionCategory? _filterCategory;

    [ObservableProperty]
    private HabitFrequency? _filterFrequency;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public HabitLibraryViewModel(IApiService apiService, NavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    partial void OnFilterCategoryChanged(ActionCategory? value) => ApplyFilters();
    partial void OnFilterFrequencyChanged(HabitFrequency? value) => ApplyFilters();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            _allItems = await _apiService.GetHabitLibraryAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load library: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddHabitAsync(Guid habitLibraryItemId)
    {
        try
        {
            ErrorMessage = null;
            var dto = new CreateHabitDto { HabitLibraryItemId = habitLibraryItemId };
            await _apiService.CreateHabitAsync(dto);
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to add habit: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        FilterCategory = null;
        FilterFrequency = null;
    }

    private void ApplyFilters()
    {
        var filtered = _allItems.AsEnumerable();

        if (FilterCategory.HasValue)
            filtered = filtered.Where(i => i.Category == FilterCategory.Value);

        if (FilterFrequency.HasValue)
            filtered = filtered.Where(i => i.Frequency == FilterFrequency.Value);

        LibraryItems = new ObservableCollection<HabitLibraryItemDto>(filtered);
    }
}
