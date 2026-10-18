using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.Text;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Email;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Identity.Auth.Interfaces;
using SocialNetwork.Application.Identity.Auth.Requests;
using SocialNetwork.Application.Identity.Auth.Responses;
using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Authentication;

public sealed class AuthService(
    AppDbContext dbContext,
    IJwtTokenGenerator tokenGenerator,
    IHostEnvironment environment,
    IEmailSender emailSender) : IAuthService
{
    public async Task<RegistrationResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();

        if (await dbContext.Users.AnyAsync(
                user => user.Email == email || user.Username == username,
                cancellationToken))
        {
            throw new ConflictException("Email or username is already registered.");
        }

        var user = new User
        {
            Email = email,
            Username = username,
            PasswordHash = PasswordService.Hash(request.Password),
            Profile = new UserProfile { DisplayName = request.DisplayName?.Trim() },
            Settings = new UserSettings()
        };

        dbContext.Users.Add(user);
        var verificationCode = CreateVerificationCode();
        dbContext.EmailVerifications.Add(new EmailVerification
        {
            UserId = user.Id,
            TokenHash = HashToken(verificationCode),
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await emailSender.SendEmailVerificationAsync(
            user.Email, user.Profile?.DisplayName, verificationCode, cancellationToken);
        return new(
            "Registration successful. Check your email for the verification code.",
            user.Email,
            environment.IsDevelopment() ? verificationCode : null);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .Include(candidate => candidate.Profile)
            .SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is null)
            throw new UnauthorizedException(
                "No account was found with this email address.",
                "AUTH_EMAIL_NOT_FOUND");

        if (!PasswordService.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException(
                "The password is incorrect.",
                "AUTH_PASSWORD_INVALID");

        if (!user.IsActive)
            throw new UnauthorizedException(
                "This account is inactive. Please contact support.",
                "AUTH_ACCOUNT_INACTIVE");

        if (!user.IsEmailVerified)
            throw new UnauthorizedException(
                "Please verify your email before signing in.",
                "AUTH_EMAIL_NOT_VERIFIED");

        return CreateResponse(user, await CreateRefreshTokenAsync(user, null, null, cancellationToken));
    }

    public async Task<AuthResponse> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await dbContext.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.Profile)
            .SingleOrDefaultAsync(token => token.Token == tokenHash, cancellationToken);

        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token is invalid or expired.");

        stored.RevokedAt = DateTime.UtcNow;
        var replacement = await CreateRefreshTokenAsync(stored.User, ipAddress, userAgent, cancellationToken);
        return CreateResponse(stored.User, replacement);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var stored = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.Token == HashToken(refreshToken), cancellationToken);
        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ActionTokenResponse> RequestEmailVerificationAsync(
        string email, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.Email == email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null) return new("If the account exists, a verification email has been sent.", null);

        var token = CreateVerificationCode();
        dbContext.EmailVerifications.Add(new EmailVerification
        {
            UserId = user.Id, TokenHash = HashToken(token), ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await emailSender.SendEmailVerificationAsync(
            user.Email, user.Profile?.DisplayName, token, cancellationToken);
        return new("Verification email sent.",
            environment.IsDevelopment() ? token : null);
    }

    public async Task VerifyEmailAsync(
        string email,
        string code,
        CancellationToken cancellationToken)
    {
        var verification = await dbContext.EmailVerifications
            .Include(item => item.User)
            .Where(item => item.User.Email == email.Trim().ToLowerInvariant())
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (verification is null || verification.VerifiedAt is not null || verification.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Verification code is invalid or expired.");
        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(verification.TokenHash),
                Convert.FromHexString(HashToken(code.Trim()))))
            throw new UnauthorizedException("Verification code is invalid or expired.");

        verification.VerifiedAt = DateTime.UtcNow;
        verification.User.IsEmailVerified = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ActionTokenResponse> RequestPasswordResetAsync(
        string email, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.Email == email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null) return new("If the account exists, a password reset email has been sent.", null);

        var token = CreateToken();
        dbContext.PasswordResets.Add(new PasswordReset
        {
            UserId = user.Id, TokenHash = HashToken(token), ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        await emailSender.SendPasswordResetAsync(
            user.Email, user.Profile?.DisplayName, token, cancellationToken);
        return new("Password reset email sent.",
            environment.IsDevelopment() ? token : null);
    }

    public async Task ResetPasswordAsync(
        string token, string password, CancellationToken cancellationToken)
    {
        var reset = await dbContext.PasswordResets
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.TokenHash == HashToken(token), cancellationToken);
        if (reset is null || reset.UsedAt is not null || reset.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Password reset token is invalid or expired.");

        reset.UsedAt = DateTime.UtcNow;
        reset.User.PasswordHash = PasswordService.Hash(password);
        reset.User.UpdatedAt = DateTime.UtcNow;
        await dbContext.RefreshTokens
            .Where(item => item.UserId == reset.UserId && item.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(item => item.RevokedAt, DateTime.UtcNow), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            throw new NotFoundException("User not found.");

        if (!PasswordService.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException(
                "Mật khẩu hiện tại không chính xác.",
                "AUTH_CURRENT_PASSWORD_INVALID");

        if (request.CurrentPassword == request.NewPassword)
            throw new ConflictException("Mật khẩu mới không được trùng với mật khẩu hiện tại.");

        user.PasswordHash = PasswordService.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Revoke tất cả refresh tokens để bảo mật (đăng xuất khỏi mọi thiết bị)
        await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(t => t.RevokedAt, DateTime.UtcNow), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> CreateRefreshTokenAsync(
        User user, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var token = CreateToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, Token = HashToken(token), ExpiresAt = DateTime.UtcNow.AddDays(30),
            IpAddress = ipAddress, UserAgent = userAgent
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return token;
    }

    private static string CreateVerificationCode() =>
        RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    private AuthResponse CreateResponse(User user, string refreshToken) =>
        new(
            tokenGenerator.GenerateAccessToken(user.Id, user.Email, user.Username),
            user.Id,
            user.Email,
            user.Username,
            user.Profile?.DisplayName,
            refreshToken);

    private static string CreateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
