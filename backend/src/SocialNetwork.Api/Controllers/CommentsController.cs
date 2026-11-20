using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Comments.Interfaces;
using SocialNetwork.Application.Content.Comments.Requests;
using SocialNetwork.Application.Content.Comments.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class CommentsController(ICommentService commentService, ICurrentUser currentUser) : ControllerBase
{
    [Authorize]
    [HttpPost("posts/{postId:guid}/comments")]
    public async Task<ActionResult<CommentResponse>> Create(
        Guid postId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commentService.CreateAsync(postId, currentUser.UserId, request, cancellationToken);
        return Created($"/api/comments/{result.Id}", result);
    }

    [HttpGet("posts/{postId:guid}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForPost(
        Guid postId,
        [FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken cancellationToken=default) =>
        Ok(await commentService.GetForPostAsync(postId,currentUser.IsAuthenticated?currentUser.UserId:null,page,pageSize,cancellationToken));

    [Authorize]
    [HttpPut("comments/{id:guid}")]
    public async Task<ActionResult<CommentResponse>> Update(
        Guid id,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await commentService.UpdateAsync(id, currentUser.UserId, request.Content, cancellationToken));

    [Authorize]
    [HttpDelete("comments/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await commentService.DeleteAsync(id, currentUser.UserId, cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateCommentRequest(string Content);
