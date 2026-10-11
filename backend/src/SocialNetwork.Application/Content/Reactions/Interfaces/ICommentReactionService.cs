using SocialNetwork.Application.Content.Reactions.Requests;
using SocialNetwork.Application.Content.Reactions.Responses;

namespace SocialNetwork.Application.Content.Reactions.Interfaces;

public interface ICommentReactionService
{
    Task<CommentReactionResponse> SetAsync(
        Guid commentId,
        Guid userId,
        SetPostReactionRequest request,
        CancellationToken cancellationToken);

    Task<CommentReactionResponse> RemoveAsync(
        Guid commentId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<CommentReactionResponse> GetAsync(
        Guid commentId,
        Guid? userId,
        CancellationToken cancellationToken);
}
