using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Content.Bookmarks.Interfaces;
using SocialNetwork.Application.Content.Posts.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class BookmarksController(
    IBookmarkService bookmarkService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("bookmarks")]
    public async Task<ActionResult<PostPageResponse>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await bookmarkService.GetPageAsync(
            currentUser.UserId,
            page,
            pageSize,
            cancellationToken));

    [HttpGet("posts/{postId:guid}/bookmark")]
    public async Task<ActionResult<bool>> IsSaved(
        Guid postId,
        CancellationToken cancellationToken) =>
        Ok(await bookmarkService.IsSavedAsync(
            postId,
            currentUser.UserId,
            cancellationToken));

    [HttpPut("posts/{postId:guid}/bookmark")]
    public async Task<ActionResult<bool>> Save(
        Guid postId,
        CancellationToken cancellationToken) =>
        Ok(await bookmarkService.SaveAsync(
            postId,
            currentUser.UserId,
            cancellationToken));

    [HttpDelete("posts/{postId:guid}/bookmark")]
    public async Task<ActionResult<bool>> Remove(
        Guid postId,
        CancellationToken cancellationToken) =>
        Ok(await bookmarkService.RemoveAsync(
            postId,
            currentUser.UserId,
            cancellationToken));
}
