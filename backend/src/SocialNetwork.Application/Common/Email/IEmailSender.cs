namespace SocialNetwork.Application.Common.Email;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        string recipientEmail,
        string? displayName,
        string verificationCode,
        CancellationToken cancellationToken);

    Task SendPasswordResetAsync(
        string recipientEmail,
        string? displayName,
        string token,
        CancellationToken cancellationToken);
}
