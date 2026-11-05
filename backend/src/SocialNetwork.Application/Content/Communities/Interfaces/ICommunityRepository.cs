using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface ICommunityRepository
{
    Task<Community?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Community> Items, int TotalCount)> GetPagedAsync(
        Guid currentUserId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetSlugsWithPrefixAsync(
        string baseSlug,
        CancellationToken cancellationToken = default);

    Task<bool> IsMemberAsync(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> GetMemberCountAsync(
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetMemberCountsAsync(
        IReadOnlyCollection<Guid> communityIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Community community,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Community community,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Community community,
        CancellationToken cancellationToken = default);
}
