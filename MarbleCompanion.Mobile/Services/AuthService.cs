using MarbleCompanion.Mobile.Models;
using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.Mobile.Services;

public class AuthService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string TokenExpiryKey = "token_expiry";
    private const string UserIdKey = "user_id";

    private readonly IApiService _api;
    private UserSession? _currentSession;

    public AuthService(IApiService api)
    {
        _api = api;
    }

    public UserSession? CurrentSession => _currentSession;
    public bool IsAuthenticated => _currentSession?.IsAuthenticated == true;

    public async Task<UserSession> SignInWithGoogleAsync(string idToken)
    {
        var response = await _api.GoogleSignInAsync(new GoogleAuthRequest { IdToken = idToken });
        return await StoreSessionAsync(response);
    }

    public async Task<UserSession> SignInWithMicrosoftAsync(string accessToken)
    {
        var response = await _api.MicrosoftSignInAsync(new MicrosoftAuthRequest { AccessToken = accessToken });
        return await StoreSessionAsync(response);
    }

    public async Task<string> GetValidTokenAsync()
    {
        if (_currentSession == null)
            await TryRestoreSessionAsync();

        if (_currentSession == null)
            throw new UnauthorizedAccessException("No active session");

        if (_currentSession.TokenExpiry <= DateTime.UtcNow.AddMinutes(2))
        {
            await RefreshTokenAsync();
        }

        return _currentSession.AccessToken;
    }

    public async Task RefreshTokenAsync()
    {
        var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("No refresh token available");

        var response = await _api.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });

        await SecureStorage.SetAsync(AccessTokenKey, response.Token);
        await SecureStorage.SetAsync(RefreshTokenKey, response.RefreshToken);
        var expiry = DateTime.UtcNow.AddHours(1);
        await SecureStorage.SetAsync(TokenExpiryKey, expiry.ToString("O"));

        if (_currentSession != null)
        {
            _currentSession.AccessToken = response.Token;
            _currentSession.RefreshToken = response.RefreshToken;
            _currentSession.TokenExpiry = expiry;
        }
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync(AccessTokenKey);
            var refresh = await SecureStorage.GetAsync(RefreshTokenKey);
            var expiryStr = await SecureStorage.GetAsync(TokenExpiryKey);
            var userId = await SecureStorage.GetAsync(UserIdKey);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refresh))
                return false;

            var expiry = DateTime.TryParse(expiryStr, out var e) ? e : DateTime.MinValue;

            _currentSession = new UserSession
            {
                UserId = userId ?? string.Empty,
                AccessToken = token,
                RefreshToken = refresh,
                TokenExpiry = expiry
            };

            if (expiry <= DateTime.UtcNow)
            {
                await RefreshTokenAsync();
            }

            var profile = await _api.GetMyProfileAsync();
            _currentSession.DisplayName = profile.DisplayName;
            _currentSession.Email = profile.Email;
            _currentSession.AvatarUrl = profile.AvatarUrl;
            _currentSession.Region = profile.Region;
            _currentSession.CurrentStreak = profile.CurrentStreak;
            _currentSession.TotalLP = profile.TotalLP;
            _currentSession.IsSetupComplete = true;

            return true;
        }
        catch
        {
            _currentSession = null;
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        try { await _api.LogoutAsync(); } catch { }
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
        SecureStorage.Remove(TokenExpiryKey);
        SecureStorage.Remove(UserIdKey);
        _currentSession = null;
    }

    private async Task<UserSession> StoreSessionAsync(AuthResponse response)
    {
        await SecureStorage.SetAsync(AccessTokenKey, response.Token);
        await SecureStorage.SetAsync(RefreshTokenKey, response.RefreshToken);
        await SecureStorage.SetAsync(TokenExpiryKey, response.Expiry.ToString("O"));
        await SecureStorage.SetAsync(UserIdKey, response.User.Id);

        _currentSession = new UserSession
        {
            UserId = response.User.Id,
            DisplayName = response.User.DisplayName,
            Email = response.User.Email,
            AvatarUrl = response.User.AvatarUrl,
            Region = response.User.Region,
            AccessToken = response.Token,
            RefreshToken = response.RefreshToken,
            TokenExpiry = response.Expiry,
            CurrentStreak = response.User.CurrentStreak,
            TotalLP = response.User.TotalLP,
            IsSetupComplete = true
        };

        return _currentSession;
    }
}
