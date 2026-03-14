using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class Friend
{
    public Guid Id { get; set; }
    public string RequesterId { get; set; } = null!;
    public string AddresseeId { get; set; } = null!;
    public FriendRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    public ApplicationUser Requester { get; set; } = null!;
    public ApplicationUser Addressee { get; set; } = null!;
}
