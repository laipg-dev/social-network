using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IRoomPermissionService
{
    Task<IReadOnlyList<RoomPermissionOverrideResponse>> GetOverridesAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomPermissionOverrideResponse>> ReplaceRoleOverridesAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        Guid roleId,
        UpdateRoomRolePermissionsRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveOverrideAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        Guid roleId,
        string code,
        CancellationToken cancellationToken = default);
}
