using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetAsync(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetByCommunityAsync(
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        Guid communityId,
        string name,
        Guid? excludeRoomId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetMemberCountsAsync(
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<int> GetMemberCountAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Room room,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Room room,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Room room,
        CancellationToken cancellationToken = default);
}
