using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Application.Common.Exceptions;

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
            Visibility = request.Visibility.Trim().ToUpperInvariant(),
            CommunityId = request.CommunityId
        };

        await _postRepository.AddAsync(
            post,
            request.MediaIds,
            cancellationToken);

        var createdPost = await _postRepository.GetByIdAsync(post.Id, cancellationToken) ?? post;
        return Map(createdPost);
    }

    public async Task<PostPageResponse> GetPageAsync(
        Guid? communityId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var posts = await _postRepository.GetPageAsync(communityId, page, pageSize, cancellationToken);
        var totalCount = await _postRepository.CountAsync(communityId, cancellationToken);
        return new(posts.Select(Map).ToList(), page, pageSize, totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PostResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Map(await _postRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Post", id));

    public async Task<PostResponse> UpdateAsync(
        Guid id,
        Guid authorId,
        UpdatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Post", id);
        EnsureAuthor(post, authorId);
        post.Content = request.Content;
        post.Visibility = request.Visibility.Trim().ToUpperInvariant();
        post.UpdatedAt = DateTime.UtcNow;
        await _postRepository.SaveChangesAsync(cancellationToken);
        return Map(post);
    }

    public async Task DeleteAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Post", id);
        EnsureAuthor(post, authorId);
        post.Status = "DELETED";
        post.DeletedAt = DateTime.UtcNow;
        await _postRepository.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureAuthor(Post post, Guid authorId)
    {
        if (post.AuthorId != authorId)
            throw new ForbiddenException("Only the post author can modify this post.");
    }

    private static PostResponse Map(Post post) => new()
    {
        Id = post.Id,
        Content = post.Content,
        Visibility = post.Visibility,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt,
        AuthorId = post.AuthorId,
        AuthorUsername = post.Author?.Username,
        AuthorDisplayName = post.Author?.Profile?.DisplayName,
        AuthorAvatarUrl = post.Author?.Profile?.AvatarUrl,
        CommunityId = post.CommunityId,
        Status = post.Status,
        Media = post.Media
            .OrderBy(m => m.Position)
            .Select(m => new SocialNetwork.Application.Content.Media.Responses.PostMediaResponse(
                m.Id, m.PostId, m.Url, m.MediaType, m.Position, m.CreatedAt))
            .ToList()
    };
}