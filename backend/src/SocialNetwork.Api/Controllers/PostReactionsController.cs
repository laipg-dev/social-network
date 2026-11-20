using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Reactions.Interfaces;
using SocialNetwork.Application.Content.Reactions.Requests;
using SocialNetwork.Application.Content.Reactions.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:guid}/reaction")]
public sealed class PostReactionsController(
    IPostReactionService reactionService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PostReactionResponse>> Get(
        Guid postId,
        CancellationToken cancellationToken)
    {
        Guid? userId = currentUser.IsAuthenticated ? currentUser.UserId : null;
        return Ok(await reactionService.GetAsync(postId, userId, cancellationToken));
    }

    [AllowAnonymous]
    [HttpGet("users")]
    public async Task<IActionResult> Users(Guid postId,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await reactionService.GetUsersAsync(postId,currentUser.IsAuthenticated?currentUser.UserId:null,page,pageSize,ct));

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<PostReactionResponse>> Set(
        Guid postId,
        SetPostReactionRequest request,
        CancellationToken cancellationToken) =>
        Ok(await reactionService.SetAsync(postId, currentUser.UserId, request, cancellationToken));

    [Authorize]
    [HttpDelete]
    public async Task<ActionResult<PostReactionResponse>> Remove(
        Guid postId,
        CancellationToken cancellationToken) =>
        Ok(await reactionService.RemoveAsync(postId, currentUser.UserId, cancellationToken));
}
