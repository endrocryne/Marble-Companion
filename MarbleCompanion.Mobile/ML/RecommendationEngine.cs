using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.ML;

public record UserBehaviorProfile(
    Dictionary<ActionCategory, int> ActionCountsByCategory,
    Dictionary<ActionCategory, decimal> CO2eByCategory,
    decimal BaselineKgPerYear,
    int CurrentStreak,
    List<string> ActiveHabitCategories,
    Dictionary<string, string> SuggestionFeedback,
    string Region);

public record RankedSuggestion(SuggestionItem Suggestion, double Score, string Reason);

public sealed class RecommendationEngine
{
    private const double WeightCategoryOverIndex = 30.0;
    private const double WeightFrequencyGap = 25.0;
    private const double WeightEffortMatch = 15.0;
    private const double WeightImpactPriority = 15.0;
    private const double WeightNovelty = 10.0;
    private const double WeightRegional = 5.0;

    // Impact scores normalized to [0, 1] relative to the max (High = 15 pts).
    private const double ImpactScoreHigh = 1.0;        // 15 / 15
    private const double ImpactScoreMedium = 10.0 / 15; // ≈ 0.667
    private const double ImpactScoreLow = 5.0 / 15;    // ≈ 0.333

    private static readonly Dictionary<string, HashSet<ActionCategory>> RegionalCategoryBoosts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["NA"] = new() { ActionCategory.Transport, ActionCategory.Energy, ActionCategory.Shopping },
        ["EU"] = new() { ActionCategory.Transport, ActionCategory.Food, ActionCategory.Energy },
        ["AS"] = new() { ActionCategory.Energy, ActionCategory.Transport, ActionCategory.Waste },
        ["SA"] = new() { ActionCategory.Food, ActionCategory.Transport, ActionCategory.Waste },
        ["AF"] = new() { ActionCategory.Energy, ActionCategory.Food, ActionCategory.Waste },
        ["OC"] = new() { ActionCategory.Transport, ActionCategory.Energy, ActionCategory.Travel },
    };

    public List<RankedSuggestion> GetRecommendations(UserBehaviorProfile profile) =>
        GetRecommendations(profile, 3);

    public List<RankedSuggestion> GetRecommendations(UserBehaviorProfile profile, int count)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var candidates = SuggestionLibrary.All;
        var allCategories = Enum.GetValues<ActionCategory>();
        int categoryCount = allCategories.Length;

        decimal proportionalShare = categoryCount > 0
            ? profile.BaselineKgPerYear / categoryCount
            : 0m;

        int maxActionCount = profile.ActionCountsByCategory.Count > 0
            ? profile.ActionCountsByCategory.Values.Max()
            : 0;

        var activeHabitsSet = new HashSet<string>(
            profile.ActiveHabitCategories ?? [],
            StringComparer.OrdinalIgnoreCase);

        var regionalBoosts = ResolveRegionalBoosts(profile.Region);

        var scored = new List<RankedSuggestion>(candidates.Count);

        foreach (var suggestion in candidates)
        {
            double feedbackMultiplier = GetFeedbackMultiplier(profile.SuggestionFeedback, suggestion.Id);
            if (feedbackMultiplier <= 0.0)
                continue;

            var reasons = new List<string>(4);

            double categoryOverIndex = ScoreCategoryOverIndex(
                suggestion.Category, profile.CO2eByCategory, proportionalShare);
            if (categoryOverIndex > 0.5)
                reasons.Add($"High emissions in {suggestion.Category}");

            double frequencyGap = ScoreFrequencyGap(
                suggestion.Category, profile.ActionCountsByCategory, maxActionCount);
            if (frequencyGap > 0.5)
                reasons.Add($"Few actions logged in {suggestion.Category}");

            double effortMatch = ScoreEffortMatch(suggestion.Effort, profile.CurrentStreak);

            double impactPriority = ScoreImpactPriority(suggestion.Impact);
            if (suggestion.Impact == SuggestionImpact.High)
                reasons.Add("High impact action");

            double novelty = ScoreNovelty(suggestion.Category, activeHabitsSet);
            if (novelty > 0.5)
                reasons.Add("New habit area to explore");

            double regional = ScoreRegionalRelevance(suggestion.Category, regionalBoosts);

            double rawScore =
                categoryOverIndex * WeightCategoryOverIndex +
                frequencyGap * WeightFrequencyGap +
                effortMatch * WeightEffortMatch +
                impactPriority * WeightImpactPriority +
                novelty * WeightNovelty +
                regional * WeightRegional;

            double finalScore = rawScore * feedbackMultiplier;

            string reason = reasons.Count > 0
                ? string.Join("; ", reasons)
                : "Good general recommendation";

            scored.Add(new RankedSuggestion(suggestion, Math.Round(finalScore, 2), reason));
        }

        return scored
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.Suggestion.EstimatedCO2eSavingKgPerYear)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Boosts suggestions in categories where the user's CO2e exceeds
    /// their proportional share of the baseline.
    /// </summary>
    private static double ScoreCategoryOverIndex(
        ActionCategory category,
        Dictionary<ActionCategory, decimal> co2eByCategory,
        decimal proportionalShare)
    {
        if (proportionalShare <= 0m)
            return 0.0;

        decimal actual = co2eByCategory.GetValueOrDefault(category, 0m);
        if (actual <= 0m)
            return 0.0;

        double ratio = (double)(actual / proportionalShare);
        return Math.Clamp(ratio - 1.0, 0.0, 1.0);
    }

    /// <summary>
    /// Categories with fewer logged actions receive a higher score,
    /// directing the user toward under-explored areas.
    /// </summary>
    private static double ScoreFrequencyGap(
        ActionCategory category,
        Dictionary<ActionCategory, int> actionCounts,
        int maxActionCount)
    {
        if (maxActionCount <= 0)
            return 1.0;

        int count = actionCounts.GetValueOrDefault(category, 0);
        return 1.0 - ((double)count / maxActionCount);
    }

    /// <summary>
    /// Matches suggestion effort level to the user's engagement,
    /// measured by their current streak.
    /// </summary>
    private static double ScoreEffortMatch(SuggestionEffort effort, int streak)
    {
        if (streak < 7)
        {
            return effort switch
            {
                SuggestionEffort.Low => 1.0,
                SuggestionEffort.Medium => 0.3,
                SuggestionEffort.High => 0.1,
                _ => 0.5,
            };
        }

        if (streak <= 30)
        {
            return effort switch
            {
                SuggestionEffort.Low => 0.5,
                SuggestionEffort.Medium => 1.0,
                SuggestionEffort.High => 0.3,
                _ => 0.5,
            };
        }

        return effort switch
        {
            SuggestionEffort.Low => 0.3,
            SuggestionEffort.Medium => 0.5,
            SuggestionEffort.High => 1.0,
            _ => 0.5,
        };
    }

    /// <summary>
    /// Higher-impact suggestions receive a proportionally higher score.
    /// </summary>
    private static double ScoreImpactPriority(SuggestionImpact impact)
    {
        return impact switch
        {
            SuggestionImpact.High => ImpactScoreHigh,
            SuggestionImpact.Medium => ImpactScoreMedium,
            SuggestionImpact.Low => ImpactScoreLow,
            _ => 0.5,
        };
    }

    /// <summary>
    /// Suggestions in categories where the user has no active habits
    /// receive a novelty bonus.
    /// </summary>
    private static double ScoreNovelty(
        ActionCategory category,
        HashSet<string> activeHabitsSet)
    {
        string categoryName = category.ToString();
        return activeHabitsSet.Contains(categoryName) ? 0.0 : 1.0;
    }

    /// <summary>
    /// Applies a small bonus for suggestions whose category aligns
    /// with regionally significant emission sources.
    /// </summary>
    private static double ScoreRegionalRelevance(
        ActionCategory category,
        HashSet<ActionCategory>? boosts)
    {
        if (boosts is null)
            return 0.0;

        return boosts.Contains(category) ? 1.0 : 0.0;
    }

    /// <summary>
    /// Maps user feedback to a score multiplier.
    /// "not_relevant" effectively excludes the suggestion.
    /// </summary>
    private static double GetFeedbackMultiplier(
        Dictionary<string, string> feedback,
        string suggestionId)
    {
        if (feedback is null || !feedback.TryGetValue(suggestionId, out string? value))
            return 1.0;

        return value switch
        {
            "not_relevant" => 0.0,
            "already_doing" => 0.1,
            "doing" => 0.3,
            _ => 1.0,
        };
    }

    private static HashSet<ActionCategory>? ResolveRegionalBoosts(string? region)
    {
        if (string.IsNullOrWhiteSpace(region))
            return null;

        return RegionalCategoryBoosts.GetValueOrDefault(region);
    }
}
