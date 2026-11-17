using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IRoomPermissionRepository
{
    Task<IReadOnlyList<RoomRolePermission>> GetByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task ReplaceRoleOverridesAsync(
        Guid roomId,
        Guid roleId,
        IReadOnlyCollection<(Guid PermissionId, bool IsAllowed)> overrides,
        CancellationToken cancellationToken = default);

    Task RemoveOverrideAsync(
        Guid roomId,
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default);
}
