using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MarbleCompanion.API.Hubs;

[Authorize]
public class ActivityHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinFriendUpdates(string friendUserId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"friend-{friendUserId}");
    }

    public async Task LeaveFriendUpdates(string friendUserId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"friend-{friendUserId}");
    }
}
