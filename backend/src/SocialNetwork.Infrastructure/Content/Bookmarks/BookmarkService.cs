using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Bookmarks.Interfaces;
using SocialNetwork.Application.Content.Posts.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Application.Content.Posts.Interfaces;

namespace SocialNetwork.Infrastructure.Content.Bookmarks;

public sealed class BookmarkService(AppDbContext dbContext, IPostAccessService postAccess) : IBookmarkService
{
    public async Task<bool> SaveAsync(
        Guid postId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await postAccess.EnsureCanViewAsync(postId,userId,cancellationToken);

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
        await postAccess.EnsureCanViewAsync(postId,userId,cancellationToken);

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
        CancellationToken cancellationToken) => IsSavedAuthorizedAsync(postId,userId,cancellationToken);

    private async Task<bool> IsSavedAuthorizedAsync(Guid postId,Guid userId,CancellationToken ct)
    {
        await postAccess.EnsureCanViewAsync(postId,userId,ct);
        return await dbContext.Bookmarks.AnyAsync(
            item => item.PostId == postId &&
                    item.UserId == userId &&
                    item.Post.Status == "ACTIVE",
            ct);
    }

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

        var candidates = await query
            .Include(item => item.Post)
                .ThenInclude(post => post.Author)
            .Include(item => item.Post)
                .ThenInclude(post => post.Author.Profile)
            .Select(item => item.Post)
            .ToListAsync(cancellationToken);
        var accessible=new List<Post>();foreach(var post in candidates){try{await postAccess.EnsureCanViewAsync(post.Id,userId,cancellationToken);accessible.Add(post);}catch(NotFoundException){}}
        var totalCount=accessible.Count;
        var allowed=accessible.Skip((page-1)*pageSize).Take(pageSize).ToList();

        return new(
            allowed.Select(Map).ToList(),
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
