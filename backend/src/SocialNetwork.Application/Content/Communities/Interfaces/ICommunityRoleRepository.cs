using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface ICommunityRoleRepository
{
    Task<IReadOnlyList<CommunityRole>> GetByCommunityAsync(
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<CommunityRole?> GetAsync(
        Guid communityId,
        Guid roleId,
        bool includePermissions = true,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        Guid communityId,
        string name,
        Guid? excludeRoleId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetMemberCountsAsync(
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(
        IReadOnlyList<CommunityRole> roles,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default);

    Task SetDefaultAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default);

    Task ReplacePermissionsAsync(
        CommunityRole role,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default);

    Task ReassignMembersAsync(
        Guid communityId,
        Guid fromRoleId,
        Guid toRoleId,
        CancellationToken cancellationToken = default);
}
