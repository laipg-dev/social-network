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
            .Include(post => post.Media)
            .FirstOrDefaultAsync(post => post.Id == id && post.Status != "DELETED", cancellationToken);
    }
    public async Task AddAsync(
        Post post,
        IReadOnlyList<Guid>? mediaIds = null,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Posts.AddAsync(post, cancellationToken);

        if (mediaIds is not null && mediaIds.Count > 0)
        {
            var medias = await _dbContext.Media
                .Where(m => mediaIds.Contains(m.Id) && m.UploaderId == post.AuthorId)
                .ToListAsync(cancellationToken);

            var position = 0;
            foreach (var mediaId in mediaIds)
            {
                var media = medias.FirstOrDefault(m => m.Id == mediaId);
                if (media is not null)
                {
                    media.ProcessingStatus = "ATTACHED";
                    post.Media.Add(new PostMedia
                    {
                        PostId = post.Id,
                        MediaId = media.Id,
                        Url = media.Url,
                        MediaType = media.MimeType,
                        Position = position++
                    });
                }
            }
        }

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
            .Include(post => post.Media)
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