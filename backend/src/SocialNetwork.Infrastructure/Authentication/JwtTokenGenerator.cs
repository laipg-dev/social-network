using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Application.Common.Interfaces;
namespace SocialNetwork.Infrastructure.Authentication;
public sealed class JwtTokenGenerator : IJwtTokenGenerator {
 private readonly JwtOptions _options;
 public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options.Value;
 public string GenerateAccessToken(Guid userId,string email,string username) {
  var claims=new[]{new Claim(JwtRegisteredClaimNames.Sub,userId.ToString()),new Claim(JwtRegisteredClaimNames.Email,email),new Claim("username",username),new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())};
  var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
  var token=new JwtSecurityToken(_options.Issuer,_options.Audience,claims,expires:DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256));
  return new JwtSecurityTokenHandler().WriteToken(token);
 }
}
