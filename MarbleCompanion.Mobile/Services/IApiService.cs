using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Refit;

namespace MarbleCompanion.Mobile.Services;

[Headers("Authorization: Bearer")]
public interface IApiService
{
    // Auth (no auth header needed for these)
    [Post("/api/auth/google")]
    [Headers("Authorization:")]
    Task<AuthResponse> GoogleSignInAsync([Body] GoogleAuthRequest request);

    [Post("/api/auth/microsoft")]
    [Headers("Authorization:")]
    Task<AuthResponse> MicrosoftSignInAsync([Body] MicrosoftAuthRequest request);

    [Post("/api/auth/refresh")]
    [Headers("Authorization:")]
    Task<RefreshTokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);

    [Delete("/api/auth/logout")]
    Task LogoutAsync();

    // Users
    [Get("/api/users/me")]
    Task<UserProfileDto> GetMyProfileAsync();

    [Put("/api/users/me")]
    Task<UserProfileDto> UpdateProfileAsync([Body] UpdateUserDto dto);

    [Delete("/api/users/me")]
    Task DeleteAccountAsync();

    [Post("/api/users/me/setup")]
    Task<UserProfileDto> SetupAccountAsync([Body] UserSetupDto dto);

    [Get("/api/users/{username}")]
    Task<UserSearchResultDto> GetUserByUsernameAsync(string username);

    [Get("/api/users/search")]
    Task<List<UserSearchResultDto>> SearchUsersAsync([Query] string q);

    [Get("/api/users/me/export")]
    Task<HttpContent> ExportDataAsync();

    // Actions
    [Post("/api/actions")]
    Task<LogActionResponse> LogActionAsync([Body] LogActionRequest request);

    [Get("/api/actions")]
    Task<List<CarbonActionDto>> GetActionsAsync([Query] DateTime? from, [Query] DateTime? to, [Query] ActionCategory? category);

    [Delete("/api/actions/{id}")]
    Task DeleteActionAsync(string id);

    [Get("/api/actions/summary")]
    Task<ActionSummaryDto> GetActionSummaryAsync();

    // Achievements
    [Get("/api/achievements")]
    Task<List<AchievementDto>> GetAchievementsAsync();

    [Get("/api/achievements/unlocked")]
    Task<List<UserAchievementDto>> GetUnlockedAchievementsAsync();

    // Challenges
    [Get("/api/challenges/curated")]
    Task<List<ChallengeDto>> GetCuratedChallengesAsync();

    [Get("/api/challenges/mine")]
    Task<List<ChallengeDto>> GetMyChallengesAsync();

    [Post("/api/challenges")]
    Task<ChallengeDto> CreateChallengeAsync([Body] CreateChallengeDto dto);

    [Get("/api/challenges/{id}")]
    Task<ChallengeDto> GetChallengeAsync(string id);

    [Post("/api/challenges/{id}/join")]
    Task<ChallengeParticipantDto> JoinChallengeAsync(string id);

    [Delete("/api/challenges/{id}")]
    Task DeleteChallengeAsync(string id);

    // Content
    [Get("/api/content")]
    Task<List<ContentSummaryDto>> GetContentListAsync();

    [Get("/api/content/today")]
    Task<ContentSummaryDto> GetTodayContentAsync();

    [Get("/api/content/{id}")]
    Task<ContentDto> GetContentAsync(string id);

    [Post("/api/content/{id}/bookmark")]
    Task BookmarkContentAsync(string id);

    // Friends
    [Get("/api/friends")]
    Task<List<FriendDto>> GetFriendsAsync();

    [Post("/api/friends/request")]
    Task<FriendRequestDto> SendFriendRequestAsync([Body] object body);

    [Put("/api/friends/request/{id}/accept")]
    Task<FriendDto> AcceptFriendRequestAsync(string id);

    [Delete("/api/friends/{id}")]
    Task RemoveFriendAsync(string id);

    [Get("/api/friends/feed")]
    Task<List<FeedEventDto>> GetFeedAsync();

    [Get("/api/friends/{id}/tree")]
    Task<TreeDto> GetFriendTreeAsync(string id);

    [Post("/api/friends/feed/{eventId}/react")]
    Task ReactToFeedEventAsync(string eventId, [Body] FeedReactionDto reaction);

    // Habits
    [Get("/api/habits/library")]
    Task<List<HabitLibraryItemDto>> GetHabitLibraryAsync();

    [Get("/api/habits/active")]
    Task<List<ActiveHabitDto>> GetActiveHabitsAsync();

    [Post("/api/habits")]
    Task<ActiveHabitDto> CreateHabitAsync([Body] CreateHabitDto dto);

    [Put("/api/habits/{id}")]
    Task<ActiveHabitDto> UpdateHabitAsync(string id, [Body] CreateHabitDto dto);

    [Delete("/api/habits/{id}")]
    Task DeleteHabitAsync(string id);

    [Post("/api/habits/{id}/checkin")]
    Task<HabitCheckinDto> CheckinHabitAsync(string id);

    // Notifications
    [Put("/api/notifications/preferences")]
    Task UpdateNotificationPreferencesAsync([Body] NotificationPreferencesDto dto);

    [Post("/api/notifications/register-token")]
    Task RegisterDeviceTokenAsync([Body] RegisterDeviceTokenDto dto);

    // Offsets
    [Get("/api/offsets/credits")]
    Task<List<OffsetCreditDto>> GetOffsetCreditsAsync();

    [Post("/api/offsets/redeem")]
    Task<OffsetHistoryDto> RedeemOffsetAsync([Body] RedeemOffsetRequest request);

    [Get("/api/offsets/history")]
    Task<List<OffsetHistoryDto>> GetOffsetHistoryAsync();

    // Tree
    [Get("/api/tree")]
    Task<TreeDto> GetTreeAsync();
}
