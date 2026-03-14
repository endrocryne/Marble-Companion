namespace MarbleCompanion.API.Services;

/// <summary>
/// Shared streak calculation logic used by ActionService and HabitService.
/// </summary>
internal static class StreakHelper
{
    /// <summary>
    /// Calculates the updated streak given the last activity date.
    /// Returns the new streak value.
    /// </summary>
    public static int CalculateNewStreak(int currentStreak, DateTime? lastActivityDate, DateTime today)
    {
        if (lastActivityDate == null)
            return 1;

        var lastDate = lastActivityDate.Value.Date;
        if (lastDate == today)
            return currentStreak; // Already counted today

        if (lastDate == today.AddDays(-1))
            return currentStreak + 1; // Consecutive day

        return 1; // Streak broken
    }
}
