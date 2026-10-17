using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Identity.Profile.Interfaces;
using SocialNetwork.Application.Identity.Profile.Requests;
using SocialNetwork.Application.Identity.Profile.Responses;
using SocialNetwork.Application.Common.Storage;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController(IProfileService profileService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await profileService.GetAsync(cancellationToken));

    [HttpPut]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfileResponse>> Update(
        UpdateProfileRequest request,
        CancellationToken cancellationToken) =>
        Ok(await profileService.UpdateAsync(request, cancellationToken));

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileResponse>> UploadAvatar(
        IFormFile avatar,
        CancellationToken cancellationToken)
    {
        if (avatar.Length == 0)
            return BadRequest("Avatar file cannot be empty.");
        if (avatar.Length > 5 * 1024 * 1024)
            return BadRequest("Avatar file must be 5 MB or smaller.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(avatar.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Avatar must be a JPG, PNG, WEBP, or GIF image.");

        await using var stream = avatar.OpenReadStream();
        return Ok(await profileService.UploadAvatarAsync(
            new FileUploadRequest(stream, avatar.FileName, avatar.ContentType, avatar.Length),
            cancellationToken));
    }

    [HttpPost("cover")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileResponse>> UploadCover(
        IFormFile cover,
        CancellationToken cancellationToken)
    {
        if (cover.Length == 0)
            return BadRequest("Cover file cannot be empty.");
        if (cover.Length > 10 * 1024 * 1024)
            return BadRequest("Cover file must be 10 MB or smaller.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(cover.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Cover must be a JPG, PNG, WEBP, or GIF image.");

        await using var stream = cover.OpenReadStream();
        return Ok(await profileService.UploadCoverAsync(
            new FileUploadRequest(stream, cover.FileName, cover.ContentType, cover.Length),
            cancellationToken));
    }
}
