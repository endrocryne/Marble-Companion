using MarbleCompanion.API.Data;
using MarbleCompanion.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Jobs;

public class StreakCheckJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<StreakCheckJob> _logger;

    public StreakCheckJob(AppDbContext db, ILogger<StreakCheckJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting midnight streak check");

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var freezeInterval = TimeSpan.FromDays(AppConstants.StreakFreezeIntervalDays);

        var usersWithStreaks = await _db.Users
            .Where(u => u.StreakCurrent > 0)
            .ToListAsync();

        var frozenCount = 0;
        var resetCount = 0;

        foreach (var user in usersWithStreaks)
        {
            // If the user acted yesterday, their streak is already maintained
            if (user.LastActionDate?.Date == yesterday)
                continue;

            // If the user acted today, streak is fine
            if (user.LastActionDate?.Date == DateTime.UtcNow.Date)
                continue;

            // User missed yesterday - check for streak freeze
            if (user.StreakFreezeAvailable &&
                (user.StreakFreezeLastUsed == null ||
                 DateTime.UtcNow - user.StreakFreezeLastUsed.Value > freezeInterval))
            {
                // Use streak freeze
                user.StreakFreezeAvailable = false;
                user.StreakFreezeLastUsed = DateTime.UtcNow;
                frozenCount++;
            }
            else
            {
                // Reset streak
                user.StreakCurrent = 0;
                resetCount++;
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Streak check complete. Frozen: {FrozenCount}, Reset: {ResetCount}",
            frozenCount, resetCount);
    }
}
