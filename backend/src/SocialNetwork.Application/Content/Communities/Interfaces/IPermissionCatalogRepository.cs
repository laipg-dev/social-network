using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IPermissionCatalogRepository
{
    Task<IReadOnlyList<Permission>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, Guid>> GetIdsByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken = default);
}
