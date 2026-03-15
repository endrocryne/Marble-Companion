using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class HabitsViewModel
{
    private readonly IApiService _apiService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ActiveHabitDto> _activeHabits = [];

    [ObservableProperty]
    private bool _hasHabits;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public HabitsViewModel(IApiService apiService, NavigationService navigationService)
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

            var habits = await _apiService.GetActiveHabitsAsync();
            ActiveHabits = new ObservableCollection<ActiveHabitDto>(habits);
            HasHabits = habits.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load habits: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CheckinAsync(string habitId)
    {
        try
        {
            ErrorMessage = null;
            var result = await _apiService.CheckinHabitAsync(habitId);

            // Refresh the list to show updated streak
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Check-in failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NavigateToLibraryAsync()
    {
        await _navigationService.GoToAsync("habitlibrary");
    }

    [RelayCommand]
    private async Task NavigateToDetailAsync(string habitId)
    {
        await _navigationService.GoToAsync("habitdetail",
            new Dictionary<string, object> { ["habitId"] = habitId });
    }
}
