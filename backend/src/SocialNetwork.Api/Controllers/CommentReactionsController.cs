using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Reactions.Interfaces;
using SocialNetwork.Application.Content.Reactions.Requests;
using SocialNetwork.Application.Content.Reactions.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/comments/{commentId:guid}/reaction")]
public sealed class CommentReactionsController(
    ICommentReactionService reactionService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CommentReactionResponse>> Get(
        Guid commentId,
        CancellationToken cancellationToken)
    {
        Guid? userId = currentUser.IsAuthenticated ? currentUser.UserId : null;
        return Ok(await reactionService.GetAsync(commentId, userId, cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<CommentReactionResponse>> Set(
        Guid commentId,
        SetPostReactionRequest request,
        CancellationToken cancellationToken) =>
        Ok(await reactionService.SetAsync(commentId, currentUser.UserId, request, cancellationToken));

    [HttpDelete]
    public async Task<ActionResult<CommentReactionResponse>> Remove(
        Guid commentId,
        CancellationToken cancellationToken) =>
        Ok(await reactionService.RemoveAsync(commentId, currentUser.UserId, cancellationToken));
}
