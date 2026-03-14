using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateUserDto dto);
    Task DeleteUserAsync(string userId);
    Task<List<UserSearchResultDto>> SearchUsersAsync(string query);
    Task<UserProfileDto> SetupUserAsync(string userId, UserSetupDto dto);
    Task<byte[]> ExportUserDataAsync(string userId);
}
