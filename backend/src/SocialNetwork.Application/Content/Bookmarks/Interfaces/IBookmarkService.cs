using SocialNetwork.Application.Content.Posts.Responses;

namespace SocialNetwork.Application.Content.Bookmarks.Interfaces;

public interface IBookmarkService
{
    Task<bool> SaveAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
    Task<bool> IsSavedAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
    Task<PostPageResponse> GetPageAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
