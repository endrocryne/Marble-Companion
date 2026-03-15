using MarbleCompanion.Mobile.ViewModels;

namespace MarbleCompanion.Mobile.Views;

public partial class TreeDetailPage : ContentPage
{
    private readonly TreeDetailViewModel _viewModel;

    public TreeDetailPage(TreeDetailViewModel viewModel)
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
        UpdateDerivedBindings();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TreeDetailViewModel.HealthScore)
            or nameof(TreeDetailViewModel.ActiveCosmetics)
            or nameof(TreeDetailViewModel.ErrorMessage))
        {
            UpdateDerivedBindings();
        }
    }

    private void UpdateDerivedBindings()
    {
        HealthBar.Progress = _viewModel.HealthScore / 100.0;
        NoCosmeticsLabel.IsVisible = _viewModel.ActiveCosmetics is null || _viewModel.ActiveCosmetics.Count == 0;
        ErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.ErrorMessage);
    }
}
