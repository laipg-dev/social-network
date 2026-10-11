using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Bookmarks.Interfaces;
using SocialNetwork.Application.Content.Posts.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Content.Bookmarks;

public sealed class BookmarkService(AppDbContext dbContext) : IBookmarkService
{
    public async Task<bool> SaveAsync(
        Guid postId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await EnsureActivePostAsync(postId, cancellationToken);

        var bookmark = await dbContext.Bookmarks
            .SingleOrDefaultAsync(item => item.PostId == postId && item.UserId == userId, cancellationToken);
        if (bookmark is null)
        {
            dbContext.Bookmarks.Add(new Bookmark
            {
                PostId = postId,
                UserId = userId
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> RemoveAsync(
        Guid postId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await EnsureActivePostAsync(postId, cancellationToken);

        var bookmark = await dbContext.Bookmarks
            .SingleOrDefaultAsync(item => item.PostId == postId && item.UserId == userId, cancellationToken);
        if (bookmark is not null)
        {
            dbContext.Bookmarks.Remove(bookmark);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return false;
    }

    public Task<bool> IsSavedAsync(
        Guid postId,
        Guid userId,
        CancellationToken cancellationToken) =>
        dbContext.Bookmarks.AnyAsync(
            item => item.PostId == postId &&
                    item.UserId == userId &&
                    item.Post.Status == "ACTIVE",
            cancellationToken);

    public async Task<PostPageResponse> GetPageAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = dbContext.Bookmarks
            .AsNoTracking()
            .Where(item => item.UserId == userId && item.Post.Status == "ACTIVE")
            .OrderByDescending(item => item.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var posts = await query
            .Include(item => item.Post)
                .ThenInclude(post => post.Author)
            .Include(item => item.Post)
                .ThenInclude(post => post.Author.Profile)
            .Select(item => item.Post)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new(
            posts.Select(Map).ToList(),
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    private async Task EnsureActivePostAsync(Guid postId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Posts.AnyAsync(
                post => post.Id == postId && post.Status == "ACTIVE",
                cancellationToken))
            throw new NotFoundException("Post", postId);
    }

    private static PostResponse Map(Post post) => new()
    {
        Id = post.Id,
        Content = post.Content,
        Visibility = post.Visibility,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt,
        AuthorId = post.AuthorId,
        AuthorUsername = post.Author?.Username,
        AuthorDisplayName = post.Author?.Profile?.DisplayName,
        AuthorAvatarUrl = post.Author?.Profile?.AvatarUrl,
        CommunityId = post.CommunityId,
        Status = post.Status
    };
}
