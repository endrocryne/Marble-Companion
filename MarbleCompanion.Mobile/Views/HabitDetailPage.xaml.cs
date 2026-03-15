using MarbleCompanion.Mobile.ViewModels;

namespace MarbleCompanion.Mobile.Views;

public partial class HabitDetailPage : ContentPage
{
    private readonly HabitDetailViewModel _viewModel;

    public HabitDetailPage(HabitDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(HabitDetailViewModel.HeatmapData))
                UpdateHeatmap();
        };
        UpdateHeatmap();
    }

    private void UpdateHeatmap()
    {
        if (_viewModel.HeatmapData is null) return;
        var dict = new Dictionary<DateTime, int>();
        foreach (var entry in _viewModel.HeatmapData)
            dict[entry.Date] = entry.Completed ? 1 : 0;
        heatmapControl.Data = dict;
    }
}
