using SocialNetwork.Domain.Content.Entities;

namespace SocialNetwork.Application.Content.Posts.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Post post,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Post>> GetPageAsync(Guid? communityId, int page, int pageSize, Guid? viewerId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? communityId, Guid? viewerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByUserAsync(Guid userId, int page, int pageSize, Guid? viewerId, CancellationToken ct = default);
    Task<int> CountByUserAsync(Guid userId, Guid? viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<Post>> SearchAsync(string query, int page, int pageSize, Guid? viewerId, CancellationToken ct = default);
    Task<int> CountSearchAsync(string query, Guid? viewerId, CancellationToken ct = default);
    Task<bool> UserExistsAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
