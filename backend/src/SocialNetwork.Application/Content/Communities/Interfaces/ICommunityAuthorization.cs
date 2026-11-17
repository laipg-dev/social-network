using SocialNetwork.Application.Content.Communities.Authorization;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface ICommunityAuthorization
{
    Task<CommunityAccess> GetAccessAsync(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CommunityAccess> RequireCommunityPermissionAsync(
        Guid communityId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);

    Task<RoomAccess> GetRoomAccessAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, RoomAccess>> GetRoomAccessesAsync(
        Guid communityId,
        Guid userId,
        IReadOnlyCollection<Guid> roomIds,
        CancellationToken cancellationToken = default);

    Task<RoomAccess> RequireRoomPermissionAsync(
        Guid roomId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);
}
