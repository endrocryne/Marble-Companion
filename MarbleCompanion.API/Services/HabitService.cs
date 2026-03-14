using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class HabitService : IHabitService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITreeService _treeService;
    private readonly IAchievementService _achievementService;

    public HabitService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        ITreeService treeService,
        IAchievementService achievementService)
    {
        _db = db;
        _userManager = userManager;
        _treeService = treeService;
        _achievementService = achievementService;
    }

    public Task<List<HabitLibraryItemDto>> GetLibraryAsync()
    {
        var items = HabitLibrary.Items.Select(h => new HabitLibraryItemDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            Category = h.Category,
            Frequency = h.Frequency,
            EstimatedCO2ePerAction = h.EstimatedCO2ePerAction
        }).ToList();

        return Task.FromResult(items);
    }

    public async Task<List<ActiveHabitDto>> GetActiveHabitsAsync(string userId)
    {
        var habits = await _db.Habits
            .Include(h => h.Checkins)
            .Where(h => h.UserId == userId && h.IsActive)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        return habits.Select(h => new ActiveHabitDto
        {
            Id = h.Id,
            HabitLibraryItemId = h.LibraryItemId != null ? Guid.Parse(h.LibraryItemId) : Guid.Empty,
            Name = h.Name,
            Category = h.Category,
            Frequency = h.Frequency,
            CurrentStreak = h.CurrentStreak,
            LongestStreak = h.BestStreak,
            LastCheckinAt = h.Checkins.MaxBy(c => c.CheckedInAt)?.CheckedInAt,
            IsCheckedInToday = h.Checkins.Any(c => c.CheckedInAt.Date == today)
        }).ToList();
    }

    public async Task<ActiveHabitDto> CreateHabitAsync(string userId, CreateHabitDto dto)
    {
        var activeCount = await _db.Habits.CountAsync(h => h.UserId == userId && h.IsActive);
        if (activeCount >= AppConstants.MaxActiveHabits)
            throw new InvalidOperationException($"Maximum of {AppConstants.MaxActiveHabits} active habits allowed.");

        var libraryItem = HabitLibrary.Items.FirstOrDefault(h => h.Id == dto.HabitLibraryItemId)
            ?? throw new KeyNotFoundException("Habit library item not found.");

        var habit = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = libraryItem.Name,
            Category = libraryItem.Category,
            Frequency = libraryItem.Frequency,
            LeafPointsReward = libraryItem.LPReward,
            EstimatedCO2eImpact = (decimal)libraryItem.EstimatedCO2ePerAction,
            IsCustom = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LibraryItemId = libraryItem.Id.ToString()
        };

        _db.Habits.Add(habit);
        await _db.SaveChangesAsync();

        return new ActiveHabitDto
        {
            Id = habit.Id,
            HabitLibraryItemId = libraryItem.Id,
            Name = habit.Name,
            Category = habit.Category,
            Frequency = habit.Frequency,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastCheckinAt = null,
            IsCheckedInToday = false
        };
    }

    public async Task<ActiveHabitDto> UpdateHabitAsync(string userId, Guid habitId, CreateHabitDto dto)
    {
        var habit = await _db.Habits.Include(h => h.Checkins)
            .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId)
            ?? throw new KeyNotFoundException("Habit not found.");

        var libraryItem = HabitLibrary.Items.FirstOrDefault(h => h.Id == dto.HabitLibraryItemId)
            ?? throw new KeyNotFoundException("Habit library item not found.");

        habit.Name = libraryItem.Name;
        habit.Category = libraryItem.Category;
        habit.Frequency = libraryItem.Frequency;
        habit.LeafPointsReward = libraryItem.LPReward;
        habit.EstimatedCO2eImpact = (decimal)libraryItem.EstimatedCO2ePerAction;
        habit.LibraryItemId = libraryItem.Id.ToString();

        await _db.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        return new ActiveHabitDto
        {
            Id = habit.Id,
            HabitLibraryItemId = libraryItem.Id,
            Name = habit.Name,
            Category = habit.Category,
            Frequency = habit.Frequency,
            CurrentStreak = habit.CurrentStreak,
            LongestStreak = habit.BestStreak,
            LastCheckinAt = habit.Checkins.MaxBy(c => c.CheckedInAt)?.CheckedInAt,
            IsCheckedInToday = habit.Checkins.Any(c => c.CheckedInAt.Date == today)
        };
    }

    public async Task DeleteHabitAsync(string userId, Guid habitId)
    {
        var habit = await _db.Habits.FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId)
            ?? throw new KeyNotFoundException("Habit not found.");

        _db.Habits.Remove(habit);
        await _db.SaveChangesAsync();
    }

    public async Task<HabitCheckinDto> CheckinAsync(string userId, Guid habitId)
    {
        var habit = await _db.Habits.Include(h => h.Checkins)
            .FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == userId && h.IsActive)
            ?? throw new KeyNotFoundException("Active habit not found.");

        var today = DateTime.UtcNow.Date;
        if (habit.Checkins.Any(c => c.CheckedInAt.Date == today))
            throw new InvalidOperationException("Already checked in today.");

        var checkin = new HabitCheckin
        {
            Id = Guid.NewGuid(),
            HabitId = habitId,
            UserId = userId,
            CheckedInAt = DateTime.UtcNow
        };
        _db.HabitCheckins.Add(checkin);

        // Update habit streak
        var lastCheckin = habit.Checkins
            .Where(c => c.CheckedInAt.Date < today)
            .MaxBy(c => c.CheckedInAt);

        if (lastCheckin?.CheckedInAt.Date == today.AddDays(-1))
        {
            habit.CurrentStreak++;
        }
        else
        {
            habit.CurrentStreak = 1;
        }
        habit.BestStreak = Math.Max(habit.BestStreak, habit.CurrentStreak);

        await _db.SaveChangesAsync();

        int lpAwarded = LPAwards.HabitCheckin;

        // Check streak milestones for bonus LP
        int milestoneBonus = LPAwards.GetStreakMilestoneLP(habit.CurrentStreak);
        lpAwarded += milestoneBonus;

        if (milestoneBonus > 0)
        {
            _db.FeedEvents.Add(new FeedEvent
            {
                UserId = userId,
                EventType = FeedEventType.StreakMilestone,
                Title = $"{habit.CurrentStreak}-Day Streak!",
                Description = $"Reached a {habit.CurrentStreak}-day streak on {habit.Name}!",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.FeedExpiryDays)
            });
            await _db.SaveChangesAsync();
        }

        // Award LP
        await _treeService.AwardLeafPointsAsync(userId, lpAwarded);

        // Check achievements
        await _achievementService.CheckAndUnlockAsync(userId);

        return new HabitCheckinDto
        {
            HabitId = habitId,
            CheckedInAt = checkin.CheckedInAt,
            LPAwarded = lpAwarded,
            CO2eSaved = (double)habit.EstimatedCO2eImpact,
            NewStreak = habit.CurrentStreak
        };
    }
}
