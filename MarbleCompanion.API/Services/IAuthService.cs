using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateGoogleAsync(string idToken);
    Task<AuthResponse> AuthenticateMicrosoftAsync(string accessToken);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string userId);
}
