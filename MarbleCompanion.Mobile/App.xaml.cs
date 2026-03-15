using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Mobile.Views;

namespace MarbleCompanion.Mobile;

public partial class App : Application
{
    private readonly AuthService _authService;

    public App(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell(_authService));
    }
}
