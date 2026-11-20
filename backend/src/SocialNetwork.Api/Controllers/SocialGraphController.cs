using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Identity.SocialGraph;

namespace SocialNetwork.Api.Controllers;

[ApiController, Route("api/users"), Authorize]
public sealed class SocialGraphController(ISocialGraphService service, ICurrentUser current) : ControllerBase
{
    [AllowAnonymous, HttpGet("{userId:guid}/profile")]
    public async Task<IActionResult> Profile(Guid userId,CancellationToken ct)=>Ok(await service.GetPublicProfileAsync(userId,current.IsAuthenticated?current.UserId:null,ct));
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery]string? q,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.SearchAsync(q,page,pageSize,current.UserId,ct));
    [HttpPut("{userId:guid}/follow")]
    public async Task<IActionResult> Follow(Guid userId,CancellationToken ct){await service.FollowAsync(current.UserId,userId,ct);return NoContent();}
    [HttpDelete("{userId:guid}/follow")]
    public async Task<IActionResult> Unfollow(Guid userId,CancellationToken ct){await service.UnfollowAsync(current.UserId,userId,ct);return NoContent();}
    [HttpGet("{userId:guid}/followers")]
    public async Task<IActionResult> Followers(Guid userId,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetFollowersAsync(userId,page,pageSize,current.UserId,ct));
    [HttpGet("{userId:guid}/following")]
    public async Task<IActionResult> Following(Guid userId,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetFollowingAsync(userId,page,pageSize,current.UserId,ct));
    [HttpGet("{userId:guid}/follow-status")]
    public async Task<IActionResult> FollowStatus(Guid userId,CancellationToken ct)=>Ok(await service.GetFollowStatusAsync(current.UserId,userId,ct));
    [HttpGet("{userId:guid}/friendship-status")]
    public async Task<IActionResult> FriendshipStatus(Guid userId,CancellationToken ct)=>Ok(await service.GetFriendshipStatusAsync(current.UserId,userId,ct));
    [HttpPost("{userId:guid}/friend-requests")]
    public async Task<IActionResult> SendRequest(Guid userId,CancellationToken ct)=>StatusCode(201,await service.SendFriendRequestAsync(current.UserId,userId,ct));
    [HttpPost("friend-requests/{requestId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid requestId,CancellationToken ct)=>Ok(await service.RespondFriendRequestAsync(current.UserId,requestId,true,ct));
    [HttpPost("friend-requests/{requestId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid requestId,CancellationToken ct)=>Ok(await service.RespondFriendRequestAsync(current.UserId,requestId,false,ct));
    [HttpDelete("friend-requests/{requestId:guid}")]
    public async Task<IActionResult> Cancel(Guid requestId,CancellationToken ct){await service.CancelFriendRequestAsync(current.UserId,requestId,ct);return NoContent();}
    [HttpDelete("{userId:guid}/friendship")]
    public async Task<IActionResult> RemoveFriend(Guid userId,CancellationToken ct){await service.RemoveFriendAsync(current.UserId,userId,ct);return NoContent();}
    [HttpGet("{userId:guid}/friends")]
    public async Task<IActionResult> Friends(Guid userId,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetFriendsAsync(userId,page,pageSize,current.UserId,ct));
    [HttpGet("me/friend-requests/incoming")]
    public async Task<IActionResult> Incoming([FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetFriendRequestsAsync(current.UserId,true,page,pageSize,ct));
    [HttpGet("me/friend-requests/outgoing")]
    public async Task<IActionResult> Outgoing([FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetFriendRequestsAsync(current.UserId,false,page,pageSize,ct));
    [HttpPut("{userId:guid}/block")]
    public async Task<IActionResult> Block(Guid userId,CancellationToken ct){await service.BlockAsync(current.UserId,userId,ct);return NoContent();}
    [HttpDelete("{userId:guid}/block")]
    public async Task<IActionResult> Unblock(Guid userId,CancellationToken ct){await service.UnblockAsync(current.UserId,userId,ct);return NoContent();}
    [HttpGet("me/blocked")]
    public async Task<IActionResult> Blocked([FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetBlockedAsync(current.UserId,page,pageSize,ct));
}
