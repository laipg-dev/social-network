using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Communities.Interfaces;
using SocialNetwork.Application.Communities.Requests;
using SocialNetwork.Application.Communities.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/communities")]
public sealed class CommunitiesController(
    ICommunityService communityService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CommunityResponse>>> Get(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        Guid? userId = currentUser.IsAuthenticated ? currentUser.UserId : null;
        return Ok(await communityService.GetPageAsync(userId, search, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CommunityResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid? userId = currentUser.IsAuthenticated ? currentUser.UserId : null;
        return Ok(await communityService.GetByIdAsync(id, userId, cancellationToken));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommunityResponse>> Create(
        CreateCommunityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await communityService.CreateAsync(
            currentUser.UserId, request, cancellationToken);
        return Created($"/api/communities/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CommunityResponse>> Update(
        Guid id,
        UpdateCommunityRequest request,
        CancellationToken cancellationToken) =>
        Ok(await communityService.UpdateAsync(
            id, currentUser.UserId, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await communityService.DeleteAsync(id, currentUser.UserId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/membership")]
    [Authorize]
    public async Task<IActionResult> Join(
        Guid id,
        CancellationToken cancellationToken)
    {
        await communityService.JoinAsync(id, currentUser.UserId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/membership")]
    [Authorize]
    public async Task<IActionResult> Leave(
        Guid id,
        CancellationToken cancellationToken)
    {
        await communityService.LeaveAsync(id, currentUser.UserId, cancellationToken);
        return NoContent();
    }
}
