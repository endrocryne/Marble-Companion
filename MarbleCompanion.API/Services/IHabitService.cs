using MarbleCompanion.Shared.DTOs;

namespace MarbleCompanion.API.Services;

public interface IHabitService
{
    Task<List<HabitLibraryItemDto>> GetLibraryAsync();
    Task<List<ActiveHabitDto>> GetActiveHabitsAsync(string userId);
    Task<ActiveHabitDto> CreateHabitAsync(string userId, CreateHabitDto dto);
    Task<ActiveHabitDto> UpdateHabitAsync(string userId, Guid habitId, CreateHabitDto dto);
    Task DeleteHabitAsync(string userId, Guid habitId);
    Task<HabitCheckinDto> CheckinAsync(string userId, Guid habitId);
}
