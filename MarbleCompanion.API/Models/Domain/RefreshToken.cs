namespace MarbleCompanion.API.Models.Domain;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
