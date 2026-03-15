namespace MarbleCompanion.API.Models.Domain;

public class TreeCosmetic
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string UnlockMethod { get; set; } = string.Empty;
    public int LeafPointsCost { get; set; }
}
