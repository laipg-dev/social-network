using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/communities/{communityId:guid}/members")]
public class CommunityMembersController : ControllerBase
{
    private readonly ICommunityMemberService _memberService;
    private readonly ICommunityRoleService _roleService;
    private readonly ICurrentUser _currentUser;

    public CommunityMembersController(
        ICommunityMemberService memberService,
        ICommunityRoleService roleService,
        ICurrentUser currentUser)
    {
        _memberService = memberService;
        _roleService = roleService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        Guid communityId,
        [FromQuery] CommunityMemberQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _memberService.GetPagedAsync(
            _currentUser.UserId,
            communityId,
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(
        Guid communityId,
        CancellationToken cancellationToken)
    {
        var result = await _memberService.GetMeAsync(
            _currentUser.UserId,
            communityId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetMember(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _memberService.GetMemberAsync(
            _currentUser.UserId,
            communityId,
            userId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join(
        Guid communityId,
        CancellationToken cancellationToken)
    {
        var result = await _memberService.JoinAsync(
            _currentUser.UserId,
            communityId,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> Leave(
        Guid communityId,
        CancellationToken cancellationToken)
    {
        await _memberService.LeaveAsync(
            _currentUser.UserId,
            communityId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{userId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _memberService.ApproveAsync(
            _currentUser.UserId,
            communityId,
            userId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{userId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _memberService.RejectAsync(
            _currentUser.UserId,
            communityId,
            userId,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Kick(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _memberService.KickAsync(
            _currentUser.UserId,
            communityId,
            userId,
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{userId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid communityId,
        Guid userId,
        [FromBody] UpdateCommunityMemberStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _memberService.UpdateStatusAsync(
            _currentUser.UserId,
            communityId,
            userId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{userId:guid}/roles")]
    public async Task<IActionResult> AssignRoles(
        Guid communityId,
        Guid userId,
        [FromBody] UpdateMemberRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.AssignMemberRolesAsync(
            _currentUser.UserId,
            communityId,
            userId,
            request,
            cancellationToken);

        return Ok(result);
    }
}
