using MarbleCompanion.Mobile.Controls;
using MarbleCompanion.Mobile.ViewModels;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    private static readonly Dictionary<ActionCategory, Color> CategoryColors = new()
    {
        [ActionCategory.Transport] = Color.FromArgb("#3B82F6"),
        [ActionCategory.Food] = Color.FromArgb("#22C55E"),
        [ActionCategory.Energy] = Color.FromArgb("#F59E0B"),
        [ActionCategory.Shopping] = Color.FromArgb("#A855F7"),
        [ActionCategory.Travel] = Color.FromArgb("#EC4899"),
        [ActionCategory.Waste] = Color.FromArgb("#06B6D4"),
    };

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.LoadCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DashboardViewModel.LineChartData):
                UpdateLineChart();
                break;
            case nameof(DashboardViewModel.DonutSegments):
                UpdateDonutChart();
                break;
            case nameof(DashboardViewModel.ErrorMessage):
                ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(_viewModel.ErrorMessage);
                break;
            case nameof(DashboardViewModel.UserCO2e):
            case nameof(DashboardViewModel.AverageUserCO2e):
                UpdateComparisonBar();
                break;
        }
    }

    private void UpdateLineChart()
    {
        if (_viewModel.LineChartData is null) return;

        TrendChart.DataPoints = _viewModel.LineChartData
            .Select(p => new ChartDataPoint(p.Date, (decimal)p.Value))
            .ToList();
    }

    private void UpdateDonutChart()
    {
        if (_viewModel.DonutSegments is null) return;

        CategoryDonut.Segments = _viewModel.DonutSegments
            .Select(s => new DonutSegment(
                s.Category.ToString(),
                (decimal)s.CO2eSaved,
                CategoryColors.GetValueOrDefault(s.Category, Colors.Grey)))
            .ToList();
    }

    private void UpdateComparisonBar()
    {
        var average = _viewModel.AverageUserCO2e;
        if (average <= 0) return;

        var ratio = Math.Clamp(_viewModel.UserCO2e / average, 0, 1);
        ComparisonBar.WidthRequest = ratio * (Width - 72);
    }

    // ── Period chip tap handlers ────────────────────────────

    private void OnDailyChipTapped(object? sender, EventArgs e) =>
        SelectChip(DailyChip, DailyChipLabel);

    private void OnWeeklyChipTapped(object? sender, EventArgs e) =>
        SelectChip(WeeklyChip, WeeklyChipLabel);

    private void OnMonthlyChipTapped(object? sender, EventArgs e) =>
        SelectChip(MonthlyChip, MonthlyChipLabel);

    private void OnYearlyChipTapped(object? sender, EventArgs e) =>
        SelectChip(YearlyChip, YearlyChipLabel);

    private void SelectChip(Frame selected, Label selectedLabel)
    {
        var chips = new[] { DailyChip, WeeklyChip, MonthlyChip, YearlyChip };
        var labels = new[] { DailyChipLabel, WeeklyChipLabel, MonthlyChipLabel, YearlyChipLabel };

        for (int i = 0; i < chips.Length; i++)
        {
            bool isSelected = chips[i] == selected;
            chips[i].BackgroundColor = isSelected
                ? (Color)Application.Current!.Resources["Primary"]
                : (Color)Application.Current!.Resources["SurfaceLight"];
            chips[i].BorderColor = isSelected
                ? (Color)Application.Current!.Resources["Primary"]
                : (Color)Application.Current!.Resources["DividerLight"];
            labels[i].TextColor = isSelected
                ? (Color)Application.Current!.Resources["TextOnPrimary"]
                : (Color)Application.Current!.Resources["TextPrimary"];
        }
    }
}
