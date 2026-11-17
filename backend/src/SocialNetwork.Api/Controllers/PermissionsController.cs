using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Content.Communities.Interfaces;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionCatalogService _permissionService;

    public PermissionsController(
        IPermissionCatalogService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _permissionService.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }
}
