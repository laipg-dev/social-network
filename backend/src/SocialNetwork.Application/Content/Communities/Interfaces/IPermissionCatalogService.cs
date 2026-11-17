using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IPermissionCatalogService
{
    Task<IReadOnlyList<PermissionResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
