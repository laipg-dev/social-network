using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Identity.Settings.Interfaces;
using SocialNetwork.Application.Identity.Settings.Requests;
using SocialNetwork.Application.Identity.Settings.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/users/me/settings")]
[Authorize]
public sealed class UserSettingsController(
    IUserSettingsService settingsService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<UserSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserSettingsResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await settingsService.GetAsync(currentUser.UserId, cancellationToken));

    [HttpPut]
    [ProducesResponseType<UserSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserSettingsResponse>> Update(
        UpdateUserSettingsRequest request,
        CancellationToken cancellationToken) =>
        Ok(await settingsService.UpdateAsync(currentUser.UserId, request, cancellationToken));
}
