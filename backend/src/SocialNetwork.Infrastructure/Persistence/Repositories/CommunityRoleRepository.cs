using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class CommunityRoleRepository : ICommunityRoleRepository
{
    private readonly AppDbContext _dbContext;

    public CommunityRoleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommunityRole>> GetByCommunityAsync(
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CommunityRoles
            .Include(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .Where(x => x.CommunityId == communityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<CommunityRole?> GetAsync(
        Guid communityId,
        Guid roleId,
        bool includePermissions = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CommunityRole> query = _dbContext.CommunityRoles;

        if (includePermissions)
        {
            query = query
                .Include(x => x.Permissions)
                .ThenInclude(x => x.Permission);
        }

        return await query.FirstOrDefaultAsync(
            x => x.CommunityId == communityId
                && x.Id == roleId,
            cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        Guid communityId,
        string name,
        Guid? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CommunityRoles
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId
                && x.Name == name);

        if (excludeRoleId.HasValue)
        {
            query = query.Where(x => x.Id != excludeRoleId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetMemberCountsAsync(
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        var counts = await _dbContext.CommunityMemberRoles
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId)
            .GroupBy(x => x.RoleId)
            .Select(group => new
            {
                RoleId = group.Key,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(
            x => x.RoleId,
            x => x.Count);
    }

    public async Task AddAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CommunityRoles.AddAsync(
            role,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(role).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(
        IReadOnlyList<CommunityRole> roles,
        CancellationToken cancellationToken = default)
    {
        _dbContext.CommunityRoles.UpdateRange(roles);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default)
    {
        _dbContext.CommunityRoles.Remove(role);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDefaultAsync(
        CommunityRole role,
        CancellationToken cancellationToken = default)
    {
        var otherDefaults = await _dbContext.CommunityRoles
            .Where(x => x.CommunityId == role.CommunityId
                && x.Id != role.Id
                && x.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var other in otherDefaults)
        {
            other.IsDefault = false;
        }

        role.IsDefault = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplacePermissionsAsync(
        CommunityRole role,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RolePermissions.RemoveRange(role.Permissions);
        role.Permissions.Clear();

        foreach (var permissionId in permissionIds)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReassignMembersAsync(
        Guid communityId,
        Guid fromRoleId,
        Guid toRoleId,
        CancellationToken cancellationToken = default)
    {
        var links = await _dbContext.CommunityMemberRoles
            .Where(x => x.CommunityId == communityId
                && x.RoleId == fromRoleId)
            .ToListAsync(cancellationToken);

        var targetUserIds = await _dbContext.CommunityMemberRoles
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId
                && x.RoleId == toRoleId)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);

        foreach (var link in links)
        {
            _dbContext.CommunityMemberRoles.Remove(link);

            if (!targetUserIds.Contains(link.UserId))
            {
                _dbContext.CommunityMemberRoles.Add(new CommunityMemberRole
                {
                    CommunityId = communityId,
                    UserId = link.UserId,
                    RoleId = toRoleId
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
