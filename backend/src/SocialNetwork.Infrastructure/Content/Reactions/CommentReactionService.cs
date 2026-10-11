using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Reactions.Interfaces;
using SocialNetwork.Application.Content.Reactions.Requests;
using SocialNetwork.Application.Content.Reactions.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Content.Reactions;

public sealed class CommentReactionService(AppDbContext dbContext) : ICommentReactionService
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

        await EnsureActiveCommentAsync(commentId, cancellationToken);

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
        await EnsureActiveCommentAsync(commentId, cancellationToken);

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
        await EnsureActiveCommentAsync(commentId, cancellationToken);
        return await BuildResponseAsync(commentId, userId, cancellationToken);
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
            .Where(reaction => reaction.CommentId == commentId)
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
