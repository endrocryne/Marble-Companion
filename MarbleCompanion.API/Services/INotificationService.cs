using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface INotificationService
{
    Task<NotificationPreferencesDto> GetPreferencesAsync(string userId);
    Task UpdatePreferencesAsync(string userId, NotificationPreferencesDto dto);
    Task RegisterTokenAsync(string userId, RegisterDeviceTokenDto dto);
}
