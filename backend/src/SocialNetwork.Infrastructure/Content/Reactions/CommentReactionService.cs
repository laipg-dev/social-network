using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Reactions.Interfaces;
using SocialNetwork.Application.Content.Reactions.Requests;
using SocialNetwork.Application.Content.Reactions.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Application.Identity.SocialGraph;

namespace SocialNetwork.Infrastructure.Content.Reactions;

public sealed class CommentReactionService(AppDbContext dbContext, IPostAccessService postAccess) : ICommentReactionService
{
    private static readonly HashSet<string> SupportedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "LIKE", "LOVE", "HAHA", "WOW", "SAD", "ANGRY" };

    public async Task<CommentReactionResponse> SetAsync(
        Guid commentId,
        Guid userId,
        SetPostReactionRequest request,
        CancellationToken cancellationToken)
    {
        var type = request.Type.Trim().ToUpperInvariant();
        if (!SupportedTypes.Contains(type))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["type"] = ["Supported values are LIKE, LOVE, HAHA, WOW, SAD and ANGRY."]
            });

        await EnsureCanAccessCommentAsync(commentId,userId,cancellationToken);

        var reaction = await dbContext.CommentReactions
            .SingleOrDefaultAsync(item => item.CommentId == commentId && item.UserId == userId, cancellationToken);

        if (reaction is null)
        {
            dbContext.CommentReactions.Add(new CommentReaction
            {
                CommentId = commentId,
                UserId = userId,
                Type = type
            });
        }
        else
        {
            reaction.Type = type;
            reaction.CreatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(commentId, userId, cancellationToken);
    }

    public async Task<CommentReactionResponse> RemoveAsync(
        Guid commentId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await EnsureCanAccessCommentAsync(commentId,userId,cancellationToken);

        var reaction = await dbContext.CommentReactions
            .SingleOrDefaultAsync(item => item.CommentId == commentId && item.UserId == userId, cancellationToken);
        if (reaction is not null)
        {
            dbContext.CommentReactions.Remove(reaction);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return await BuildResponseAsync(commentId, userId, cancellationToken);
    }

    public async Task<CommentReactionResponse> GetAsync(
        Guid commentId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        await EnsureCanAccessCommentAsync(commentId,userId,cancellationToken);
        return await BuildResponseAsync(commentId, userId, cancellationToken);
    }

    public async Task<PagedResponse<PublicUserResponse>> GetUsersAsync(Guid commentId,Guid? viewerId,int page,int pageSize,CancellationToken ct)
    {
        await EnsureCanAccessCommentAsync(commentId,viewerId,ct);page=Math.Max(1,page);pageSize=Math.Clamp(pageSize,1,100);
        var q=dbContext.CommentReactions.AsNoTracking().Where(x=>x.CommentId==commentId
            && !dbContext.CommunityBans.Any(b=>b.CommunityId==x.Comment.Post.CommunityId&&b.UserId==x.UserId&&b.CreatedAt>=x.CreatedAt)).Include(x=>x.User).ThenInclude(x=>x.Profile).OrderBy(x=>x.User.Username);
        var count=await q.CountAsync(ct);var users=await q.Skip((page-1)*pageSize).Take(pageSize).Select(x=>x.User).ToListAsync(ct);
        return new(){Items=users.Select(x=>new PublicUserResponse(x.Id,x.Username,x.Profile?.DisplayName,x.Profile?.Bio,x.Profile?.AvatarUrl,x.CreatedAt)).ToArray(),Page=page,PageSize=pageSize,TotalCount=count};
    }
    private async Task EnsureCanAccessCommentAsync(Guid id,Guid? viewer,CancellationToken ct)
    {
        var postId=await dbContext.Comments.Where(x=>x.Id==id&&x.Status=="ACTIVE"&&x.QuarantinedAt==null).Select(x=>(Guid?)x.PostId).SingleOrDefaultAsync(ct)??throw new NotFoundException("Comment",id);
        await postAccess.EnsureCanViewAsync(postId,viewer,ct);
    }

    private async Task EnsureActiveCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Comments.AnyAsync(
                comment => comment.Id == commentId && comment.Status == "ACTIVE",
                cancellationToken))
            throw new NotFoundException("Comment", commentId);
    }

    private async Task<CommentReactionResponse> BuildResponseAsync(
        Guid commentId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var reactions = await dbContext.CommentReactions
            .AsNoTracking()
            .Where(reaction => reaction.CommentId == commentId
                && !dbContext.CommunityBans.Any(b=>b.CommunityId==reaction.Comment.Post.CommunityId&&b.UserId==reaction.UserId&&b.CreatedAt>=reaction.CreatedAt))
            .ToListAsync(cancellationToken);

        var counts = reactions
            .GroupBy(reaction => reaction.Type)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var currentReaction = userId is null
            ? null
            : reactions.FirstOrDefault(reaction => reaction.UserId == userId.Value)?.Type;

        return new CommentReactionResponse(commentId, currentReaction, counts, reactions.Count);
    }

}
