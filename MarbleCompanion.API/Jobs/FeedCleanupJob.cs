using MarbleCompanion.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Jobs;

public class FeedCleanupJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<FeedCleanupJob> _logger;

    public FeedCleanupJob(AppDbContext db, ILogger<FeedCleanupJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting feed cleanup job");

        var expiredEvents = await _db.FeedEvents
            .Where(e => e.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expiredEvents.Count > 0)
        {
            _db.FeedEvents.RemoveRange(expiredEvents);
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Feed cleanup complete. Removed {Count} expired events", expiredEvents.Count);
    }
}
