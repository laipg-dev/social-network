using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/public/dev")]
public class DevAuthController : ControllerBase
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public DevAuthController(IJwtTokenGenerator jwtTokenGenerator)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [AllowAnonymous]
[HttpPost("token")]
public IActionResult GenerateToken()
{
    var userId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111");

    var requestId = Guid.NewGuid();

    var token = _jwtTokenGenerator.GenerateAccessToken(
        userId,
        "dev@socialnetwork.local",
        "dev-user");

    return Ok(new
    {
        RequestId = requestId,
        GeneratedAtUtc = DateTime.UtcNow,
        AccessToken = token
    });
}
}