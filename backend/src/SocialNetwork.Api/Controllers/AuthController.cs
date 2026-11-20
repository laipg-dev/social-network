using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Identity.Auth.Interfaces;
using SocialNetwork.Application.Identity.Auth.Requests;
using SocialNetwork.Application.Identity.Auth.Responses;
using SocialNetwork.Application.Common.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth-sensitive")]
    [AllowAnonymous]
    [ProducesResponseType<RegistrationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth-sensitive")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.RefreshAsync(
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        RefreshRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpPost("email-verification/request")]
    [EnableRateLimiting("auth-sensitive")]
    [AllowAnonymous]
    [ProducesResponseType<ActionTokenResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ActionTokenResponse>> RequestEmailVerification(
        EmailRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.RequestEmailVerificationAsync(request.Email, cancellationToken));
    }

    [HttpPost("email-verification/confirm")]
    [EnableRateLimiting("auth-sensitive")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmEmailVerification(
        VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        await authService.VerifyEmailAsync(request.Email, request.Code, cancellationToken);
        return NoContent();
    }

    [HttpPost("password-reset/request")]
    [EnableRateLimiting("auth-sensitive")]
    [AllowAnonymous]
    [ProducesResponseType<ActionTokenResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ActionTokenResponse>> RequestPasswordReset(
        EmailRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.RequestPasswordResetAsync(request.Email, cancellationToken));
    }

    [HttpPost("password-reset/confirm")]
    [EnableRateLimiting("auth-sensitive")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmPasswordReset(
        ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(request.Token, request.Password, cancellationToken);
        return NoContent();
    }

    [HttpPost("password/change")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ChangePasswordAsync(currentUser.UserId, request.CurrentPassword, request.NewPassword, cancellationToken);
        return NoContent();
    }
}

public sealed record ChangePasswordRequest(
    [property: System.ComponentModel.DataAnnotations.Required] string CurrentPassword,
    [property: System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(8), System.ComponentModel.DataAnnotations.MaxLength(128)] string NewPassword);
