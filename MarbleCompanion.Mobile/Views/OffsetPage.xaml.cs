using MarbleCompanion.Mobile.ViewModels;

namespace MarbleCompanion.Mobile.Views;

public partial class OffsetPage : ContentPage
{
    private readonly OffsetViewModel _viewModel;

    public OffsetPage(OffsetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
