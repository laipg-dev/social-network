using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class CommunityMemberRepository : ICommunityMemberRepository
{
    private readonly AppDbContext _dbContext;

    public CommunityMemberRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommunityMember?> GetAsync(
        Guid communityId,
        Guid userId,
        bool includeRoles = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CommunityMember> query = _dbContext.CommunityMembers
            .Include(x => x.User)
            .ThenInclude(x => x.Profile);

        if (includeRoles)
        {
            query = query
                .Include(x => x.Roles)
                .ThenInclude(x => x.Role);
        }

        return await query.FirstOrDefaultAsync(
            x => x.CommunityId == communityId
                && x.UserId == userId,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<CommunityMember> Items, int TotalCount)> GetPagedAsync(
        Guid communityId,
        string? status,
        Guid? roleId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CommunityMember> query = _dbContext.CommunityMembers
            .AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(x => x.Profile)
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .Where(x => x.CommunityId == communityId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (roleId.HasValue)
        {
            query = query.Where(x =>
                x.Roles.Any(role => role.RoleId == roleId.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLikePattern(search.Trim())}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.User.Username, pattern, "\\")
                || (x.User.Profile != null
                    && x.User.Profile.DisplayName != null
                    && EF.Functions.ILike(
                        x.User.Profile.DisplayName,
                        pattern,
                        "\\")));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.RequestedAt)
            .ThenBy(x => x.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<CommunityRole?> GetDefaultRoleAsync(
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CommunityRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.CommunityId == communityId
                    && x.IsDefault,
                cancellationToken);
    }

    public async Task AddAsync(
        CommunityMember member,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CommunityMembers.AddAsync(
            member,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        CommunityMember member,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(member).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveRolesAsync(
        CommunityMember member,
        CancellationToken cancellationToken = default)
    {
        _dbContext.CommunityMemberRoles.RemoveRange(member.Roles);

        return Task.CompletedTask;
    }

    public Task RemoveRolesAsync(
        CommunityMember member,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        var links = member.Roles
            .Where(x => roleIds.Contains(x.RoleId))
            .ToArray();

        _dbContext.CommunityMemberRoles.RemoveRange(links);

        return Task.CompletedTask;
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
