using MarbleCompanion.API.Data;
using MarbleCompanion.API.Models.Domain;
using MarbleCompanion.Shared.Constants;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarbleCompanion.API.Services;

public class FriendService : IFriendService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITreeService _treeService;

    public FriendService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        ITreeService treeService)
    {
        _db = db;
        _userManager = userManager;
        _treeService = treeService;
    }

    public async Task<List<FriendDto>> GetFriendsAsync(string userId)
    {
        var friendRecords = await _db.Friends
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId)
                     && f.Status == FriendRequestStatus.Accepted)
            .ToListAsync();

        return friendRecords.Select(f =>
        {
            var friend = f.RequesterId == userId ? f.Addressee : f.Requester;
            return new FriendDto
            {
                UserId = Guid.Parse(friend.Id),
                DisplayName = friend.DisplayName,
                AvatarIndex = friend.AvatarIndex,
                TotalLP = friend.TotalLeafPoints,
                CurrentStreak = friend.StreakCurrent,
                FriendsSince = f.AcceptedAt ?? f.CreatedAt
            };
        }).ToList();
    }

    public async Task<FriendRequestDto> SendRequestAsync(string userId, string targetUsername)
    {
        var target = await _userManager.FindByNameAsync(targetUsername)
            ?? throw new KeyNotFoundException("User not found.");

        if (target.Id == userId)
            throw new InvalidOperationException("Cannot send a friend request to yourself.");

        var existing = await _db.Friends
            .AnyAsync(f =>
                (f.RequesterId == userId && f.AddresseeId == target.Id) ||
                (f.RequesterId == target.Id && f.AddresseeId == userId));
        if (existing)
            throw new InvalidOperationException("A friend request already exists between these users.");

        var requester = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("Requester not found.");

        var friendRequest = new Friend
        {
            Id = Guid.NewGuid(),
            RequesterId = userId,
            AddresseeId = target.Id,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Friends.Add(friendRequest);
        await _db.SaveChangesAsync();

        return new FriendRequestDto
        {
            Id = friendRequest.Id,
            FromUserId = Guid.Parse(userId),
            FromDisplayName = requester.DisplayName,
            FromAvatarIndex = requester.AvatarIndex,
            Status = FriendRequestStatus.Pending,
            SentAt = friendRequest.CreatedAt
        };
    }

    public async Task<FriendDto> AcceptRequestAsync(string userId, Guid requestId)
    {
        var request = await _db.Friends
            .Include(f => f.Requester)
            .FirstOrDefaultAsync(f => f.Id == requestId && f.AddresseeId == userId && f.Status == FriendRequestStatus.Pending)
            ?? throw new KeyNotFoundException("Friend request not found.");

        request.Status = FriendRequestStatus.Accepted;
        request.AcceptedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new FriendDto
        {
            UserId = Guid.Parse(request.Requester.Id),
            DisplayName = request.Requester.DisplayName,
            AvatarIndex = request.Requester.AvatarIndex,
            TotalLP = request.Requester.TotalLeafPoints,
            CurrentStreak = request.Requester.StreakCurrent,
            FriendsSince = request.AcceptedAt ?? DateTime.UtcNow
        };
    }

    public async Task RemoveFriendAsync(string userId, Guid friendId)
    {
        var targetUserId = friendId.ToString();
        var friendship = await _db.Friends
            .FirstOrDefaultAsync(f =>
                ((f.RequesterId == userId && f.AddresseeId == targetUserId) ||
                 (f.RequesterId == targetUserId && f.AddresseeId == userId))
                && f.Status == FriendRequestStatus.Accepted)
            ?? throw new KeyNotFoundException("Friendship not found.");

        _db.Friends.Remove(friendship);
        await _db.SaveChangesAsync();
    }

    public async Task<List<FeedEventDto>> GetFeedAsync(string userId)
    {
        var friendIds = await _db.Friends
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId)
                     && f.Status == FriendRequestStatus.Accepted)
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var events = await _db.FeedEvents
            .Include(e => e.User)
            .Include(e => e.Reactions)
            .Where(e => friendIds.Contains(e.UserId) && e.ExpiresAt > now)
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .ToListAsync();

        return events.Select(e =>
        {
            var reactionCounts = e.Reactions
                .GroupBy(r => r.ReactionType)
                .ToDictionary(g => g.Key, g => g.Count());

            var userReaction = e.Reactions
                .FirstOrDefault(r => r.UserId == userId)?.ReactionType;

            return new FeedEventDto
            {
                Id = e.Id,
                UserId = Guid.Parse(e.UserId),
                DisplayName = e.User.DisplayName,
                AvatarIndex = e.User.AvatarIndex,
                EventType = e.EventType,
                Title = e.Title,
                Description = e.Description,
                OccurredAt = e.CreatedAt,
                Reactions = reactionCounts,
                UserReaction = userReaction
            };
        }).ToList();
    }

    public async Task<TreeDto> GetFriendTreeAsync(string userId, Guid friendId)
    {
        var friendUserId = friendId.ToString();

        // Verify friendship
        var isFriend = await _db.Friends
            .AnyAsync(f =>
                ((f.RequesterId == userId && f.AddresseeId == friendUserId) ||
                 (f.RequesterId == friendUserId && f.AddresseeId == userId))
                && f.Status == FriendRequestStatus.Accepted);

        if (!isFriend)
            throw new UnauthorizedAccessException("Not friends with this user.");

        return await _treeService.GetTreeAsync(friendUserId);
    }

    public async Task ReactToFeedEventAsync(string userId, Guid eventId, ReactionType reaction)
    {
        var feedEvent = await _db.FeedEvents.FirstOrDefaultAsync(e => e.Id == eventId)
            ?? throw new KeyNotFoundException("Feed event not found.");

        var existing = await _db.FeedReactions
            .FirstOrDefaultAsync(r => r.FeedEventId == eventId && r.UserId == userId);

        if (existing != null)
        {
            existing.ReactionType = reaction;
        }
        else
        {
            _db.FeedReactions.Add(new FeedReaction
            {
                FeedEventId = eventId,
                UserId = userId,
                ReactionType = reaction,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }
}
