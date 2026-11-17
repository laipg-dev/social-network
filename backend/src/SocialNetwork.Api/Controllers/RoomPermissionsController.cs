using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/communities/{communityId:guid}/rooms/{roomId:guid}")]
public class RoomPermissionsController : ControllerBase
{
    private readonly IRoomPermissionService _roomPermissionService;
    private readonly ICurrentUser _currentUser;

    public RoomPermissionsController(
        IRoomPermissionService roomPermissionService,
        ICurrentUser currentUser)
    {
        _roomPermissionService = roomPermissionService;
        _currentUser = currentUser;
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetOverrides(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        var result = await _roomPermissionService.GetOverridesAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("roles/{roleId:guid}/permissions")]
    public async Task<IActionResult> ReplaceRoleOverrides(
        Guid communityId,
        Guid roomId,
        Guid roleId,
        [FromBody] UpdateRoomRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roomPermissionService.ReplaceRoleOverridesAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            roleId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("roles/{roleId:guid}/permissions/{code}")]
    public async Task<IActionResult> RemoveOverride(
        Guid communityId,
        Guid roomId,
        Guid roleId,
        string code,
        CancellationToken cancellationToken)
    {
        await _roomPermissionService.RemoveOverrideAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            roleId,
            code,
            cancellationToken);

        return NoContent();
    }
}
