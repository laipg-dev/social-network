using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;

namespace SocialNetwork.Application.Content.Posts.Interfaces;

public interface IPostService
{
    Task<PostResponse> CreateAsync(
        Guid authorId,
        CreatePostRequest request,
        CancellationToken cancellationToken = default);

    Task<PostPageResponse> GetPageAsync(Guid? communityId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PostResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostResponse> UpdateAsync(Guid id, Guid authorId, UpdatePostRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default);
}