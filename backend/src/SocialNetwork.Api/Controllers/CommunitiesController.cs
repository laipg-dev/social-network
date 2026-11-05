using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/communities")]
public class CommunitiesController : ControllerBase
{
    private readonly ICommunityService _communityService;
    private readonly ICurrentUser _currentUser;

    public CommunitiesController(
        ICommunityService communityService,
        ICurrentUser currentUser)
    {
        _communityService = communityService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCommunityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _communityService.CreateAsync(
            _currentUser.UserId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] CommunityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _communityService.GetPagedAsync(
            _currentUser.UserId,
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _communityService.GetByIdAsync(
            _currentUser.UserId,
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCommunityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _communityService.UpdateAsync(
            _currentUser.UserId,
            id,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateCommunityStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _communityService.UpdateStatusAsync(
            _currentUser.UserId,
            id,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _communityService.DeleteAsync(
            _currentUser.UserId,
            id,
            cancellationToken);

        return NoContent();
    }
}
