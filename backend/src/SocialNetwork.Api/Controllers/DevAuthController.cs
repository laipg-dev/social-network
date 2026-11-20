using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/public/dev")]
public class DevAuthController : ControllerBase
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHostEnvironment _environment;

    public DevAuthController(IJwtTokenGenerator jwtTokenGenerator, IHostEnvironment environment)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _environment = environment;
    }

    [AllowAnonymous]
[HttpPost("token")]
public IActionResult GenerateToken()
{
    if (!_environment.IsDevelopment()) return NotFound();
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
