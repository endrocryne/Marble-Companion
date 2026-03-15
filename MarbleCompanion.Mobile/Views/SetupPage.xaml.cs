using MarbleCompanion.Mobile.ViewModels;

namespace MarbleCompanion.Mobile.Views;

public partial class SetupPage : ContentPage
{
    private readonly SetupViewModel _viewModel;

    public SetupPage(SetupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SetupViewModel.ErrorMessage))
                ErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.ErrorMessage);
        };
    }

    private void OnAvatarTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Frame frame &&
            frame.GestureRecognizers[0] is TapGestureRecognizer tap &&
            tap.CommandParameter is string param &&
            int.TryParse(param, out var index))
        {
            _viewModel.SelectedAvatarIndex = index;
            HighlightSelectedAvatar(frame);
        }
    }

    private void HighlightSelectedAvatar(Frame selected)
    {
        if (selected.Parent is HorizontalStackLayout stack)
        {
            foreach (var child in stack.Children.OfType<Frame>())
            {
                child.BorderColor = (Color)Application.Current!.Resources["DisabledLight"];
                child.BackgroundColor = (Color)Application.Current!.Resources["SurfaceLight"];
            }
        }

        selected.BorderColor = (Color)Application.Current!.Resources["Primary"];
        selected.BackgroundColor = (Color)Application.Current!.Resources["Accent"];
    }

    private void OnDietChipTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is string option)
            _viewModel.DietType = option;
    }

    private void OnTransportChipTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is string option)
            _viewModel.TransportMode = option;
    }

    private void OnEnergyChipTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is string option)
            _viewModel.EnergySource = option;
    }

    private void OnFlightChipTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is string option)
            _viewModel.FlightFrequency = option;
    }

    private void OnShoppingChipTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is string option)
            _viewModel.ShoppingHabits = option;
    }
}
