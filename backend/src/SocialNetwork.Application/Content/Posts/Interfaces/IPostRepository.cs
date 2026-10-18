using SocialNetwork.Domain.Content.Entities;

namespace SocialNetwork.Application.Content.Posts.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Post post,
        IReadOnlyList<Guid>? mediaIds = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Post>> GetPageAsync(Guid? communityId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? communityId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}