using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/communities/{communityId:guid}/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ICurrentUser _currentUser;

    public RoomsController(
        IRoomService roomService,
        ICurrentUser currentUser)
    {
        _roomService = roomService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms(
        Guid communityId,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.GetRoomsAsync(
            _currentUser.UserId,
            communityId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{roomId:guid}")]
    public async Task<IActionResult> GetRoom(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.GetRoomAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid communityId,
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.CreateAsync(
            _currentUser.UserId,
            communityId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetRoom),
            new { communityId, roomId = result.Id },
            result);
    }

    [HttpPut("{roomId:guid}")]
    public async Task<IActionResult> Update(
        Guid communityId,
        Guid roomId,
        [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.UpdateAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{roomId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid communityId,
        Guid roomId,
        [FromBody] UpdateRoomStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.UpdateStatusAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> Delete(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        await _roomService.DeleteAsync(
            _currentUser.UserId,
            communityId,
            roomId,
            cancellationToken);

        return NoContent();
    }

    [HttpPut("order")]
    public async Task<IActionResult> Reorder(
        Guid communityId,
        [FromBody] ReorderRoomsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.ReorderAsync(
            _currentUser.UserId,
            communityId,
            request,
            cancellationToken);

        return Ok(result);
    }
}
