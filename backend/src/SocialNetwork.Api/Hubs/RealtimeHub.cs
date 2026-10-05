using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Application.Common.Interfaces;

namespace SocialNetwork.Api.Hubs;

[Authorize]
public sealed class RealtimeHub : Hub
{
    private readonly ICurrentUser _currentUser;

    public RealtimeHub(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUser.IsAuthenticated)
        {
            // Một group riêng cho mỗi user.
            // Sau này Notification/Messaging có thể gửi realtime
            // tới user:{userId}.
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user:{_currentUser.UserId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        if (_currentUser.IsAuthenticated)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"user:{_currentUser.UserId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}