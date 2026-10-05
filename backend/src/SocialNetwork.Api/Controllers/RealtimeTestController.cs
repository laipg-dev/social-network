using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Common.Realtime;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/realtime-test")]
public sealed class RealtimeTestController : ControllerBase
{
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly ICurrentUser _currentUser;

    public RealtimeTestController(
        IRealtimeNotifier realtimeNotifier,
        ICurrentUser currentUser)
    {
        _realtimeNotifier = realtimeNotifier;
        _currentUser = currentUser;
    }

    [HttpPost("send-to-me")]
    public async Task<IActionResult> SendToMe(
        CancellationToken cancellationToken)
    {
        var data = new
        {
            Message = "Realtime foundation is working!",
            UserId = _currentUser.UserId,
            SentAt = DateTime.UtcNow
        };

        await _realtimeNotifier.SendToUserAsync(
            _currentUser.UserId,
            "TestNotification",
            data,
            cancellationToken);

        return Ok(data);
    }
}