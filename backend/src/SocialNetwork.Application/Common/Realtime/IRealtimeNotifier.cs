namespace SocialNetwork.Application.Common.Realtime;

public interface IRealtimeNotifier
{
    Task SendToUserAsync(
        Guid userId,
        string eventName,
        object data,
        CancellationToken cancellationToken = default);

    Task SendToUsersAsync(
        IEnumerable<Guid> userIds,
        string eventName,
        object data,
        CancellationToken cancellationToken = default);

    Task SendToGroupAsync(
        string groupName,
        string eventName,
        object data,
        CancellationToken cancellationToken = default);
}