using System.Text.Json.Serialization;

namespace MarbleCompanion.Shared.DTOs;

public record GoogleAuthRequest(
    [property: JsonPropertyName("idToken")] string IdToken
);

public record MicrosoftAuthRequest(
    [property: JsonPropertyName("accessToken")] string AccessToken
);

public record AuthResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt,
    [property: JsonPropertyName("user")] UserProfileDto User
);

public record RefreshTokenRequest(
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);

public record RefreshTokenResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt
);
