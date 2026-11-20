using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Interfaces;
using SocialNetwork.Application.Content.Media.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:guid}/media")]
public sealed class PostMediaController(
    IPostMediaService mediaService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<PostMediaResponse>>> Get(
        Guid postId,
        CancellationToken cancellationToken) =>
        Ok(await mediaService.GetForPostAsync(postId,currentUser.IsAuthenticated?currentUser.UserId:null,new Uri($"{Request.Scheme}://{Request.Host}"),cancellationToken));

    [HttpGet("{mediaId:guid}/access")]
    [AllowAnonymous]
    public async Task<ActionResult<PostMediaResponse>> Access(Guid postId, Guid mediaId, CancellationToken cancellationToken)
    {
        var response = await mediaService.GetAccessAsync(postId, mediaId, currentUser.IsAuthenticated ? currentUser.UserId : null,
            new Uri($"{Request.Scheme}://{Request.Host}"), cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PostMediaResponse>> Add(
        Guid postId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediaService.AddAsync(
            postId,
            currentUser.UserId,
            new FileUploadRequest(stream, file.FileName, file.ContentType, file.Length),
            cancellationToken);
        return Created($"/api/posts/{postId}/media/{result.Id}", result);
    }

    [Authorize]
    [HttpDelete("{mediaId:guid}")]
    public async Task<IActionResult> Delete(
        Guid postId,
        Guid mediaId,
        CancellationToken cancellationToken)
    {
        await mediaService.DeleteAsync(postId, mediaId, currentUser.UserId, cancellationToken);
        return NoContent();
    }
}
