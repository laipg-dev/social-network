using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Comments.Interfaces;
using SocialNetwork.Application.Content.Comments.Requests;
using SocialNetwork.Application.Content.Comments.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Content.Comments;

public sealed class CommentService(AppDbContext dbContext) : ICommentService
{
    public async Task<CommentResponse> CreateAsync(
        Guid postId,
        Guid authorId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Posts.AnyAsync(post => post.Id == postId && post.Status == "ACTIVE", cancellationToken))
            throw new NotFoundException("Post", postId);

        if (request.ParentCommentId is not null &&
            !await dbContext.Comments.AnyAsync(comment =>
                comment.Id == request.ParentCommentId &&
                comment.PostId == postId &&
                comment.Status == "ACTIVE", cancellationToken))
            throw new NotFoundException("Parent comment", request.ParentCommentId);

        var comment = new Comment
        {
            PostId = postId,
            AuthorId = authorId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim()
        };
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(comment, []);
    }

    public async Task<IReadOnlyList<CommentResponse>> GetForPostAsync(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var comments = await dbContext.Comments
            .AsNoTracking()
            .Include(comment => comment.Author)
                .ThenInclude(author => author.Profile)
            .Where(comment => comment.PostId == postId && comment.Status == "ACTIVE")
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

        var byParent = comments
            .Where(comment => comment.ParentCommentId is not null)
            .GroupBy(comment => comment.ParentCommentId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());
        return comments
            .Where(comment => comment.ParentCommentId is null)
            .Select(comment => Map(comment, BuildReplies(comment.Id, byParent)))
            .ToList();
    }

    public async Task<CommentResponse> UpdateAsync(
        Guid id,
        Guid authorId,
        string content,
        CancellationToken cancellationToken)
    {
        var comment = await dbContext.Comments
            .Include(c => c.Author)
                .ThenInclude(a => a.Profile)
            .SingleOrDefaultAsync(item => item.Id == id && item.Status == "ACTIVE", cancellationToken)
            ?? throw new NotFoundException("Comment", id);
        EnsureAuthor(comment, authorId);
        comment.Content = content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(comment, []);
    }

    public async Task DeleteAsync(Guid id, Guid authorId, CancellationToken cancellationToken)
    {
        var comment = await dbContext.Comments
            .SingleOrDefaultAsync(item => item.Id == id && item.Status == "ACTIVE", cancellationToken)
            ?? throw new NotFoundException("Comment", id);
        EnsureAuthor(comment, authorId);
        comment.Status = "DELETED";
        comment.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<CommentResponse> BuildReplies(
        Guid parentId,
        IReadOnlyDictionary<Guid, List<Comment>> byParent) =>
        byParent.TryGetValue(parentId, out var children)
            ? children.Select(child => Map(child, BuildReplies(child.Id, byParent))).ToList()
            : [];

    private static CommentResponse Map(Comment comment, IReadOnlyList<CommentResponse> replies) =>
        new(comment.Id, comment.PostId, comment.AuthorId, comment.ParentCommentId,
            comment.Content, comment.Status, comment.CreatedAt, comment.UpdatedAt, replies,
            comment.Author?.Username, comment.Author?.Profile?.DisplayName, comment.Author?.Profile?.AvatarUrl);

    private static void EnsureAuthor(Comment comment, Guid authorId)
    {
        if (comment.AuthorId != authorId)
            throw new ForbiddenException("Only the comment author can modify this comment.");
    }
}
