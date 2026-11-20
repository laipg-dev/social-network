using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Storage;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/media/{category}/{objectId}")]
public sealed class MediaDeliveryController(IStorageService storage) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(string category, string objectId, [FromQuery] long expires, [FromQuery] string token, CancellationToken cancellationToken)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(category, "^[a-z0-9-]{1,32}$") || !Guid.TryParseExact(objectId, "N", out _))
            return NotFound();
        try
        {
            var stream = await storage.OpenSignedReadAsync($"{category}/{objectId}", expires, token, cancellationToken);
            return File(stream, "application/octet-stream", enableRangeProcessing: true);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}
