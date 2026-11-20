using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/communities/{communityId:guid}/rooms/{roomId:guid}/members")]
public class RoomMembersController : ControllerBase
{
    private readonly IRoomMemberService _roomMemberService;
    private readonly ICurrentUser _currentUser;

    public RoomMembersController(
        IRoomMemberService roomMemberService,
        ICurrentUser currentUser)
    {
        _roomMemberService = roomMemberService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        var result = await _roomMemberService.GetMembersAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        Guid communityId,
        Guid roomId,
        [FromBody] AddRoomMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roomMemberService.AddAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Remove(
        Guid communityId,
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _roomMemberService.RemoveAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            userId,
            cancellationToken);

        return NoContent();
    }
}