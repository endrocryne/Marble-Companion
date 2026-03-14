using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface IChallengeService
{
    Task<List<ChallengeDto>> GetCuratedAsync();
    Task<List<ChallengeDto>> GetUserChallengesAsync(string userId);
    Task<ChallengeDto> CreateChallengeAsync(string userId, CreateChallengeDto dto);
    Task<ChallengeDto> GetChallengeAsync(Guid id);
    Task<ChallengeParticipantDto> JoinChallengeAsync(string userId, Guid challengeId);
    Task<ChallengeProgressDto> UpdateProgressAsync(string userId, Guid challengeId, decimal progress);
    Task DeleteChallengeAsync(string userId, Guid challengeId);
}
