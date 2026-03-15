namespace MarbleCompanion.API.Models.Domain;

public class DeviceToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
