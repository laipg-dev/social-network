using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Api.Hubs;
using SocialNetwork.Application.Common.Realtime;

namespace SocialNetwork.Api.Realtime;

public sealed class SignalRRealtimeNotifier
    : IRealtimeNotifier
{
    private readonly IHubContext<RealtimeHub> _hubContext;

    public SignalRRealtimeNotifier(
        IHubContext<RealtimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToUserAsync(
        Guid userId,
        string eventName,
        object data,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"user:{userId}")
            .SendAsync(
                eventName,
                data,
                cancellationToken);
    }

    public Task SendToUsersAsync(
        IEnumerable<Guid> userIds,
        string eventName,
        object data,
        CancellationToken cancellationToken = default)
    {
        var groups = userIds
            .Distinct()
            .Select(id => $"user:{id}")
            .ToList();

        return _hubContext.Clients
            .Groups(groups)
            .SendAsync(
                eventName,
                data,
                cancellationToken);
    }

    public Task SendToGroupAsync(
        string groupName,
        string eventName,
        object data,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(groupName)
            .SendAsync(
                eventName,
                data,
                cancellationToken);
    }
}