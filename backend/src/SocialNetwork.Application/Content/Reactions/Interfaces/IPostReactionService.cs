using SocialNetwork.Application.Content.Reactions.Requests;
using SocialNetwork.Application.Content.Reactions.Responses;

namespace SocialNetwork.Application.Content.Reactions.Interfaces;

public interface IPostReactionService
{
    Task<PostReactionResponse> SetAsync(
        Guid postId,
        Guid userId,
        SetPostReactionRequest request,
        CancellationToken cancellationToken);

    Task<PostReactionResponse> RemoveAsync(
        Guid postId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<PostReactionResponse> GetAsync(
        Guid postId,
        Guid? userId,
        CancellationToken cancellationToken);
}
