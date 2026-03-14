using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.Services;

public class NotificationService
{
    private readonly IApiService _api;
    private readonly AuthService _auth;

    public NotificationService(IApiService api, AuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // FCM initialization is platform-specific; token retrieval
            // would be done via Plugin.Firebase.CloudMessaging in real builds.
            var token = await GetFcmTokenAsync();
            if (!string.IsNullOrEmpty(token) && _auth.IsAuthenticated)
            {
                await RegisterTokenWithServerAsync(token);
            }
        }
        catch (Exception)
        {
            // Notification setup is non-critical
        }
    }

    public async Task RegisterTokenWithServerAsync(string fcmToken)
    {
        var dto = new RegisterDeviceTokenDto
        {
            Token = fcmToken,
            Platform = DeviceInfo.Platform == DevicePlatform.Android ? "android" : "ios"
        };
        await _api.RegisterDeviceTokenAsync(dto);
    }

    public async Task UpdatePreferencesAsync(NotificationPreferencesDto prefs)
    {
        await _api.UpdateNotificationPreferencesAsync(prefs);
    }

    private Task<string> GetFcmTokenAsync()
    {
        // Plugin.Firebase.CloudMessaging provides CrossFirebaseCloudMessaging.Current.GetTokenAsync()
        // Returning empty for compilation; actual implementation uses Firebase plugin.
        return Task.FromResult(string.Empty);
    }
}
