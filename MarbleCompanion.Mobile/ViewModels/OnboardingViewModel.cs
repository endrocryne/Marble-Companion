using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarbleCompanion.Mobile.Services;

namespace MarbleCompanion.Mobile.ViewModels;

[ObservableObject]
public partial class OnboardingViewModel
{
    private const int TotalPages = 3;

    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private int _currentPage;

    [ObservableProperty]
    private bool _isLastPage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public OnboardingViewModel(AuthService authService, NavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    partial void OnCurrentPageChanged(int value)
    {
        IsLastPage = value >= TotalPages - 1;
    }

    [RelayCommand]
    private void Skip()
    {
        CurrentPage = TotalPages - 1;
    }

    [RelayCommand]
    private void Next()
    {
        if (CurrentPage < TotalPages - 1)
            CurrentPage++;
    }

    [RelayCommand]
    private async Task SignInWithGoogleAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var idToken = await GetGoogleIdTokenAsync();
            var session = await _authService.SignInWithGoogleAsync(idToken);

            if (session.IsSetupComplete)
                await _navigationService.GoToAsync("//home");
            else
                await _navigationService.GoToAsync("//setup");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Google sign-in failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SignInWithMicrosoftAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var accessToken = await GetMicrosoftAccessTokenAsync();
            var session = await _authService.SignInWithMicrosoftAsync(accessToken);

            if (session.IsSetupComplete)
                await _navigationService.GoToAsync("//home");
            else
                await _navigationService.GoToAsync("//setup");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Microsoft sign-in failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<string> GetGoogleIdTokenAsync()
    {
#if ANDROID
        await Task.CompletedTask;
        // Platform-specific Google sign-in returns an ID token
        return string.Empty;
#elif IOS
        await Task.CompletedTask;
        return string.Empty;
#else
        await Task.CompletedTask;
        return string.Empty;
#endif
    }

    private static async Task<string> GetMicrosoftAccessTokenAsync()
    {
#if ANDROID
        await Task.CompletedTask;
        return string.Empty;
#elif IOS
        await Task.CompletedTask;
        return string.Empty;
#else
        await Task.CompletedTask;
        return string.Empty;
#endif
    }
}
