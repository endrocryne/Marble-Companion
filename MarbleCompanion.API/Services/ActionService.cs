using System.Text.Json;
using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class ActionService : IActionService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITreeService _treeService;
    private readonly IAchievementService _achievementService;

    public ActionService(
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

    public async Task<LogActionResponse> LogActionAsync(string userId, LogActionRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var templateKey = request.ActionTemplateId.ToString();
        var emissionFactor = await _db.EmissionFactors
            .FirstOrDefaultAsync(ef => ef.ActionKey == templateKey && ef.Category == request.Category.ToString() && ef.IsActive);

        decimal co2eSaved = emissionFactor?.FactorKgCO2ePerUnit ?? 0.5m;
        int lpAwarded = request.IsDetailed ? LPAwards.DetailedLog : LPAwards.QuickLog;

        string? detailedJson = request.DetailedData != null
            ? JsonSerializer.Serialize(request.DetailedData)
            : null;

        var action = new CarbonAction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = request.Category,
            ActionTemplateId = templateKey,
            IsDetailed = request.IsDetailed,
            CO2eSavedKg = co2eSaved,
            LeafPointsAwarded = lpAwarded,
            DetailedDataJson = detailedJson,
            LoggedAt = DateTime.UtcNow,
            EmissionFactorId = emissionFactor?.Id
        };

        _db.CarbonActions.Add(action);

        // Update user totals
        user.TotalCO2eAvoided += co2eSaved;

        // Update streak
        var today = DateTime.UtcNow.Date;
        if (user.LastActionDate == null || user.LastActionDate.Value.Date < today)
        {
            user.StreakCurrent = StreakHelper.CalculateNewStreak(user.StreakCurrent, user.LastActionDate, today);
            user.StreakBest = Math.Max(user.StreakBest, user.StreakCurrent);
        }
        user.LastActionDate = DateTime.UtcNow;

        // Recover tree health on action
        user.TreeHealthScore = Math.Min(TreeGrowthConstants.MaxHealth,
            user.TreeHealthScore + TreeGrowthConstants.ActionHealthRecovery);

        await _userManager.UpdateAsync(user);
        await _db.SaveChangesAsync();

        // Award LP (also handles tree stage advancement)
        await _treeService.AwardLeafPointsAsync(userId, lpAwarded);

        // Check achievements
        await _achievementService.CheckAndUnlockAsync(userId);

        return new LogActionResponse
        {
            Id = action.Id,
            LPAwarded = lpAwarded,
            CO2eSaved = (double)co2eSaved
        };
    }

    public async Task<List<CarbonActionDto>> GetActionsAsync(
        string userId, DateTime? from, DateTime? to, ActionCategory? category)
    {
        var query = _db.CarbonActions.Where(a => a.UserId == userId);

        if (from.HasValue) query = query.Where(a => a.LoggedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.LoggedAt <= to.Value);
        if (category.HasValue) query = query.Where(a => a.Category == category.Value);

        var actions = await query.OrderByDescending(a => a.LoggedAt).ToListAsync();

        return actions.Select(a => new CarbonActionDto
        {
            Id = a.Id,
            Category = a.Category,
            TemplateName = a.ActionTemplateId,
            CO2eSaved = (double)a.CO2eSavedKg,
            LPAwarded = a.LeafPointsAwarded,
            LoggedAt = a.LoggedAt,
            IsDetailed = a.IsDetailed
        }).ToList();
    }

    public async Task DeleteActionAsync(string userId, Guid actionId)
    {
        var action = await _db.CarbonActions.FirstOrDefaultAsync(a => a.Id == actionId && a.UserId == userId)
            ?? throw new KeyNotFoundException("Action not found.");

        _db.CarbonActions.Remove(action);
        await _db.SaveChangesAsync();
    }

    public async Task<ActionSummaryDto> GetSummaryAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var now = DateTime.UtcNow;
        var actions = await _db.CarbonActions
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var categoryTotals = new Dictionary<ActionCategory, CategoryTotalDto>();
        foreach (ActionCategory cat in Enum.GetValues<ActionCategory>())
        {
            var catActions = actions.Where(a => a.Category == cat).ToList();
            categoryTotals[cat] = new CategoryTotalDto
            {
                ActionCount = catActions.Count,
                LPEarned = catActions.Sum(a => a.LeafPointsAwarded),
                CO2eSaved = (double)catActions.Sum(a => a.CO2eSavedKg)
            };
        }

        var firstAction = actions.MinBy(a => a.LoggedAt);
        var periodStart = firstAction?.LoggedAt ?? user.JoinedAt;

        return new ActionSummaryDto
        {
            PeriodStart = periodStart,
            PeriodEnd = now,
            TotalLP = user.TotalLeafPoints,
            TotalCO2eSaved = (double)user.TotalCO2eAvoided,
            TotalsByCategory = categoryTotals
        };
    }
}
