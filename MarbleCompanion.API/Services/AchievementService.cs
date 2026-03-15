using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class AchievementService : IAchievementService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AchievementService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<List<AchievementDto>> GetAllAsync()
    {
        var achievements = await _db.Achievements.OrderBy(a => a.Id).ToListAsync();
        return achievements.Select(MapToDto).ToList();
    }

    public async Task<List<UserAchievementDto>> GetUnlockedAsync(string userId)
    {
        var unlocked = await _db.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.UnlockedAt)
            .ToListAsync();

        return unlocked.Select(ua => new UserAchievementDto
        {
            Achievement = MapToDto(ua.Achievement),
            UnlockedAt = ua.UnlockedAt,
            IsNew = (DateTime.UtcNow - ua.UnlockedAt).TotalDays < 1
        }).ToList();
    }

    public async Task<List<UserAchievementDto>> CheckAndUnlockAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var alreadyUnlockedList = await _db.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AchievementId)
            .ToListAsync();
        var alreadyUnlocked = alreadyUnlockedList.ToHashSet();

        var allAchievements = await _db.Achievements.ToListAsync();
        var newlyUnlocked = new List<UserAchievementDto>();

        // Gather user stats for condition checking
        var actionCount = await _db.CarbonActions.CountAsync(a => a.UserId == userId);
        var habitCheckinCount = await _db.HabitCheckins.CountAsync(c => c.UserId == userId);
        var challengeCompletionCount = await _db.ChallengeParticipants
            .CountAsync(cp => cp.UserId == userId && cp.CompletedAt != null);
        var friendCount = await _db.Friends
            .CountAsync(f => (f.RequesterId == userId || f.AddresseeId == userId)
                          && f.Status == FriendRequestStatus.Accepted);
        var contentReadCount = await _db.UserContentReads.CountAsync(r => r.UserId == userId);
        var categoryActionCounts = await _db.CarbonActions
            .Where(a => a.UserId == userId)
            .GroupBy(a => a.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Category, g => g.Count);

        foreach (var achievement in allAchievements)
        {
            if (alreadyUnlocked.Contains(achievement.Id))
                continue;

            bool met = EvaluateCondition(achievement, user, actionCount, habitCheckinCount,
                challengeCompletionCount, friendCount, contentReadCount, categoryActionCounts);

            if (met)
            {
                var ua = new UserAchievement
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AchievementId = achievement.Id,
                    UnlockedAt = DateTime.UtcNow
                };
                _db.UserAchievements.Add(ua);

                _db.FeedEvents.Add(new FeedEvent
                {
                    UserId = userId,
                    EventType = FeedEventType.MilestoneAchievement,
                    Title = "Achievement Unlocked!",
                    Description = $"Earned \"{achievement.Title}\"!",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.FeedExpiryDays)
                });

                newlyUnlocked.Add(new UserAchievementDto
                {
                    Achievement = MapToDto(achievement),
                    UnlockedAt = ua.UnlockedAt,
                    IsNew = true
                });
            }
        }

        if (newlyUnlocked.Count > 0)
            await _db.SaveChangesAsync();

        return newlyUnlocked;
    }

    public async Task SeedAchievementsAsync()
    {
        var existingKeysList = await _db.Achievements.Select(a => a.Key).ToListAsync();
        var existingKeys = existingKeysList.ToHashSet();

        var definitions = GetAchievementDefinitions();
        var toAdd = definitions.Where(d => !existingKeys.Contains(d.Key)).ToList();

        if (toAdd.Count > 0)
        {
            _db.Achievements.AddRange(toAdd);
            await _db.SaveChangesAsync();
        }
    }

    private static bool EvaluateCondition(
        Achievement achievement,
        ApplicationUser user,
        int actionCount,
        int habitCheckinCount,
        int challengeCompletionCount,
        int friendCount,
        int contentReadCount,
        Dictionary<ActionCategory, int> categoryActionCounts)
    {
        var category = achievement.Category.ToLowerInvariant();
        int threshold = achievement.Threshold;

        return category switch
        {
            "actions" => actionCount >= threshold,
            "actions_transport" => categoryActionCounts.GetValueOrDefault(ActionCategory.Transport) >= threshold,
            "actions_food" => categoryActionCounts.GetValueOrDefault(ActionCategory.Food) >= threshold,
            "actions_energy" => categoryActionCounts.GetValueOrDefault(ActionCategory.Energy) >= threshold,
            "actions_shopping" => categoryActionCounts.GetValueOrDefault(ActionCategory.Shopping) >= threshold,
            "actions_waste" => categoryActionCounts.GetValueOrDefault(ActionCategory.Waste) >= threshold,
            "actions_travel" => categoryActionCounts.GetValueOrDefault(ActionCategory.Travel) >= threshold,
            "habits" => habitCheckinCount >= threshold,
            "challenges" => challengeCompletionCount >= threshold,
            "friends" => friendCount >= threshold,
            "content" => contentReadCount >= threshold,
            "streak" => user.StreakBest >= threshold,
            "lp" => user.TotalLeafPoints >= threshold,
            "co2e" => (double)user.TotalCO2eAvoided >= threshold,
            "tree_stage" => user.TreeStage >= threshold,
            _ => false
        };
    }

    private static AchievementDto MapToDto(Achievement a) => new()
    {
        Id = IntToGuid(a.Id),
        Name = a.Title,
        Description = a.Description,
        IconUrl = a.IconName,
        LPReward = a.LeafPointsReward,
        Category = a.Category
    };

    private static Guid IntToGuid(int id)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(id).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private static List<Achievement> GetAchievementDefinitions() =>
    [
        // Actions milestones
        new() { Key = "first_action", Title = "First Step", Description = "Log your first carbon action", IconName = "seedling", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions", Threshold = 1 },
        new() { Key = "actions_10", Title = "Getting Started", Description = "Log 10 carbon actions", IconName = "leaf", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions", Threshold = 10 },
        new() { Key = "actions_25", Title = "Eco Warrior", Description = "Log 25 carbon actions", IconName = "tree", LeafPointsReward = LPAwards.AchievementSilver, Category = "actions", Threshold = 25 },
        new() { Key = "actions_50", Title = "Green Champion", Description = "Log 50 carbon actions", IconName = "trophy", LeafPointsReward = LPAwards.AchievementSilver, Category = "actions", Threshold = 50 },
        new() { Key = "actions_100", Title = "Century Mark", Description = "Log 100 carbon actions", IconName = "star", LeafPointsReward = LPAwards.AchievementGold, Category = "actions", Threshold = 100 },
        new() { Key = "actions_250", Title = "Dedicated Ecologist", Description = "Log 250 carbon actions", IconName = "globe", LeafPointsReward = LPAwards.AchievementGold, Category = "actions", Threshold = 250 },
        new() { Key = "actions_500", Title = "Planet Protector", Description = "Log 500 carbon actions", IconName = "shield", LeafPointsReward = LPAwards.AchievementPlatinum, Category = "actions", Threshold = 500 },

        // Category-specific
        new() { Key = "transport_10", Title = "Green Commuter", Description = "Log 10 transport actions", IconName = "bike", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions_transport", Threshold = 10 },
        new() { Key = "transport_50", Title = "Road Less Driven", Description = "Log 50 transport actions", IconName = "bus", LeafPointsReward = LPAwards.AchievementSilver, Category = "actions_transport", Threshold = 50 },
        new() { Key = "food_10", Title = "Mindful Eater", Description = "Log 10 food actions", IconName = "apple", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions_food", Threshold = 10 },
        new() { Key = "food_50", Title = "Sustainable Chef", Description = "Log 50 food actions", IconName = "carrot", LeafPointsReward = LPAwards.AchievementSilver, Category = "actions_food", Threshold = 50 },
        new() { Key = "energy_10", Title = "Power Saver", Description = "Log 10 energy actions", IconName = "bolt", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions_energy", Threshold = 10 },
        new() { Key = "energy_50", Title = "Energy Guardian", Description = "Log 50 energy actions", IconName = "sun", LeafPointsReward = LPAwards.AchievementSilver, Category = "actions_energy", Threshold = 50 },
        new() { Key = "shopping_10", Title = "Conscious Consumer", Description = "Log 10 shopping actions", IconName = "bag", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions_shopping", Threshold = 10 },
        new() { Key = "shopping_50", Title = "Zero Waste Shopper", Description = "Log 50 shopping actions", IconName = "recycle", LeafPointsReward = LPAwards.AchievementSilver, Category = "actions_shopping", Threshold = 50 },
        new() { Key = "waste_10", Title = "Waste Reducer", Description = "Log 10 waste actions", IconName = "trash", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions_waste", Threshold = 10 },
        new() { Key = "waste_50", Title = "Recycling Hero", Description = "Log 50 waste actions", IconName = "recycle2", LeafPointsReward = LPAwards.AchievementSilver, Category = "actions_waste", Threshold = 50 },
        new() { Key = "travel_10", Title = "Eco Traveler", Description = "Log 10 travel actions", IconName = "plane", LeafPointsReward = LPAwards.AchievementBronze, Category = "actions_travel", Threshold = 10 },

        // Streaks
        new() { Key = "streak_3", Title = "On a Roll", Description = "Reach a 3-day streak", IconName = "fire", LeafPointsReward = LPAwards.AchievementBronze, Category = "streak", Threshold = 3 },
        new() { Key = "streak_7", Title = "Week Warrior", Description = "Reach a 7-day streak", IconName = "fire2", LeafPointsReward = LPAwards.AchievementBronze, Category = "streak", Threshold = 7 },
        new() { Key = "streak_14", Title = "Fortnight Force", Description = "Reach a 14-day streak", IconName = "fire3", LeafPointsReward = LPAwards.AchievementSilver, Category = "streak", Threshold = 14 },
        new() { Key = "streak_30", Title = "Monthly Master", Description = "Reach a 30-day streak", IconName = "calendar", LeafPointsReward = LPAwards.AchievementSilver, Category = "streak", Threshold = 30 },
        new() { Key = "streak_60", Title = "Two Month Titan", Description = "Reach a 60-day streak", IconName = "medal", LeafPointsReward = LPAwards.AchievementGold, Category = "streak", Threshold = 60 },
        new() { Key = "streak_100", Title = "Centurion", Description = "Reach a 100-day streak", IconName = "crown", LeafPointsReward = LPAwards.AchievementGold, Category = "streak", Threshold = 100 },
        new() { Key = "streak_365", Title = "Year of Green", Description = "Reach a 365-day streak", IconName = "diamond", LeafPointsReward = LPAwards.AchievementPlatinum, Category = "streak", Threshold = 365 },

        // Habits
        new() { Key = "habit_first", Title = "Habit Formed", Description = "Complete your first habit check-in", IconName = "check", LeafPointsReward = LPAwards.AchievementBronze, Category = "habits", Threshold = 1 },
        new() { Key = "habits_25", Title = "Habitual", Description = "Complete 25 habit check-ins", IconName = "repeat", LeafPointsReward = LPAwards.AchievementBronze, Category = "habits", Threshold = 25 },
        new() { Key = "habits_50", Title = "Habit Master", Description = "Complete 50 habit check-ins", IconName = "brain", LeafPointsReward = LPAwards.AchievementSilver, Category = "habits", Threshold = 50 },
        new() { Key = "habits_100", Title = "Creature of Habit", Description = "Complete 100 habit check-ins", IconName = "dna", LeafPointsReward = LPAwards.AchievementGold, Category = "habits", Threshold = 100 },
        new() { Key = "habits_250", Title = "Lifestyle Change", Description = "Complete 250 habit check-ins", IconName = "butterfly", LeafPointsReward = LPAwards.AchievementPlatinum, Category = "habits", Threshold = 250 },

        // Challenges
        new() { Key = "challenge_first", Title = "Challenger", Description = "Complete your first challenge", IconName = "flag", LeafPointsReward = LPAwards.AchievementBronze, Category = "challenges", Threshold = 1 },
        new() { Key = "challenges_5", Title = "Challenge Accepted", Description = "Complete 5 challenges", IconName = "target", LeafPointsReward = LPAwards.AchievementSilver, Category = "challenges", Threshold = 5 },
        new() { Key = "challenges_10", Title = "Challenge Champion", Description = "Complete 10 challenges", IconName = "trophy2", LeafPointsReward = LPAwards.AchievementGold, Category = "challenges", Threshold = 10 },
        new() { Key = "challenges_25", Title = "Unstoppable", Description = "Complete 25 challenges", IconName = "rocket", LeafPointsReward = LPAwards.AchievementPlatinum, Category = "challenges", Threshold = 25 },

        // Friends
        new() { Key = "friend_first", Title = "Social Seed", Description = "Make your first friend", IconName = "handshake", LeafPointsReward = LPAwards.AchievementBronze, Category = "friends", Threshold = 1 },
        new() { Key = "friends_5", Title = "Growing Circle", Description = "Have 5 friends", IconName = "people", LeafPointsReward = LPAwards.AchievementBronze, Category = "friends", Threshold = 5 },
        new() { Key = "friends_10", Title = "Popular Plant", Description = "Have 10 friends", IconName = "community", LeafPointsReward = LPAwards.AchievementSilver, Category = "friends", Threshold = 10 },
        new() { Key = "friends_25", Title = "Social Forest", Description = "Have 25 friends", IconName = "forest", LeafPointsReward = LPAwards.AchievementGold, Category = "friends", Threshold = 25 },

        // Content / Learning
        new() { Key = "content_first", Title = "Curious Mind", Description = "Read your first article", IconName = "book", LeafPointsReward = LPAwards.AchievementBronze, Category = "content", Threshold = 1 },
        new() { Key = "content_10", Title = "Knowledge Seeker", Description = "Read 10 articles", IconName = "glasses", LeafPointsReward = LPAwards.AchievementBronze, Category = "content", Threshold = 10 },
        new() { Key = "content_25", Title = "Climate Scholar", Description = "Read 25 articles", IconName = "graduation", LeafPointsReward = LPAwards.AchievementSilver, Category = "content", Threshold = 25 },

        // LP milestones
        new() { Key = "lp_100", Title = "Leaf Collector", Description = "Earn 100 Leaf Points", IconName = "leaf2", LeafPointsReward = LPAwards.AchievementBronze, Category = "lp", Threshold = 100 },
        new() { Key = "lp_500", Title = "Leaf Hoarder", Description = "Earn 500 Leaf Points", IconName = "leaves", LeafPointsReward = LPAwards.AchievementSilver, Category = "lp", Threshold = 500 },
        new() { Key = "lp_1000", Title = "Thousand Leaves", Description = "Earn 1,000 Leaf Points", IconName = "tree2", LeafPointsReward = LPAwards.AchievementGold, Category = "lp", Threshold = 1000 },
        new() { Key = "lp_5000", Title = "Leaf Legend", Description = "Earn 5,000 Leaf Points", IconName = "crown2", LeafPointsReward = LPAwards.AchievementPlatinum, Category = "lp", Threshold = 5000 },

        // CO2e saved
        new() { Key = "co2e_10", Title = "Carbon Cutter", Description = "Avoid 10 kg CO2e", IconName = "cloud", LeafPointsReward = LPAwards.AchievementBronze, Category = "co2e", Threshold = 10 },
        new() { Key = "co2e_50", Title = "Emission Eliminator", Description = "Avoid 50 kg CO2e", IconName = "wind", LeafPointsReward = LPAwards.AchievementSilver, Category = "co2e", Threshold = 50 },
        new() { Key = "co2e_100", Title = "Carbon Crusher", Description = "Avoid 100 kg CO2e", IconName = "lightning", LeafPointsReward = LPAwards.AchievementGold, Category = "co2e", Threshold = 100 },
        new() { Key = "co2e_500", Title = "Climate Champion", Description = "Avoid 500 kg CO2e", IconName = "earth", LeafPointsReward = LPAwards.AchievementPlatinum, Category = "co2e", Threshold = 500 },

        // Tree stages
        new() { Key = "tree_sprout", Title = "First Sprout", Description = "Grow your tree to Sprout stage", IconName = "sprout", LeafPointsReward = LPAwards.AchievementBronze, Category = "tree_stage", Threshold = 1 },
        new() { Key = "tree_sapling", Title = "Sapling", Description = "Grow your tree to Sapling stage", IconName = "sapling", LeafPointsReward = LPAwards.AchievementBronze, Category = "tree_stage", Threshold = 2 },
        new() { Key = "tree_young", Title = "Young Tree", Description = "Grow your tree to Young Tree stage", IconName = "tree3", LeafPointsReward = LPAwards.AchievementSilver, Category = "tree_stage", Threshold = 3 },
        new() { Key = "tree_mature", Title = "Mature Tree", Description = "Grow your tree to Mature Tree stage", IconName = "tree4", LeafPointsReward = LPAwards.AchievementGold, Category = "tree_stage", Threshold = 5 },
        new() { Key = "tree_elder", Title = "Elder Tree", Description = "Grow your tree to Elder stage", IconName = "elder", LeafPointsReward = LPAwards.AchievementPlatinum, Category = "tree_stage", Threshold = 11 },
    ];
}
