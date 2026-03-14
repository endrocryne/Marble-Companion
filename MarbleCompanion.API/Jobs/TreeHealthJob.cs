using MarbleCompanion.API.Data;
using MarbleCompanion.API.Services;
using MarbleCompanion.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Jobs;

public class TreeHealthJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<TreeHealthJob> _logger;

    public TreeHealthJob(AppDbContext db, ILogger<TreeHealthJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting daily tree health update");

        var gracePeriodDays = 3;
        var cutoffDate = DateTime.UtcNow.AddDays(-gracePeriodDays);

        var inactiveUsers = await _db.Users
            .Where(u => u.LastActionDate != null && u.LastActionDate < cutoffDate && u.TreeHealthScore > 0)
            .ToListAsync();

        foreach (var user in inactiveUsers)
        {
            var daysSinceAction = (DateTime.UtcNow - user.LastActionDate!.Value).Days;
            var decayDays = daysSinceAction - gracePeriodDays;
            if (decayDays > 0)
            {
                var decay = decayDays * TreeGrowthConstants.DailyHealthDecay;
                user.TreeHealthScore = Math.Max(0, user.TreeHealthScore - decay);
            }
        }

        // Also decay users who have never logged an action
        var neverActedUsers = await _db.Users
            .Where(u => u.LastActionDate == null && u.TreeHealthScore > 0 && u.JoinedAt < cutoffDate)
            .ToListAsync();

        foreach (var user in neverActedUsers)
        {
            user.TreeHealthScore = Math.Max(0, user.TreeHealthScore - TreeGrowthConstants.DailyHealthDecay);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Tree health updated for {InactiveCount} inactive users and {NeverActedCount} never-acted users",
            inactiveUsers.Count, neverActedUsers.Count);
    }
}
