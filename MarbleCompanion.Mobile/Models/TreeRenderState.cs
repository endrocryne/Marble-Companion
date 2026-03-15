using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Models;

public class TreeRenderState
{
    public TreeSpecies Species { get; set; } = TreeSpecies.Oak;

    public int Stage { get; set; }

    public string StageName { get; set; } = "Seed";

    public double HealthScore { get; set; } = 100;

    public TreeHealthState HealthState { get; set; } = TreeHealthState.Healthy;

    public int TotalLP { get; set; }

    public double TotalCo2EAvoided { get; set; }

    public int NextMilestoneLP { get; set; }

    public Season CurrentSeason { get; set; } = Season.Spring;

    public List<string> ActiveCosmetics { get; set; } = new();

    public bool HasCompanionCreature { get; set; }

    public string CompanionCreatureType { get; set; } = string.Empty;

    public float BreathingPhase { get; set; }
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}
