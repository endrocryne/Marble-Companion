using MarbleCompanion.Mobile.Services;

namespace MarbleCompanion.Mobile.Views;

public partial class AppShell : Shell
{
    public AppShell(AuthService authService)
    {
        InitializeComponent();

        // Register non-tabbed navigation routes
        Routing.RegisterRoute("quicklog", typeof(QuickLogPage));
        Routing.RegisterRoute("detailedlog", typeof(DetailedLogPage));
        Routing.RegisterRoute("treedetail", typeof(TreeDetailPage));
        Routing.RegisterRoute("habitdetail", typeof(HabitDetailPage));
        Routing.RegisterRoute("habitlibrary", typeof(HabitLibraryPage));
        Routing.RegisterRoute("feed", typeof(FeedPage));
        Routing.RegisterRoute("challenges", typeof(ChallengesPage));
        Routing.RegisterRoute("challengedetail", typeof(ChallengeDetailPage));
        Routing.RegisterRoute("achievements", typeof(AchievementsPage));
        Routing.RegisterRoute("contentlibrary", typeof(ContentLibraryPage));
        Routing.RegisterRoute("contentdetail", typeof(ContentDetailPage));
        Routing.RegisterRoute("offset", typeof(OffsetPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
        Routing.RegisterRoute("suggestions", typeof(SuggestionsPage));

        // Navigate based on auth state
        Dispatcher.Dispatch(async () =>
        {
            var restored = await authService.TryRestoreSessionAsync();
            if (restored)
                await GoToAsync("//main/home/homepage");
            else
                await GoToAsync("//onboarding");
        });
    }
}
