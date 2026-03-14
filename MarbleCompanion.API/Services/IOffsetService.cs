using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface IOffsetService
{
    Task<List<OffsetCreditDto>> GetCreditsAsync(string userId);
    Task<OffsetHistoryDto> RedeemAsync(string userId, RedeemOffsetRequest request);
    Task<List<OffsetHistoryDto>> GetHistoryAsync(string userId);
}
