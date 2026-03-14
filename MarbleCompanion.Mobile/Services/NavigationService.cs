namespace MarbleCompanion.Mobile.Services;

public class NavigationService
{
    public async Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters != null)
            await Shell.Current.GoToAsync(route, parameters);
        else
            await Shell.Current.GoToAsync(route);
    }

    public async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");

    public async Task GoToRootAsync() =>
        await Shell.Current.GoToAsync("//home");

    public async Task PushModalAsync(string route) =>
        await Shell.Current.GoToAsync(route, true);

    public async Task PopModalAsync() =>
        await Shell.Current.GoToAsync("..", true);
}
