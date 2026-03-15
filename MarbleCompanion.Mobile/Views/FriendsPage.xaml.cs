using MarbleCompanion.Mobile.ViewModels;

namespace MarbleCompanion.Mobile.Views;

public partial class FriendsPage : ContentPage
{
    private readonly FriendsViewModel _viewModel;

    public FriendsPage(FriendsViewModel viewModel)
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
