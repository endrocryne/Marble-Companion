using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class TreeService : ITreeService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public TreeService(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<TreeDto> GetTreeAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var stage = TreeGrowthConstants.GetStageForLP(user.TotalLeafPoints);
        var healthState = TreeGrowthConstants.GetHealthState(user.TreeHealthScore);

        int nextMilestoneLP = stage < TreeGrowthConstants.LPThresholds.Length - 1
            ? TreeGrowthConstants.LPThresholds[stage + 1]
            : TreeGrowthConstants.LPThresholds[^1];

        var cosmetics = new List<string>();
        if (user.ActiveSkyTheme != null) cosmetics.Add(user.ActiveSkyTheme);
        if (user.ActiveGroundTheme != null) cosmetics.Add(user.ActiveGroundTheme);
        if (user.ActiveCompanionCreature != null) cosmetics.Add(user.ActiveCompanionCreature);

        return new TreeDto
        {
            Species = user.TreeSpecies,
            Stage = stage,
            StageName = TreeGrowthConstants.StageNames[stage],
            HealthScore = user.TreeHealthScore,
            HealthState = healthState,
            TotalLP = user.TotalLeafPoints,
            TotalCO2eAvoided = (double)user.TotalCO2eAvoided,
            NextMilestoneLP = nextMilestoneLP,
            ActiveCosmetics = cosmetics
        };
    }

    public async Task UpdateHealthAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.LastActionDate == null)
        {
            // No actions ever logged — apply decay
            user.TreeHealthScore = Math.Max(0,
                user.TreeHealthScore - TreeGrowthConstants.DailyHealthDecay);
            await _userManager.UpdateAsync(user);
            return;
        }

        var daysSinceLastAction = (int)(DateTime.UtcNow - user.LastActionDate.Value).TotalDays;

        if (daysSinceLastAction > TreeGrowthConstants.InactivityGraceDays)
        {
            int decayDays = daysSinceLastAction - TreeGrowthConstants.InactivityGraceDays;
            int decay = decayDays * TreeGrowthConstants.DailyHealthDecay;
            user.TreeHealthScore = Math.Max(0, user.TreeHealthScore - decay);
        }
        else
        {
            // Within grace period — recover
            user.TreeHealthScore = Math.Min(TreeGrowthConstants.MaxHealth,
                user.TreeHealthScore + TreeGrowthConstants.ActionHealthRecovery);
        }

        await _userManager.UpdateAsync(user);
    }

    public async Task AwardLeafPointsAsync(string userId, int points)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        int oldStage = TreeGrowthConstants.GetStageForLP(user.TotalLeafPoints);
        user.TotalLeafPoints += points;
        int newStage = TreeGrowthConstants.GetStageForLP(user.TotalLeafPoints);

        user.TreeStage = newStage;

        if (newStage > oldStage)
        {
            _db.FeedEvents.Add(new FeedEvent
            {
                UserId = userId,
                EventType = FeedEventType.TreeAdvancement,
                Title = "Tree Advanced!",
                Description = $"{user.DisplayName}'s tree grew to {TreeGrowthConstants.StageNames[newStage]}!",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.FeedExpiryDays)
            });
        }

        await _userManager.UpdateAsync(user);
        await _db.SaveChangesAsync();
    }
}
