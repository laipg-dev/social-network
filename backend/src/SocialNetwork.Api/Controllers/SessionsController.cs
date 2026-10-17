using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Identity.Sessions.Interfaces;
using SocialNetwork.Application.Identity.Sessions.Responses;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController(
    ISessionService sessionService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SessionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> GetActiveSessions(
        [FromHeader(Name = "X-Refresh-Token")] string? refreshToken,
        CancellationToken cancellationToken)
    {
        var sessions = await sessionService.GetActiveSessionsAsync(
            currentUser.UserId, refreshToken, cancellationToken);
        return Ok(sessions);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sessionService.RevokeSessionAsync(currentUser.UserId, id, cancellationToken);
        return NoContent();
    }

    [HttpDelete("others")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeOtherSessions(
        [FromHeader(Name = "X-Refresh-Token")] string? refreshToken,
        CancellationToken cancellationToken)
    {
        await sessionService.RevokeAllOtherSessionsAsync(
            currentUser.UserId, refreshToken, cancellationToken);
        return NoContent();
    }
}
