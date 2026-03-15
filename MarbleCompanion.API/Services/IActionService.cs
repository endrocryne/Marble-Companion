using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Services;

public interface IActionService
{
    Task<LogActionResponse> LogActionAsync(string userId, LogActionRequest request);
    Task<List<CarbonActionDto>> GetActionsAsync(string userId, DateTime? from, DateTime? to, ActionCategory? category);
    Task DeleteActionAsync(string userId, Guid actionId);
    Task<ActionSummaryDto> GetSummaryAsync(string userId);
}
