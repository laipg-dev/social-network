using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SocialNetwork.Application.Common.Interfaces;
namespace SocialNetwork.Api.Authentication;
public sealed class CurrentUser : ICurrentUser {
 private readonly IHttpContextAccessor _accessor;
 public CurrentUser(IHttpContextAccessor accessor)=>_accessor=accessor;
 private ClaimsPrincipal? Principal=>_accessor.HttpContext?.User;
 public bool IsAuthenticated=>Principal?.Identity?.IsAuthenticated==true;
 public Guid UserId { get { var value=Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)??Principal?.FindFirstValue(ClaimTypes.NameIdentifier); return Guid.TryParse(value,out var id)?id:throw new InvalidOperationException("Authenticated user id claim is missing or invalid."); } }
 public string? Email=>Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)??Principal?.FindFirstValue(ClaimTypes.Email);
 public string? Username=>Principal?.FindFirstValue("username");
}
