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

public sealed class PostReactionService(AppDbContext dbContext, IPostAccessService postAccess) : IPostReactionService
{
    private static readonly HashSet<string> SupportedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "LIKE", "LOVE", "HAHA", "WOW", "SAD", "ANGRY" };

    public async Task<PostReactionResponse> SetAsync(
        Guid postId,
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

        await postAccess.EnsureCanViewAsync(postId,userId,cancellationToken);

        var reaction = await dbContext.PostReactions
            .SingleOrDefaultAsync(item => item.PostId == postId && item.UserId == userId, cancellationToken);

        if (reaction is null)
        {
            dbContext.PostReactions.Add(new PostReaction
            {
                PostId = postId,
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
        return await BuildResponseAsync(postId, userId, cancellationToken);
    }

    public async Task<PostReactionResponse> RemoveAsync(
        Guid postId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await postAccess.EnsureCanViewAsync(postId,userId,cancellationToken);

        var reaction = await dbContext.PostReactions
            .SingleOrDefaultAsync(item => item.PostId == postId && item.UserId == userId, cancellationToken);
        if (reaction is not null)
        {
            dbContext.PostReactions.Remove(reaction);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return await BuildResponseAsync(postId, userId, cancellationToken);
    }

    public async Task<PostReactionResponse> GetAsync(
        Guid postId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        await postAccess.EnsureCanViewAsync(postId,userId,cancellationToken);
        return await BuildResponseAsync(postId, userId, cancellationToken);
    }

    public async Task<PagedResponse<PublicUserResponse>> GetUsersAsync(Guid postId,Guid? viewerId,int page,int pageSize,CancellationToken ct)
    {
        await postAccess.EnsureCanViewAsync(postId,viewerId,ct);page=Math.Max(1,page);pageSize=Math.Clamp(pageSize,1,100);
        var q=dbContext.PostReactions.AsNoTracking().Where(x=>x.PostId==postId
            && !dbContext.CommunityBans.Any(b=>b.CommunityId==x.Post.CommunityId&&b.UserId==x.UserId&&b.CreatedAt>=x.CreatedAt)).Include(x=>x.User).ThenInclude(x=>x.Profile).OrderBy(x=>x.User.Username);
        var count=await q.CountAsync(ct);var users=await q.Skip((page-1)*pageSize).Take(pageSize).Select(x=>x.User).ToListAsync(ct);
        return new(){Items=users.Select(x=>new PublicUserResponse(x.Id,x.Username,x.Profile?.DisplayName,x.Profile?.Bio,x.Profile?.AvatarUrl,x.CreatedAt)).ToArray(),Page=page,PageSize=pageSize,TotalCount=count};
    }

    private async Task EnsureActivePostAsync(Guid postId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Posts.AnyAsync(
                post => post.Id == postId && post.Status == "ACTIVE",
                cancellationToken))
            throw new NotFoundException("Post", postId);
    }

    private async Task<PostReactionResponse> BuildResponseAsync(
        Guid postId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var reactions = await dbContext.PostReactions
            .AsNoTracking()
            .Where(reaction => reaction.PostId == postId
                && !dbContext.CommunityBans.Any(b=>b.CommunityId==reaction.Post.CommunityId&&b.UserId==reaction.UserId&&b.CreatedAt>=reaction.CreatedAt))
            .ToListAsync(cancellationToken);

        var counts = reactions
            .GroupBy(reaction => reaction.Type)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var currentReaction = userId is null
            ? null
            : reactions.FirstOrDefault(reaction => reaction.UserId == userId.Value)?.Type;

        return new PostReactionResponse(postId, currentReaction, counts, reactions.Count);
    }

}
