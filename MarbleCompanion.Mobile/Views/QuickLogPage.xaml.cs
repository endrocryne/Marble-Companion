using MarbleCompanion.Mobile.ViewModels;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Views;

public partial class QuickLogPage : ContentPage
{
    private readonly QuickLogViewModel _viewModel;

    public QuickLogPage(QuickLogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateErrorVisibility();
        HighlightSelectedTile();
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
            case nameof(QuickLogViewModel.ErrorMessage):
                UpdateErrorVisibility();
                break;
            case nameof(QuickLogViewModel.SelectedCategory):
                HighlightSelectedTile();
                break;
        }
    }

    private void UpdateErrorVisibility()
    {
        ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(_viewModel.ErrorMessage);
    }

    // ── Category tile tap handlers ──────────────────────────

    private void OnTransportTapped(object? sender, EventArgs e) =>
        _viewModel.SelectedCategory = ActionCategory.Transport;

    private void OnFoodTapped(object? sender, EventArgs e) =>
        _viewModel.SelectedCategory = ActionCategory.Food;

    private void OnEnergyTapped(object? sender, EventArgs e) =>
        _viewModel.SelectedCategory = ActionCategory.Energy;

    private void OnShoppingTapped(object? sender, EventArgs e) =>
        _viewModel.SelectedCategory = ActionCategory.Shopping;

    private void OnTravelTapped(object? sender, EventArgs e) =>
        _viewModel.SelectedCategory = ActionCategory.Travel;

    private void OnWasteTapped(object? sender, EventArgs e) =>
        _viewModel.SelectedCategory = ActionCategory.Waste;

    private void HighlightSelectedTile()
    {
        var tiles = new Dictionary<ActionCategory, Frame>
        {
            [ActionCategory.Transport] = TransportTile,
            [ActionCategory.Food] = FoodTile,
            [ActionCategory.Energy] = EnergyTile,
            [ActionCategory.Shopping] = ShoppingTile,
            [ActionCategory.Travel] = TravelTile,
            [ActionCategory.Waste] = WasteTile,
        };

        var categoryColors = new Dictionary<ActionCategory, Color>
        {
            [ActionCategory.Transport] = Color.FromArgb("#3B82F6"),
            [ActionCategory.Food] = Color.FromArgb("#22C55E"),
            [ActionCategory.Energy] = Color.FromArgb("#F59E0B"),
            [ActionCategory.Shopping] = Color.FromArgb("#A855F7"),
            [ActionCategory.Travel] = Color.FromArgb("#EC4899"),
            [ActionCategory.Waste] = Color.FromArgb("#06B6D4"),
        };

        var selected = _viewModel.SelectedCategory;

        foreach (var (category, tile) in tiles)
        {
            bool isSelected = category == selected;
            tile.BorderColor = isSelected
                ? categoryColors[category]
                : (Color)Application.Current!.Resources["DividerLight"];
            tile.BackgroundColor = isSelected
                ? categoryColors[category].WithAlpha(0.12f)
                : (Color)Application.Current!.Resources["CardLight"];
        }
    }
}
