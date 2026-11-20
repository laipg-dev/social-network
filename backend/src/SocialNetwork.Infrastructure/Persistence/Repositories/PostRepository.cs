using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _dbContext;

    public PostRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Post?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts
            .Include(post => post.Author)
                .ThenInclude(author => author.Profile)
            .FirstOrDefaultAsync(post => post.Id == id, cancellationToken);
    }
    public async Task AddAsync(Post post, CancellationToken cancellationToken)
    {
        await _dbContext.Posts.AddAsync(post, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetPageAsync(
        Guid? communityId,
        int page,
        int pageSize,
        Guid? viewerId,
        CancellationToken cancellationToken = default)
    {
        return await VisibleTo(viewerId)
            .AsNoTracking()
            .Include(post => post.Author)
                .ThenInclude(author => author.Profile)
            .Where(post => communityId == null || post.CommunityId == communityId)
            .OrderByDescending(post => post.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid? communityId,Guid? viewerId,CancellationToken cancellationToken = default) =>
        VisibleTo(viewerId).CountAsync(post => communityId == null || post.CommunityId == communityId,cancellationToken);

    public async Task<IReadOnlyList<Post>> GetByUserAsync(Guid userId,int page,int pageSize,Guid? viewerId,CancellationToken ct=default)=>await VisibleTo(viewerId).Where(x=>x.AuthorId==userId).OrderByDescending(x=>x.CreatedAt).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
    public Task<int> CountByUserAsync(Guid userId,Guid? viewerId,CancellationToken ct=default)=>VisibleTo(viewerId).CountAsync(x=>x.AuthorId==userId,ct);
    public async Task<IReadOnlyList<Post>> SearchAsync(string query,int page,int pageSize,Guid? viewerId,CancellationToken ct=default)=>await VisibleTo(viewerId).Where(x=>x.Content!=null&&x.Content.Contains(query)).OrderByDescending(x=>x.CreatedAt).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
    public Task<int> CountSearchAsync(string query,Guid? viewerId,CancellationToken ct=default)=>VisibleTo(viewerId).CountAsync(x=>x.Content!=null&&x.Content.Contains(query),ct);
    public Task<bool> UserExistsAsync(Guid userId,CancellationToken ct=default)=>_dbContext.Users.AnyAsync(x=>x.Id==userId,ct);
    private IQueryable<Post> VisibleTo(Guid? viewerId)
    {
        var now=DateTime.UtcNow;
        var q=_dbContext.Posts.AsNoTracking().Include(x=>x.Author).ThenInclude(x=>x.Profile).Where(x=>x.Status=="ACTIVE"&&x.QuarantinedAt==null);
        if(viewerId is null)return q.Where(x=>x.Visibility=="PUBLIC"&&(x.CommunityId==null||_dbContext.Communities.Any(c=>c.Id==x.CommunityId&&c.IsPublic)));
        var id=viewerId.Value;
        return q.Where(x =>
            // Keep the author's own personal content available; community bans still override it.
            (x.AuthorId == id && (x.CommunityId == null ||
                (!_dbContext.CommunityBans.Any(b => b.CommunityId == x.CommunityId && b.UserId == id && b.RevokedAt == null && (b.ExpiresAt == null || b.ExpiresAt > now))
                    && (_dbContext.Communities.Any(c => c.Id == x.CommunityId && (c.IsPublic || c.OwnerId == id))
                        || _dbContext.CommunityMembers.Any(m => m.CommunityId == x.CommunityId && m.UserId == id && m.Status == "ACTIVE")))))
            || (!_dbContext.UserBlocks.Any(b => (b.BlockerId == id && b.BlockedId == x.AuthorId) || (b.BlockerId == x.AuthorId && b.BlockedId == id))
                && (x.CommunityId == null || (_dbContext.Communities.Any(c => c.Id == x.CommunityId && (c.IsPublic || c.OwnerId == id)) || _dbContext.CommunityMembers.Any(m => m.CommunityId == x.CommunityId && m.UserId == id && m.Status == "ACTIVE"))
                    && !_dbContext.CommunityBans.Any(b => b.CommunityId == x.CommunityId && b.UserId == id && b.RevokedAt == null && (b.ExpiresAt == null || b.ExpiresAt > now))
                    )
                && (x.Visibility == "PUBLIC" || (x.Visibility == "FRIENDS" && _dbContext.FriendRequests.Any(r => r.Status == "ACCEPTED" && ((r.SenderId == id && r.ReceiverId == x.AuthorId) || (r.SenderId == x.AuthorId && r.ReceiverId == id)))))));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    
}
