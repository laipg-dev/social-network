using SocialNetwork.Application.Identity.Auth.Requests;
using SocialNetwork.Application.Identity.Auth.Responses;

namespace SocialNetwork.Application.Identity.Auth.Interfaces;

public interface IAuthService
{
    Task<RegistrationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
    Task<ActionTokenResponse> RequestEmailVerificationAsync(string email, CancellationToken cancellationToken);
    Task VerifyEmailAsync(string email, string code, CancellationToken cancellationToken);
    Task<ActionTokenResponse> RequestPasswordResetAsync(string email, CancellationToken cancellationToken);
    Task ResetPasswordAsync(string token, string password, CancellationToken cancellationToken);
}
