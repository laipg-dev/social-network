using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class CommunityRepository : ICommunityRepository
{
    private readonly AppDbContext _dbContext;

    public CommunityRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Community?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Communities
            .Include(x => x.Owner)
            .ThenInclude(x => x.Profile)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Community> Items, int TotalCount)> GetPagedAsync(
        Guid currentUserId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Community> query = _dbContext.Communities
            .AsNoTracking();

        // Chỉ trả cộng đồng ACTIVE công khai hoặc cộng đồng mà user hiện tại
        // là thành viên. Community CLOSED bị ẩn khỏi danh sách khám phá.
        query = query.Where(x =>
            x.Status == CommunityStatus.Active
            && (currentUserId == Guid.Empty || !_dbContext.CommunityBans.Any(b => b.CommunityId == x.Id && b.UserId == currentUserId && b.RevokedAt == null && (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)))
            && (x.IsPublic
                || x.Members.Any(member => member.UserId == currentUserId && (member.Status == CommunityMemberStatus.Active || member.Status == CommunityMemberStatus.Pending))));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLikePattern(search.Trim())}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern, "\\"));
        }

        query = query
            .Include(x => x.Owner)
            .ThenInclude(x => x.Profile);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<string>> GetSlugsWithPrefixAsync(
        string baseSlug,
        CancellationToken cancellationToken = default)
    {
        var pattern = $"{EscapeLikePattern(baseSlug)}%";

        return await _dbContext.Communities
            .AsNoTracking()
            .Where(x => EF.Functions.ILike(x.Slug, pattern, "\\"))
            .Select(x => x.Slug)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsMemberAsync(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CommunityMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.CommunityId == communityId
                    && x.UserId == userId,
                cancellationToken);
    }

    public async Task<int> GetMemberCountAsync(
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CommunityMembers
            .AsNoTracking()
            .CountAsync(
                x => x.CommunityId == communityId,
                cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetMemberCountsAsync(
        IReadOnlyCollection<Guid> communityIds,
        CancellationToken cancellationToken = default)
    {
        if (communityIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var counts = await _dbContext.CommunityMembers
            .AsNoTracking()
            .Where(x => communityIds.Contains(x.CommunityId))
            .GroupBy(x => x.CommunityId)
            .Select(group => new
            {
                CommunityId = group.Key,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(
            x => x.CommunityId,
            x => x.Count);
    }

    public async Task AddAsync(
        Community community,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Communities.AddAsync(
            community,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Community community,
        CancellationToken cancellationToken = default)
    {
        // Chỉ đánh dấu entity gốc để không vô tình update cả Owner/Profile.
        _dbContext.Entry(community).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Community community,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Communities.Remove(community);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
