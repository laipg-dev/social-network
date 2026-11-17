using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/communities/{communityId:guid}/roles")]
public class CommunityRolesController : ControllerBase
{
    private readonly ICommunityRoleService _roleService;
    private readonly ICurrentUser _currentUser;

    public CommunityRolesController(
        ICommunityRoleService roleService,
        ICurrentUser currentUser)
    {
        _roleService = roleService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(
        Guid communityId,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRolesAsync(
            _currentUser.UserId,
            communityId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{roleId:guid}")]
    public async Task<IActionResult> GetRole(
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRoleAsync(
            _currentUser.UserId,
            communityId,
            roleId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid communityId,
        [FromBody] CreateCommunityRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(
            _currentUser.UserId,
            communityId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetRole),
            new { communityId, roleId = result.Id },
            result);
    }

    [HttpPut("{roleId:guid}")]
    public async Task<IActionResult> Update(
        Guid communityId,
        Guid roleId,
        [FromBody] UpdateCommunityRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(
            _currentUser.UserId,
            communityId,
            roleId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> Delete(
        Guid communityId,
        Guid roleId,
        [FromQuery] Guid? reassignToRoleId,
        CancellationToken cancellationToken)
    {
        await _roleService.DeleteAsync(
            _currentUser.UserId,
            communityId,
            roleId,
            reassignToRoleId,
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{roleId:guid}/default")]
    public async Task<IActionResult> SetDefault(
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.SetDefaultAsync(
            _currentUser.UserId,
            communityId,
            roleId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("order")]
    public async Task<IActionResult> Reorder(
        Guid communityId,
        [FromBody] ReorderCommunityRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.ReorderAsync(
            _currentUser.UserId,
            communityId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{roleId:guid}/permissions")]
    public async Task<IActionResult> GetPermissions(
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.GetPermissionsAsync(
            _currentUser.UserId,
            communityId,
            roleId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{roleId:guid}/permissions")]
    public async Task<IActionResult> ReplacePermissions(
        Guid communityId,
        Guid roleId,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.ReplacePermissionsAsync(
            _currentUser.UserId,
            communityId,
            roleId,
            request,
            cancellationToken);

        return Ok(result);
    }
}
