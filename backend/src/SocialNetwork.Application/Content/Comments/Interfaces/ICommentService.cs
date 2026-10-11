using SocialNetwork.Application.Content.Comments.Requests;
using SocialNetwork.Application.Content.Comments.Responses;

namespace SocialNetwork.Application.Content.Comments.Interfaces;

public interface ICommentService
{
    Task<CommentResponse> CreateAsync(Guid postId, Guid authorId, CreateCommentRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommentResponse>> GetForPostAsync(Guid postId, CancellationToken cancellationToken);
    Task<CommentResponse> UpdateAsync(Guid id, Guid authorId, string content, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid authorId, CancellationToken cancellationToken);
}
