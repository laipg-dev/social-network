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
            .FirstOrDefaultAsync(post => post.Id == id && post.Status != "DELETED", cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts
            .AsNoTracking()
            .Include(post => post.Author)
                .ThenInclude(author => author.Profile)
            .Where(post => post.Status == "ACTIVE" &&
                (communityId == null || post.CommunityId == communityId))
            .OrderByDescending(post => post.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid? communityId, CancellationToken cancellationToken = default) =>
        _dbContext.Posts.CountAsync(post => post.Status == "ACTIVE" &&
            (communityId == null || post.CommunityId == communityId), cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    
}