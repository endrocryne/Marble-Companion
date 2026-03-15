namespace MarbleCompanion.API.Models.Domain;

public class EmissionFactor
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ActionKey { get; set; } = string.Empty;
    public decimal FactorKgCO2ePerUnit { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int SourceYear { get; set; }
    public string? Region { get; set; }
    public bool IsActive { get; set; } = true;
}
