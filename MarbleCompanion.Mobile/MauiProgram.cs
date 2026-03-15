using MarbleCompanion.Mobile.ML;
using MarbleCompanion.Mobile.Services;
using MarbleCompanion.Mobile.ViewModels;
using MarbleCompanion.Mobile.Views;
using Microsoft.Extensions.Logging;
using Refit;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MarbleCompanion.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                // Georgia is a system font on both platforms; no custom font files needed.
            });

        // Services
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<LocalDatabaseService>();
        builder.Services.AddSingleton<NavigationService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<PreferencesService>();
        builder.Services.AddSingleton<SyncService>();
        builder.Services.AddSingleton<RecommendationEngine>();
        builder.Services.AddSingleton<SuggestionLibrary>();

        // Refit API client
        builder.Services
            .AddRefitClient<IApiService>()
            .ConfigureHttpClient(c =>
            {
                var baseUrl = Preferences.Get("api_base_url", "https://api.marbleclimate.com");
                c.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddTransient<AuthHeaderHandler>();

        // ViewModels
        builder.Services.AddTransient<AppShellViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<SetupViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<TreeDetailViewModel>();
        builder.Services.AddTransient<QuickLogViewModel>();
        builder.Services.AddTransient<DetailedLogViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<HabitsViewModel>();
        builder.Services.AddTransient<HabitDetailViewModel>();
        builder.Services.AddTransient<HabitLibraryViewModel>();
        builder.Services.AddTransient<FriendsViewModel>();
        builder.Services.AddTransient<FeedViewModel>();
        builder.Services.AddTransient<ChallengesViewModel>();
        builder.Services.AddTransient<ChallengeDetailViewModel>();
        builder.Services.AddTransient<AchievementsViewModel>();
        builder.Services.AddTransient<ContentLibraryViewModel>();
        builder.Services.AddTransient<ContentDetailViewModel>();
        builder.Services.AddTransient<OffsetViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SuggestionsViewModel>();

        // Pages
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<SetupPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<TreeDetailPage>();
        builder.Services.AddTransient<QuickLogPage>();
        builder.Services.AddTransient<DetailedLogPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<HabitDetailPage>();
        builder.Services.AddTransient<HabitLibraryPage>();
        builder.Services.AddTransient<FriendsPage>();
        builder.Services.AddTransient<FeedPage>();
        builder.Services.AddTransient<ChallengesPage>();
        builder.Services.AddTransient<ChallengeDetailPage>();
        builder.Services.AddTransient<AchievementsPage>();
        builder.Services.AddTransient<ContentLibraryPage>();
        builder.Services.AddTransient<ContentDetailPage>();
        builder.Services.AddTransient<OffsetPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SuggestionsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthHeaderHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_authService.IsAuthenticated)
        {
            var token = await _authService.GetValidTokenAsync();
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
