using CommunityToolkit.Mvvm.ComponentModel;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class AppShellViewModel
{
    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _currentRoute = string.Empty;

    [ObservableProperty]
    private bool _isSetupComplete;
}
