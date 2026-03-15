using MarbleCompanion.Mobile.ViewModels;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Views;

public partial class HabitLibraryPage : ContentPage
{
    private readonly HabitLibraryViewModel _viewModel;

    public HabitLibraryPage(HabitLibraryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    private void OnClearFilters(object? sender, EventArgs e) => _viewModel.ClearFiltersCommand.Execute(null);
    private void OnFilterTransport(object? sender, EventArgs e) => _viewModel.FilterCategory = ActionCategory.Transport;
    private void OnFilterFood(object? sender, EventArgs e) => _viewModel.FilterCategory = ActionCategory.Food;
    private void OnFilterEnergy(object? sender, EventArgs e) => _viewModel.FilterCategory = ActionCategory.Energy;
    private void OnFilterShopping(object? sender, EventArgs e) => _viewModel.FilterCategory = ActionCategory.Shopping;
    private void OnFilterTravel(object? sender, EventArgs e) => _viewModel.FilterCategory = ActionCategory.Travel;
    private void OnFilterWaste(object? sender, EventArgs e) => _viewModel.FilterCategory = ActionCategory.Waste;
}
