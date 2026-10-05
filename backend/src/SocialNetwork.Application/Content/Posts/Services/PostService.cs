using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;
using SocialNetwork.Domain.Content.Entities;

namespace SocialNetwork.Application.Content.Posts.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostResponse> CreateAsync(
        Guid authorId,
        CreatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        var post = new Post
        {
            AuthorId = authorId,
            Content = request.Content,
            Visibility = request.Visibility
        };

        await _postRepository.AddAsync(
            post,
            cancellationToken);

        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            Visibility = post.Visibility,
            CreatedAt = post.CreatedAt
        };
    }
}