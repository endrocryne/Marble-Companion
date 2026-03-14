using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface IAchievementService
{
    Task<List<AchievementDto>> GetAllAsync();
    Task<List<UserAchievementDto>> GetUnlockedAsync(string userId);
    Task<List<UserAchievementDto>> CheckAndUnlockAsync(string userId);
    Task SeedAchievementsAsync();
}
