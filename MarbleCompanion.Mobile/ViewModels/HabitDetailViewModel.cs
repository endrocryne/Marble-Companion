using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
[QueryProperty(nameof(HabitId), "habitId")]
public partial class HabitDetailViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string? _habitId;

    [ObservableProperty]
    private string _habitName = string.Empty;

    [ObservableProperty]
    private ActionCategory _category;

    [ObservableProperty]
    private HabitFrequency _frequency;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _longestStreak;

    [ObservableProperty]
    private DateTime? _lastCheckinAt;

    [ObservableProperty]
    private bool _isCheckedInToday;

    [ObservableProperty]
    private ObservableCollection<CalendarHeatmapEntry> _heatmapData = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public HabitDetailViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(HabitId)) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var habits = await _apiService.GetActiveHabitsAsync();
            var habit = habits.FirstOrDefault(h => h.Id.ToString() == HabitId);

            if (habit is null)
            {
                ErrorMessage = "Habit not found.";
                return;
            }

            HabitName = habit.Name;
            Category = habit.Category;
            Frequency = habit.Frequency;
            CurrentStreak = habit.CurrentStreak;
            LongestStreak = habit.LongestStreak;
            LastCheckinAt = habit.LastCheckinAt;
            IsCheckedInToday = habit.IsCheckedInToday;

            BuildHeatmapData(habit);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load habit: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CheckinAsync()
    {
        if (string.IsNullOrEmpty(HabitId) || IsCheckedInToday) return;

        try
        {
            ErrorMessage = null;
            var result = await _apiService.CheckinHabitAsync(HabitId);

            CurrentStreak = result.NewStreak;
            IsCheckedInToday = true;
            LastCheckinAt = result.CheckedInAt;

            // Refresh heatmap
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Check-in failed: {ex.Message}";
        }
    }

    private void BuildHeatmapData(ActiveHabitDto habit)
    {
        // Generate last 90 days of heatmap data based on streak info
        var entries = new List<CalendarHeatmapEntry>();
        var today = DateTime.UtcNow.Date;

        for (int i = 89; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            bool completed = false;

            // If the date falls within the current streak window, mark as completed
            if (habit.LastCheckinAt.HasValue && i < habit.CurrentStreak)
                completed = true;

            entries.Add(new CalendarHeatmapEntry(date, completed));
        }

        HeatmapData = new ObservableCollection<CalendarHeatmapEntry>(entries);
    }
}

public record CalendarHeatmapEntry(DateTime Date, bool Completed);
