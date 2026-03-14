using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface IContentService
{
    Task<List<ContentSummaryDto>> GetAllAsync();
    Task<ContentDto> GetByIdAsync(Guid id, string userId);
    Task BookmarkAsync(string userId, Guid contentId);
    Task<ContentDto> CreateAsync(CreateContentDto dto);
    Task<ContentSummaryDto> GetTodaysFactAsync();
    Task SeedContentAsync();
}
