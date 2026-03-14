using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Services;

public interface IFriendService
{
    Task<List<FriendDto>> GetFriendsAsync(string userId);
    Task<FriendRequestDto> SendRequestAsync(string userId, string targetUsername);
    Task<FriendDto> AcceptRequestAsync(string userId, Guid requestId);
    Task RemoveFriendAsync(string userId, Guid friendId);
    Task<List<FeedEventDto>> GetFeedAsync(string userId);
    Task<TreeDto> GetFriendTreeAsync(string userId, Guid friendId);
    Task ReactToFeedEventAsync(string userId, Guid eventId, ReactionType reaction);
}
