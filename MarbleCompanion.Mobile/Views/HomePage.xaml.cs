using MarbleCompanion.Mobile.ViewModels;

namespace MarbleCompanion.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.LoadCommand.CanExecute(null))
        {
            _viewModel.LoadCommand.Execute(null);
        }

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HomeViewModel.TodaysFact))
            FactCard.IsVisible = _viewModel.TodaysFact is not null;

        if (e.PropertyName == nameof(HomeViewModel.ErrorMessage))
            ErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.ErrorMessage);
    }
}
