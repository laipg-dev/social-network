using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/communities")]
public sealed class CommunitiesController(
    ICommunityService communityService,
    ICommunityMemberService memberService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaged(
        [FromQuery] CommunityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.IsAuthenticated
            ? currentUser.UserId
            : Guid.Empty;
        return Ok(await communityService.GetPagedAsync(
            userId,
            parameters,
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.IsAuthenticated
            ? currentUser.UserId
            : Guid.Empty;
        return Ok(await communityService.GetByIdAsync(
            userId,
            id,
            cancellationToken));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateCommunityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await communityService.CreateAsync(
            currentUser.UserId,
            request,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCommunityRequest request,
        CancellationToken cancellationToken) =>
        Ok(await communityService.UpdateAsync(
            currentUser.UserId,
            id,
            request,
            cancellationToken));

    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateCommunityStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await communityService.UpdateStatusAsync(
            currentUser.UserId,
            id,
            request,
            cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await communityService.DeleteAsync(
            currentUser.UserId,
            id,
            cancellationToken);
        return NoContent();
    }

    // Keep the hip membership routes working while clients migrate to
    // /members/join and /members/me.
    [HttpPut("{id:guid}/membership")]
    [Authorize]
    public async Task<IActionResult> Join(
        Guid id,
        CancellationToken cancellationToken)
    {
        await memberService.JoinAsync(currentUser.UserId, id, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/membership")]
    [Authorize]
    public async Task<IActionResult> Leave(
        Guid id,
        CancellationToken cancellationToken)
    {
        await memberService.LeaveAsync(currentUser.UserId, id, cancellationToken);
        return NoContent();
    }
}
