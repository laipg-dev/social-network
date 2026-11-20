using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Communities.Interfaces;

namespace SocialNetwork.Api.Controllers;

[ApiController,Route("api/communities/{communityId:guid}/bans"),Authorize]
public sealed class CommunityBansController(ICommunityBanService service,ICurrentUser current):ControllerBase
{
    [HttpPost("{userId:guid}")]
    public async Task<IActionResult> Ban(Guid communityId,Guid userId,BanCommunityMemberRequest request,CancellationToken ct)=>Ok(await service.BanAsync(current.UserId,communityId,userId,request,ct));
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Unban(Guid communityId,Guid userId,CancellationToken ct){await service.UnbanAsync(current.UserId,communityId,userId,ct);return NoContent();}
    [HttpGet]
    public async Task<IActionResult> Get(Guid communityId,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetBannedAsync(current.UserId,communityId,page,pageSize,ct));
    [HttpGet("~/api/communities/{communityId:guid}/moderation/quarantined-posts")]
    public async Task<IActionResult> GetQuarantined(Guid communityId,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await service.GetQuarantinedPostsAsync(current.UserId,communityId,page,pageSize,new Uri($"{Request.Scheme}://{Request.Host}"),ct));
    [HttpPost("~/api/communities/{communityId:guid}/moderation/quarantined-posts/{postId:guid}/restore")]
    public async Task<IActionResult> Restore(Guid communityId,Guid postId,CancellationToken ct){await service.RestorePostAsync(current.UserId,communityId,postId,ct);return NoContent();}
    [HttpGet("~/api/communities/{communityId:guid}/moderation/quarantined-comments")]
    public async Task<IActionResult> GetQuarantinedComments(Guid communityId,[FromQuery]int page=1,[FromQuery]int pageSize=50,CancellationToken ct=default)=>Ok(await service.GetQuarantinedCommentsAsync(current.UserId,communityId,page,pageSize,ct));
    [HttpPost("~/api/communities/{communityId:guid}/moderation/quarantined-comments/{commentId:guid}/restore")]
    public async Task<IActionResult> RestoreComment(Guid communityId,Guid commentId,CancellationToken ct){await service.RestoreCommentAsync(current.UserId,communityId,commentId,ct);return NoContent();}
}
