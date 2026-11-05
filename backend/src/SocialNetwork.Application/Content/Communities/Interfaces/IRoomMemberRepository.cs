using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IRoomMemberRepository
{
    Task<RoomMember?> GetAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomMember>> GetByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RoomMember member,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        RoomMember member,
        CancellationToken cancellationToken = default);
}
