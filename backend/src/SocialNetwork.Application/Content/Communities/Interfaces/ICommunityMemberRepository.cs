using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface ICommunityMemberRepository
{
    Task<CommunityMember?> GetAsync(
        Guid communityId,
        Guid userId,
        bool includeRoles = false,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<CommunityMember> Items, int TotalCount)> GetPagedAsync(
        Guid communityId,
        string? status,
        Guid? roleId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CommunityRole?> GetDefaultRoleAsync(
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CommunityMember member,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        CommunityMember member,
        CancellationToken cancellationToken = default);

    Task RemoveRolesAsync(
        CommunityMember member,
        CancellationToken cancellationToken = default);

    Task RemoveRolesAsync(
        CommunityMember member,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken = default);
}
