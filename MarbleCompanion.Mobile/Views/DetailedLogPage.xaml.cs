using MarbleCompanion.Mobile.ViewModels;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Views;

public partial class DetailedLogPage : ContentPage
{
    private readonly DetailedLogViewModel _viewModel;

    public DetailedLogPage(DetailedLogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateCategoryVisibility();
        UpdateErrorVisibility();
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
            case nameof(DetailedLogViewModel.Category):
                UpdateCategoryVisibility();
                break;
            case nameof(DetailedLogViewModel.ErrorMessage):
                UpdateErrorVisibility();
                break;
        }
    }

    private void UpdateCategoryVisibility()
    {
        var category = _viewModel.Category;

        TransportSection.IsVisible = category == ActionCategory.Transport;
        FoodSection.IsVisible = category == ActionCategory.Food;
        EnergySection.IsVisible = category == ActionCategory.Energy;
        ShoppingSection.IsVisible = category == ActionCategory.Shopping;
        TravelSection.IsVisible = category == ActionCategory.Travel;
        WasteSection.IsVisible = category == ActionCategory.Waste;
    }

    private void UpdateErrorVisibility()
    {
        ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(_viewModel.ErrorMessage);
    }
}
