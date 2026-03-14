using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface ITreeService
{
    Task<TreeDto> GetTreeAsync(string userId);
    Task UpdateHealthAsync(string userId);
    Task AwardLeafPointsAsync(string userId, int points);
}
