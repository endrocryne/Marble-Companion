using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Shared.Constants;

public static class TreeGrowthConstants
{
    /// <summary>
    /// LP thresholds for each growth stage (index = stage number).
    /// </summary>
    public static readonly int[] LPThresholds =
        [0, 50, 150, 350, 700, 1200, 2000, 3200, 5000, 7500, 11000, 16000];

    /// <summary>
    /// Stage thresholds (LP required to reach each stage, index = stage number).
    /// Alias for <see cref="LPThresholds"/>.
    /// </summary>
    public static readonly int[] StageThresholds = LPThresholds;

    /// <summary>
    /// Display names for each growth stage (index = stage number).
    /// </summary>
    public static readonly string[] StageNames =
        ["Seed", "Sprout", "Sapling", "Young Tree", "Growing Tree", "Mature Tree",
         "Full Tree", "Ancient-1", "Ancient-2", "Ancient-3", "Ancient-4", "Elder"];

    public const int MaxHealth = 100;
    public const int InactivityGraceDays = 3;
    public const int DailyHealthDecay = 10;
    public const int ActionHealthRecovery = 15;

    public const int HealthyMin = 80;
    public const int StressedMin = 50;
    public const int WitheringMin = 20;

    /// <summary>
    /// Returns the 0-based stage index for the given total LP.
    /// </summary>
    public static int GetStageForLP(int totalLP)
    {
        int stage = 0;
        for (int i = LPThresholds.Length - 1; i >= 0; i--)
        {
            if (totalLP >= LPThresholds[i])
            {
                stage = i;
                break;
            }
        }
        return stage;
    }

    /// <summary>
    /// Returns the health state for the given health score (0-100).
    /// </summary>
    public static TreeHealthState GetHealthState(int healthScore)
    {
        if (healthScore >= HealthyMin) return TreeHealthState.Healthy;
        if (healthScore >= StressedMin) return TreeHealthState.Stressed;
        if (healthScore >= WitheringMin) return TreeHealthState.Withering;
        return TreeHealthState.Dormant;
    }
}
