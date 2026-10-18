using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Interfaces;
using SocialNetwork.Application.Content.Media.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/media")]
public sealed class MediaController(
    IMediaService mediaService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<MediaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaResponse>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await mediaService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<MediaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaResponse>> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var uploadRequest = new FileUploadRequest(stream, file.FileName, file.ContentType, file.Length);
        var result = await mediaService.UploadAsync(currentUser.UserId, uploadRequest, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediaService.DeleteAsync(id, currentUser.UserId, cancellationToken);
        return NoContent();
    }
}
