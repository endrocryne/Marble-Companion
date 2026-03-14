namespace MarbleCompanion.Shared.Constants;

public static class LPAwards
{
    // Action logging
    public const int QuickLog = 5;
    public const int DetailedLog = 10;

    // Habits
    public const int HabitCheckin = 8;

    // Content
    public const int ArticleRead = 3;

    // Achievements (base values by rarity)
    public const int AchievementBronze = 25;
    public const int AchievementSilver = 50;
    public const int AchievementGold = 100;
    public const int AchievementPlatinum = 200;

    // Streak milestones
    public const int Streak3Days = 50;
    public const int Streak7Days = 100;
    public const int Streak14Days = 150;
    public const int Streak30Days = 250;
    public const int Streak60Days = 350;
    public const int Streak100Days = 500;
    public const int Streak365Days = 500;

    // Challenge completion (range 25-200 based on difficulty)
    public const int ChallengeCompletionMin = 25;
    public const int ChallengeCompletionMax = 200;
    public const int ChallengeEasy = 25;
    public const int ChallengeMedium = 75;
    public const int ChallengeHard = 200;

    /// <summary>
    /// Returns the LP award for reaching the given streak day count, or 0 if not a milestone.
    /// </summary>
    public static int GetStreakMilestoneLP(int streakDays) => streakDays switch
    {
        3 => Streak3Days,
        7 => Streak7Days,
        14 => Streak14Days,
        30 => Streak30Days,
        60 => Streak60Days,
        100 => Streak100Days,
        365 => Streak365Days,
        _ => 0
    };
}
