using System.Globalization;
using System.Text;
using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public UserService(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapProfile(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (dto.DisplayName != null) user.DisplayName = dto.DisplayName;
        if (dto.AvatarIndex.HasValue) user.AvatarIndex = dto.AvatarIndex.Value;
        if (dto.RegionContinent != null) user.Region = dto.RegionContinent;
        if (dto.RegionCountry != null) user.Region = $"{dto.RegionContinent ?? user.Region?.Split('/').FirstOrDefault()}/{dto.RegionCountry}";

        await _userManager.UpdateAsync(user);
        return MapProfile(user);
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        // GDPR hard delete: cascading via DB config handles most relations.
        // Explicitly remove Friend records since they use Restrict delete.
        var friendRecords = await _db.Friends
            .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
            .ToListAsync();
        _db.Friends.RemoveRange(friendRecords);

        var feedReactions = await _db.FeedReactions
            .Where(r => r.UserId == userId)
            .ToListAsync();
        _db.FeedReactions.RemoveRange(feedReactions);

        await _db.SaveChangesAsync();
        await _userManager.DeleteAsync(user);
    }

    public async Task<List<UserSearchResultDto>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        var normalizedQuery = query.ToUpperInvariant();
        var users = await _db.Users
            .Where(u => u.NormalizedUserName!.Contains(normalizedQuery)
                     || u.DisplayName.ToUpper().Contains(normalizedQuery))
            .Take(20)
            .ToListAsync();

        return users.Select(u => new UserSearchResultDto
        {
            Id = Guid.Parse(u.Id),
            DisplayName = u.DisplayName,
            AvatarIndex = u.AvatarIndex,
            TotalLP = u.TotalLeafPoints
        }).ToList();
    }

    public async Task<UserProfileDto> SetupUserAsync(string userId, UserSetupDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.DisplayName = dto.DisplayName;
        user.AvatarIndex = dto.AvatarIndex;

        if (dto.RegionContinent != null)
            user.Region = dto.RegionCountry != null
                ? $"{dto.RegionContinent}/{dto.RegionCountry}"
                : dto.RegionContinent;

        // Process baseline quiz answers
        if (dto.BaselineQuizAnswers.TryGetValue("diet", out var diet))
            user.DietType = diet;
        if (dto.BaselineQuizAnswers.TryGetValue("transport", out var transport))
            user.TransportMode = transport;
        if (dto.BaselineQuizAnswers.TryGetValue("energy", out var energy))
            user.EnergySource = energy;
        if (dto.BaselineQuizAnswers.TryGetValue("flights", out var flights))
            user.FlightFrequency = flights;
        if (dto.BaselineQuizAnswers.TryGetValue("shopping", out var shopping))
            user.ShoppingHabits = shopping;

        user.CarbonBaselineKg = CalculateBaseline(user);

        await _userManager.UpdateAsync(user);
        return MapProfile(user);
    }

    public async Task<byte[]> ExportUserDataAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var sb = new StringBuilder();

        // Profile section
        sb.AppendLine("=== PROFILE ===");
        sb.AppendLine("Field,Value");
        sb.AppendLine($"DisplayName,\"{Escape(user.DisplayName)}\"");
        sb.AppendLine($"Email,\"{Escape(user.Email ?? "")}\"");
        sb.AppendLine($"Region,\"{Escape(user.Region ?? "")}\"");
        sb.AppendLine($"JoinedAt,{user.JoinedAt:O}");
        sb.AppendLine($"TotalLeafPoints,{user.TotalLeafPoints}");
        sb.AppendLine($"TotalCO2eAvoided,{user.TotalCO2eAvoided}");
        sb.AppendLine($"TreeSpecies,{user.TreeSpecies}");
        sb.AppendLine($"TreeStage,{user.TreeStage}");
        sb.AppendLine($"StreakCurrent,{user.StreakCurrent}");
        sb.AppendLine($"StreakBest,{user.StreakBest}");
        sb.AppendLine();

        // Actions
        var actions = await _db.CarbonActions.Where(a => a.UserId == userId).OrderBy(a => a.LoggedAt).ToListAsync();
        sb.AppendLine("=== ACTIONS ===");
        sb.AppendLine("Id,Category,ActionTemplateId,CO2eSavedKg,LeafPointsAwarded,IsDetailed,LoggedAt");
        foreach (var a in actions)
            sb.AppendLine($"{a.Id},{a.Category},\"{Escape(a.ActionTemplateId)}\",{a.CO2eSavedKg},{a.LeafPointsAwarded},{a.IsDetailed},{a.LoggedAt:O}");
        sb.AppendLine();

        // Habits
        var habits = await _db.Habits.Where(h => h.UserId == userId).ToListAsync();
        sb.AppendLine("=== HABITS ===");
        sb.AppendLine("Id,Name,Category,Frequency,CurrentStreak,BestStreak,CreatedAt");
        foreach (var h in habits)
            sb.AppendLine($"{h.Id},\"{Escape(h.Name)}\",{h.Category},{h.Frequency},{h.CurrentStreak},{h.BestStreak},{h.CreatedAt:O}");
        sb.AppendLine();

        // Checkins
        var checkins = await _db.HabitCheckins.Where(c => c.UserId == userId).OrderBy(c => c.CheckedInAt).ToListAsync();
        sb.AppendLine("=== HABIT CHECKINS ===");
        sb.AppendLine("Id,HabitId,CheckedInAt");
        foreach (var c in checkins)
            sb.AppendLine($"{c.Id},{c.HabitId},{c.CheckedInAt:O}");
        sb.AppendLine();

        // Achievements
        var achievements = await _db.UserAchievements.Include(ua => ua.Achievement).Where(ua => ua.UserId == userId).ToListAsync();
        sb.AppendLine("=== ACHIEVEMENTS ===");
        sb.AppendLine("AchievementKey,Title,UnlockedAt");
        foreach (var ua in achievements)
            sb.AppendLine($"\"{Escape(ua.Achievement.Key)}\",\"{Escape(ua.Achievement.Title)}\",{ua.UnlockedAt:O}");
        sb.AppendLine();

        // Offset history
        var offsets = await _db.OffsetTransactions.Where(o => o.UserId == userId).OrderBy(o => o.RedeemedAt).ToListAsync();
        sb.AppendLine("=== OFFSET TRANSACTIONS ===");
        sb.AppendLine("Id,Tier,CreditsSpent,Description,RedeemedAt");
        foreach (var o in offsets)
            sb.AppendLine($"{o.Id},{o.Tier},{o.CreditsSpent},\"{Escape(o.Description)}\",{o.RedeemedAt:O}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static UserProfileDto MapProfile(ApplicationUser user) => new()
    {
        Id = Guid.Parse(user.Id),
        DisplayName = user.DisplayName,
        Email = user.Email,
        AvatarIndex = user.AvatarIndex,
        RegionContinent = user.Region?.Split('/').FirstOrDefault(),
        RegionCountry = user.Region?.Contains('/') == true ? user.Region.Split('/').LastOrDefault() : null,
        CurrentStreak = user.StreakCurrent,
        LongestStreak = user.StreakBest,
        TotalLP = user.TotalLeafPoints,
        JoinedAt = user.JoinedAt
    };

    private static decimal CalculateBaseline(ApplicationUser user)
    {
        decimal baseline = 4000; // Average global baseline ~4 tonnes CO2e/year in kg
        baseline += user.DietType switch
        {
            "vegan" => -800,
            "vegetarian" => -500,
            "pescatarian" => -300,
            "meat_light" => -100,
            _ => 0
        };
        baseline += user.TransportMode switch
        {
            "bike" or "walk" => -1200,
            "public_transit" => -600,
            "electric_car" => -400,
            "carpool" => -300,
            _ => 0
        };
        baseline += user.EnergySource switch
        {
            "renewable" => -800,
            "mixed" => -300,
            _ => 0
        };
        baseline += user.FlightFrequency switch
        {
            "never" => -500,
            "rarely" => -200,
            "often" => 500,
            "frequent" => 1200,
            _ => 0
        };
        return Math.Max(baseline, 500);
    }

    private static string Escape(string value) => value.Replace("\"", "\"\"");
}
