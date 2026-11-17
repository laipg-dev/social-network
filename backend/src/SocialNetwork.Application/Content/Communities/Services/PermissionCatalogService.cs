using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Services;

public class PermissionCatalogService : IPermissionCatalogService
{
    private readonly IPermissionCatalogRepository _permissionRepository;

    public PermissionCatalogService(
        IPermissionCatalogRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<IReadOnlyList<PermissionResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(
            cancellationToken);

        return permissions
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .Select(x => new PermissionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description
            })
            .ToArray();
    }
}
