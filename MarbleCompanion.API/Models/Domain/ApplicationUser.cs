using Microsoft.AspNetCore.Identity;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public int AvatarIndex { get; set; }
    public string? Region { get; set; }

    // Onboarding lifestyle
    public string? DietType { get; set; }
    public string? TransportMode { get; set; }
    public string? EnergySource { get; set; }
    public string? FlightFrequency { get; set; }
    public string? ShoppingHabits { get; set; }
    public decimal CarbonBaselineKg { get; set; }

    // Progression
    public int TotalLeafPoints { get; set; }
    public decimal TotalCO2eAvoided { get; set; }

    // Virtual tree
    public TreeSpecies TreeSpecies { get; set; }
    public int TreeStage { get; set; }
    public int TreeHealthScore { get; set; } = 100;
    public DateTime? LastActionDate { get; set; }

    // Cosmetics
    public string? ActiveSkyTheme { get; set; }
    public string? ActiveGroundTheme { get; set; }
    public string? ActiveCompanionCreature { get; set; }

    // Streaks
    public int StreakCurrent { get; set; }
    public int StreakBest { get; set; }
    public bool StreakFreezeAvailable { get; set; }
    public DateTime? StreakFreezeLastUsed { get; set; }

    // Symbolic offsets
    public int OffsetCredits { get; set; }
    public int SymbolicTreesPlanted { get; set; }
    public decimal SymbolicCO2eOffset { get; set; }
    public int SymbolicWindHours { get; set; }

    // Preferences
    public bool UnitsMetric { get; set; } = true;
    public string? ThemePreference { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public string? PinnedAchievementIds { get; set; }
}
