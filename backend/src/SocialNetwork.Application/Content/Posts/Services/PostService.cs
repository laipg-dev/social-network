using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities.Interfaces;

namespace SocialNetwork.Application.Content.Posts.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IPostAccessService _postAccess;
    private readonly ICommunityAuthorization _communityAuthorization;

    public PostService(IPostRepository postRepository, IPostAccessService postAccess, ICommunityAuthorization communityAuthorization)
    {
        _postRepository = postRepository;
        _postAccess = postAccess;
        _communityAuthorization = communityAuthorization;
    }

    public async Task<PostResponse> CreateAsync(
        Guid authorId,
        CreatePostRequest request,
        CancellationToken cancellationToken = default,
        bool hasMedia = false)
    {
        if (string.IsNullOrWhiteSpace(request.Content) && !hasMedia)
            throw new ValidationException(new Dictionary<string, string[]> { ["content"] = ["Post content cannot be empty."] });
        var visibility = NormalizeVisibility(request.Visibility);
        if (request.CommunityId is Guid communityId)
        {
            var access = await _communityAuthorization.GetAccessAsync(communityId, authorId, cancellationToken);
            if (access.IsBanned || !access.IsMember)
                throw new ForbiddenException("Only active, non-banned community members can create community posts.");
        }
        var post = new Post
        {
            AuthorId = authorId,
            Content = request.Content,
            Visibility = visibility,
            CommunityId = request.CommunityId
        };

        await _postRepository.AddAsync(
            post,
            cancellationToken);

        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            Visibility = post.Visibility,
            CreatedAt = post.CreatedAt,
            AuthorId = post.AuthorId,
            AuthorUsername = post.Author?.Username,
            AuthorDisplayName = post.Author?.Profile?.DisplayName,
            AuthorAvatarUrl = post.Author?.Profile?.AvatarUrl,
            CommunityId = post.CommunityId,
            Status = post.Status
        };
    }

    public async Task<PostPageResponse> GetPageAsync(
        Guid? communityId,
        int page,
        int pageSize,
        Guid? viewerId,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var posts = await _postRepository.GetPageAsync(communityId, page, pageSize, viewerId, cancellationToken);
        var allowed=new List<Post>(); foreach(var post in posts){try{await _postAccess.EnsureCanViewAsync(post.Id,viewerId,cancellationToken);allowed.Add(post);}catch(NotFoundException){}}
        var totalCount = await _postRepository.CountAsync(communityId, viewerId, cancellationToken);
        return new(allowed.Select(Map).ToList(), page, pageSize, totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PostResponse> GetByIdAsync(Guid id, Guid? viewerId = null, CancellationToken cancellationToken = default)
    { await _postAccess.EnsureCanViewAsync(id,viewerId,cancellationToken); return Map(await _postRepository.GetByIdAsync(id,cancellationToken) ?? throw new NotFoundException("Post",id)); }

    public async Task<PostPageResponse> GetByUserAsync(Guid userId,Guid? viewerId,int page,int pageSize,CancellationToken ct=default)
    {
        if(!await _postRepository.UserExistsAsync(userId,ct))throw new NotFoundException("User",userId);
        page=Math.Max(page,1);pageSize=Math.Clamp(pageSize,1,50);var all=await _postRepository.GetByUserAsync(userId,page,pageSize,viewerId,ct);var posts=new List<Post>();
        foreach(var post in all){try{await _postAccess.EnsureCanViewAsync(post.Id,viewerId,ct);posts.Add(post);}catch(NotFoundException){}}
        var total=await _postRepository.CountByUserAsync(userId,viewerId,ct);return new(posts.Select(Map).ToArray(),page,pageSize,total,(int)Math.Ceiling(total/(double)pageSize));
    }
    public async Task<PostPageResponse> SearchAsync(string query,Guid? viewerId,int page,int pageSize,CancellationToken ct=default)
    {
        if(string.IsNullOrWhiteSpace(query))throw new ValidationException(new Dictionary<string,string[]>{{"query",["Search text is required."]}});
        page=Math.Max(page,1);pageSize=Math.Clamp(pageSize,1,50);var all=await _postRepository.SearchAsync(query.Trim(),page,pageSize,viewerId,ct);var posts=new List<Post>();
        foreach(var post in all){try{await _postAccess.EnsureCanViewAsync(post.Id,viewerId,ct);posts.Add(post);}catch(NotFoundException){}}
        var total=await _postRepository.CountSearchAsync(query.Trim(),viewerId,ct);return new(posts.Select(Map).ToArray(),page,pageSize,total,(int)Math.Ceiling(total/(double)pageSize));
    }

    public async Task<PostResponse> UpdateAsync(
        Guid id,
        Guid authorId,
        UpdatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Post", id);
        if (post.Status != "ACTIVE") throw new NotFoundException("Post", id);
        EnsureAuthor(post, authorId);
        post.Content = request.Content;
        post.Visibility = NormalizeVisibility(request.Visibility);
        post.UpdatedAt = DateTime.UtcNow;
        await _postRepository.SaveChangesAsync(cancellationToken);
        return Map(post);
    }

    public async Task DeleteAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Post", id);
        EnsureAuthor(post, authorId);
        if (post.Status == "DELETED") return;
        post.Status = "DELETED";
        post.DeletedAt = DateTime.UtcNow;
        await _postRepository.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureAuthor(Post post, Guid authorId)
    {
        if (post.AuthorId != authorId)
            throw new ForbiddenException("Only the post author can modify this post.");
    }

    private static string NormalizeVisibility(string? visibility)
    {
        var value = visibility?.Trim().ToUpperInvariant();
        if (value is not ("PUBLIC" or "FRIENDS" or "PRIVATE"))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["visibility"] = ["Visibility must be PUBLIC, FRIENDS, or PRIVATE."]
            });
        return value;
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
        Status = post.Status
    };
}
