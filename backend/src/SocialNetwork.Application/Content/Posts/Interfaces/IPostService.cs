using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;

namespace SocialNetwork.Application.Content.Posts.Interfaces;

public interface IPostService
{
    Task<PostResponse> CreateAsync(
        Guid authorId,
        CreatePostRequest request,
        CancellationToken cancellationToken = default);
}