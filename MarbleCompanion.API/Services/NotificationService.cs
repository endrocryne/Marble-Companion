using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<NotificationPreferencesDto> GetPreferencesAsync(string userId)
    {
        var pref = await _db.NotificationPreferences
            .FirstOrDefaultAsync(n => n.UserId == userId);

        if (pref == null)
        {
            return new NotificationPreferencesDto
            {
                HabitReminders = true,
                ChallengeUpdates = true,
                FriendActivity = true,
                AchievementAlerts = true,
                WeeklyReport = true
            };
        }

        return new NotificationPreferencesDto
        {
            HabitReminders = pref.DailyReminder,
            ChallengeUpdates = pref.ChallengeEnding || pref.NewChallenge,
            FriendActivity = pref.FriendMilestone || pref.FriendRequest,
            AchievementAlerts = pref.AchievementUnlocked,
            WeeklyReport = pref.DailyReminder
        };
    }

    public async Task UpdatePreferencesAsync(string userId, NotificationPreferencesDto dto)
    {
        var pref = await _db.NotificationPreferences
            .FirstOrDefaultAsync(n => n.UserId == userId);

        if (pref == null)
        {
            pref = new NotificationPreference
            {
                UserId = userId,
                DailyReminder = dto.HabitReminders,
                StreakAtRisk = dto.HabitReminders,
                FriendMilestone = dto.FriendActivity,
                ChallengeEnding = dto.ChallengeUpdates,
                NewChallenge = dto.ChallengeUpdates,
                AchievementUnlocked = dto.AchievementAlerts,
                FriendRequest = dto.FriendActivity
            };
            _db.NotificationPreferences.Add(pref);
        }
        else
        {
            pref.DailyReminder = dto.HabitReminders;
            pref.StreakAtRisk = dto.HabitReminders;
            pref.FriendMilestone = dto.FriendActivity;
            pref.ChallengeEnding = dto.ChallengeUpdates;
            pref.NewChallenge = dto.ChallengeUpdates;
            pref.AchievementUnlocked = dto.AchievementAlerts;
            pref.FriendRequest = dto.FriendActivity;
        }

        await _db.SaveChangesAsync();
    }

    public async Task RegisterTokenAsync(string userId, RegisterDeviceTokenDto dto)
    {
        var existing = await _db.DeviceTokens
            .FirstOrDefaultAsync(d => d.Token == dto.Token);

        if (existing != null)
        {
            existing.UserId = userId;
            existing.Platform = dto.Platform;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.DeviceTokens.Add(new DeviceToken
            {
                UserId = userId,
                Token = dto.Token,
                Platform = dto.Platform,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }
}
