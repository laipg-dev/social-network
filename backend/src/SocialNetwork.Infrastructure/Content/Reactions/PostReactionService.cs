using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Reactions.Interfaces;
using SocialNetwork.Application.Content.Reactions.Requests;
using SocialNetwork.Application.Content.Reactions.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Content.Reactions;

public sealed class PostReactionService(AppDbContext dbContext) : IPostReactionService
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

        await EnsureActivePostAsync(postId, cancellationToken);

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
        await EnsureActivePostAsync(postId, cancellationToken);

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
        await EnsureActivePostAsync(postId, cancellationToken);
        return await BuildResponseAsync(postId, userId, cancellationToken);
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
            .Where(reaction => reaction.PostId == postId)
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
