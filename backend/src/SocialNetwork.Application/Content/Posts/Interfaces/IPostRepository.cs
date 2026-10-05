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
}