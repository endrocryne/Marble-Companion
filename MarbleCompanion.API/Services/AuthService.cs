using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MarbleCompanion.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AuthResponse> AuthenticateGoogleAsync(string idToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}");

        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Invalid Google ID token.");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var email = root.GetProperty("email").GetString()
            ?? throw new UnauthorizedAccessException("Google token missing email.");
        var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? email : email;

        var user = await CreateOrUpdateUser(email, name, "Google");
        return await BuildAuthResponse(user);
    }

    public async Task<AuthResponse> AuthenticateMicrosoftAsync(string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Invalid Microsoft access token.");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var email = root.TryGetProperty("mail", out var mailProp) ? mailProp.GetString() : null;
        email ??= root.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null;
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Microsoft token missing email.");

        var name = root.TryGetProperty("displayName", out var dnProp) ? dnProp.GetString() ?? email : email;

        var user = await CreateOrUpdateUser(email, name, "Microsoft");
        return await BuildAuthResponse(user);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken && r.RevokedAt == null);

        if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // Rotate: revoke old token
        stored.RevokedAt = DateTime.UtcNow;
        var newRefreshToken = GenerateRefreshToken();
        stored.ReplacedByToken = newRefreshToken;

        var newStored = new RefreshToken
        {
            UserId = stored.UserId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        _db.RefreshTokens.Add(newStored);
        await _db.SaveChangesAsync();

        var jwt = GenerateJwtToken(stored.User);

        return new RefreshTokenResponse(
            Token: jwt,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );
    }

    public async Task RevokeRefreshTokenAsync(string userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private async Task<ApplicationUser> CreateOrUpdateUser(string email, string name, string provider)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = name,
                JoinedAt = DateTime.UtcNow,
                TreeHealthScore = 100
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, email, provider));
        }
        else
        {
            user.DisplayName = name;
            await _userManager.UpdateAsync(user);
        }

        return user;
    }

    private async Task<AuthResponse> BuildAuthResponse(ApplicationUser user)
    {
        var jwt = GenerateJwtToken(user);
        var refresh = GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var profile = new UserProfileDto
        {
            Id = Guid.Parse(user.Id),
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarIndex = user.AvatarIndex,
            RegionContinent = user.Region,
            CurrentStreak = user.StreakCurrent,
            LongestStreak = user.StreakBest,
            TotalLP = user.TotalLeafPoints,
            JoinedAt = user.JoinedAt
        };

        return new AuthResponse(
            Token: jwt,
            RefreshToken: refresh,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            User: profile
        );
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.")));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
