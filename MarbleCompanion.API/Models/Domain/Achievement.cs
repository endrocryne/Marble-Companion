using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class Achievement
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public int LeafPointsReward { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Threshold { get; set; }
    public TreeSpecies? UnlockSpecies { get; set; }
    public string? UnlockCosmetic { get; set; }
}
