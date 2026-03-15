using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Controls;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class DashboardViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private double _dailyCO2e;

    [ObservableProperty]
    private double _weeklyCO2e;

    [ObservableProperty]
    private double _monthlyCO2e;

    [ObservableProperty]
    private double _yearlyCO2e;

    [ObservableProperty]
    private int _totalLP;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private double _averageUserCO2e;

    [ObservableProperty]
    private double _userCO2e;

    [ObservableProperty]
    private ObservableCollection<ChartDataPoint> _lineChartData = [];

    [ObservableProperty]
    private ObservableCollection<CategorySegment> _donutSegments = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public DashboardViewModel(IApiService apiService)
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

            var summaryTask = _apiService.GetActionSummaryAsync();
            var profileTask = _apiService.GetMyProfileAsync();
            var actionsTask = _apiService.GetActionsAsync(
                DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, null);

            await Task.WhenAll(summaryTask, profileTask, actionsTask);

            var summary = summaryTask.Result;
            var profile = profileTask.Result;
            var recentActions = actionsTask.Result;

            TotalLP = summary.TotalLP;
            CurrentStreak = profile.CurrentStreak;

            var totalSaved = summary.TotalCO2eSaved;
            YearlyCO2e = totalSaved;

            var periodDays = (summary.PeriodEnd - summary.PeriodStart).TotalDays;
            if (periodDays > 0)
            {
                DailyCO2e = totalSaved / periodDays;
                WeeklyCO2e = totalSaved / periodDays * 7;
                MonthlyCO2e = totalSaved / periodDays * 30;
            }

            // Build line chart from recent actions grouped by day
            var grouped = recentActions
                .GroupBy(a => a.LoggedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ChartDataPoint(g.Key, (decimal)g.Sum(a => a.CO2eSaved)))
                .ToList();
            LineChartData = new ObservableCollection<ChartDataPoint>(grouped);

            // Build donut chart by category
            var segments = summary.TotalsByCategory
                .Select(kvp => new CategorySegment(
                    kvp.Key,
                    kvp.Value.CO2eSaved,
                    kvp.Value.ActionCount))
                .OrderByDescending(s => s.CO2eSaved)
                .ToList();
            DonutSegments = new ObservableCollection<CategorySegment>(segments);

            // "You vs average" comparison (average ~4,600 kg CO2e/year)
            AverageUserCO2e = 4600;
            UserCO2e = Math.Max(0, AverageUserCO2e - YearlyCO2e);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public record CategorySegment(ActionCategory Category, double CO2eSaved, int ActionCount);
