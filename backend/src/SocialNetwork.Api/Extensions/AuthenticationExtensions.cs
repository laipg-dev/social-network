using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Api.Authentication;
using SocialNetwork.Infrastructure.Authentication;
using SocialNetwork.Api.Common.Errors;
namespace SocialNetwork.Api.Extensions;
public static class AuthenticationExtensions {
 public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,IConfiguration configuration) {
  var section=configuration.GetSection(JwtOptions.SectionName);
  var jwt=section.Get<JwtOptions>()??throw new InvalidOperationException("JWT configuration is missing.");
  if(string.IsNullOrWhiteSpace(jwt.Secret)||jwt.Secret.Length<32) throw new InvalidOperationException("Jwt:Secret must contain at least 32 characters.");
  services.Configure<JwtOptions>(section);
  services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options=>{
   options.MapInboundClaims=false;
   options.TokenValidationParameters=new TokenValidationParameters { ValidateIssuer=true,ValidIssuer=jwt.Issuer,ValidateAudience=true,ValidAudience=jwt.Audience,ValidateLifetime=true,ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),ClockSkew=TimeSpan.Zero,NameClaimType="username" };
    options.Events = new JwtBearerEvents
{

    OnAuthenticationFailed = context =>
{
    Console.WriteLine("========== JWT AUTH FAILED ==========");
    Console.WriteLine(context.Exception.GetType().FullName);
    Console.WriteLine(context.Exception.Message);
    Console.WriteLine("=====================================");

    return Task.CompletedTask;
},

    OnChallenge = async context =>
    {
        context.HandleResponse();

        var response = context.Response;

        response.StatusCode =
            StatusCodes.Status401Unauthorized;

        response.ContentType =
            "application/json";

        var error = new ApiErrorResponse(
            StatusCodes.Status401Unauthorized,
            "UNAUTHORIZED",
            "Authentication is required.",
            TraceId: context.HttpContext.TraceIdentifier);

        await response.WriteAsJsonAsync(error);
    },

    OnMessageReceived = context =>
    {
        var accessToken =
            context.Request.Query["access_token"];

        var path = context.HttpContext.Request.Path;

        if (!string.IsNullOrEmpty(accessToken)
            && path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    },

    OnForbidden = async context =>
    {
        var response = context.Response;

        response.StatusCode =
            StatusCodes.Status403Forbidden;

        response.ContentType =
            "application/json";

        var error = new ApiErrorResponse(
            StatusCodes.Status403Forbidden,
            "FORBIDDEN",
            "You do not have permission to perform this action.",
            TraceId: context.HttpContext.TraceIdentifier);

        await response.WriteAsJsonAsync(error);
    }
};
  });
  services.AddAuthorization(options=>{ options.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); });
  services.AddHttpContextAccessor();
  services.AddScoped<ICurrentUser,CurrentUser>();
  services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
  return services;
 }
}
