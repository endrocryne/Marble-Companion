using MarbleCompanion.Mobile.ViewModels;

namespace MarbleCompanion.Mobile.Views;

public partial class ChallengesPage : ContentPage
{
    private readonly ChallengesViewModel _viewModel;

    public ChallengesPage(ChallengesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    private void OnCuratedTapped(object? sender, EventArgs e)
    {
        curatedList.IsVisible = true;
        myList.IsVisible = false;
        btnCurated.Style = null;
        btnMine.Style = (Style)Resources.MergedDictionaries.FirstOrDefault()?["SecondaryButton"]
            ?? Application.Current!.Resources["SecondaryButton"] as Style;
    }

    private void OnMineTapped(object? sender, EventArgs e)
    {
        curatedList.IsVisible = false;
        myList.IsVisible = true;
        btnMine.Style = null;
        btnCurated.Style = (Style)Resources.MergedDictionaries.FirstOrDefault()?["SecondaryButton"]
            ?? Application.Current!.Resources["SecondaryButton"] as Style;
    }
}
