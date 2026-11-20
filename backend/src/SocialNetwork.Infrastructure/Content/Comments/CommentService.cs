using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Comments.Interfaces;
using SocialNetwork.Application.Content.Comments.Requests;
using SocialNetwork.Application.Content.Comments.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Content.Comments;

public sealed class CommentService(AppDbContext dbContext, IPostAccessService postAccess) : ICommentService
{
    public async Task<CommentResponse> CreateAsync(
        Guid postId,
        Guid authorId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        await postAccess.EnsureCanViewAsync(postId,authorId,cancellationToken);

        if (request.ParentCommentId is not null &&
            !await dbContext.Comments.AnyAsync(comment =>
                comment.Id == request.ParentCommentId &&
                comment.PostId == postId &&
                comment.Status == "ACTIVE" && comment.QuarantinedAt == null, cancellationToken))
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

    public async Task<PagedResponse<CommentResponse>> GetForPostAsync(Guid postId,Guid? viewerId,int page,int pageSize,CancellationToken cancellationToken)
    {
        await postAccess.EnsureCanViewAsync(postId,viewerId,cancellationToken);
        page=Math.Max(page,1);pageSize=Math.Clamp(pageSize,1,100);
        var query=dbContext.Comments.AsNoTracking().Where(comment=>comment.PostId==postId&&comment.Status=="ACTIVE"&&comment.QuarantinedAt==null&&comment.ParentCommentId==null);
        var total=await query.CountAsync(cancellationToken);
        var roots = await query
            .OrderBy(comment=>comment.CreatedAt).ThenBy(comment=>comment.Id)
            .Skip((page-1)*pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var comments = await dbContext.Comments
            .AsNoTracking()
            .Where(comment => comment.PostId == postId && comment.Status == "ACTIVE" && comment.QuarantinedAt == null)
            .OrderBy(comment => comment.CreatedAt).ThenBy(comment=>comment.Id)
            .ToListAsync(cancellationToken);

        var byParent = comments
            .Where(comment => comment.ParentCommentId is not null)
            .GroupBy(comment => comment.ParentCommentId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());
        return new PagedResponse<CommentResponse>{Items=roots.Select(comment=>Map(comment,BuildReplies(comment.Id,byParent))).ToArray(),Page=page,PageSize=pageSize,TotalCount=total};
    }

    public async Task<CommentResponse> UpdateAsync(
        Guid id,
        Guid authorId,
        string content,
        CancellationToken cancellationToken)
    {
        var comment = await dbContext.Comments
            .SingleOrDefaultAsync(item => item.Id == id && item.Status == "ACTIVE" && item.QuarantinedAt == null, cancellationToken)
            ?? throw new NotFoundException("Comment", id);
        EnsureAuthor(comment, authorId);
        await postAccess.EnsureCanViewAsync(comment.PostId,authorId,cancellationToken);
        comment.Content = content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(comment, []);
    }

    public async Task DeleteAsync(Guid id, Guid authorId, CancellationToken cancellationToken)
    {
        var comment = await dbContext.Comments
            .SingleOrDefaultAsync(item => item.Id == id && item.Status == "ACTIVE" && item.QuarantinedAt == null, cancellationToken)
            ?? throw new NotFoundException("Comment", id);
        EnsureAuthor(comment, authorId);
        await postAccess.EnsureCanViewAsync(comment.PostId,authorId,cancellationToken);
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
            comment.Content, comment.Status, comment.CreatedAt, comment.UpdatedAt, replies);

    private static void EnsureAuthor(Comment comment, Guid authorId)
    {
        if (comment.AuthorId != authorId)
            throw new ForbiddenException("Only the comment author can modify this comment.");
    }
}
