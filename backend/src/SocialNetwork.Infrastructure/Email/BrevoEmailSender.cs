using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SocialNetwork.Application.Common.Email;

namespace SocialNetwork.Infrastructure.Email;

public sealed class BrevoEmailSender(
    HttpClient httpClient,
    IOptions<EmailOptions> options,
    ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public Task SendEmailVerificationAsync(
        string recipientEmail,
        string? displayName,
        string verificationCode,
        CancellationToken cancellationToken) =>
        SendCodeAsync(
            recipientEmail,
            displayName,
            "Verify your Social Network account",
            verificationCode,
            "Use this verification code to confirm your email address. It expires in 15 minutes.",
            cancellationToken);

    public Task SendPasswordResetAsync(
        string recipientEmail,
        string? displayName,
        string token,
        CancellationToken cancellationToken) =>
        SendAsync(
            recipientEmail,
            displayName,
            "Reset your Social Network password",
            BuildLink("/reset-password", token),
            "Click the link below to reset your password. This link expires in 1 hour.",
            cancellationToken);

    private async Task SendAsync(
        string recipientEmail,
        string? displayName,
        string subject,
        string action,
        string description,
        CancellationToken cancellationToken,
        bool isCode = false)
    {
        ValidateConfiguration();

        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email");
        request.Headers.Add("api-key", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            sender = new { name = _options.FromName, email = _options.FromAddress },
            to = new[] { new { email = recipientEmail, name = displayName ?? recipientEmail } },
            subject,
            textContent = isCode
                ? $"{description}\n\nVerification code: {action}\n\nIf you did not request this, you can ignore this email."
                : $"{description}\n\n{action}\n\nIf you did not request this, you can ignore this email.",
            htmlContent = isCode
                ? $"<p>{description}</p><p>Your verification code is <strong>{WebUtility.HtmlEncode(action)}</strong></p><p>If you did not request this, you can ignore this email.</p>"
                : $"<p>{description}</p><p><a href=\"{WebUtility.HtmlEncode(action)}\">Continue</a></p><p>If you did not request this, you can ignore this email.</p>"
        });

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Brevo rejected email to {RecipientEmail} with status {StatusCode}: {Details}",
                    recipientEmail, response.StatusCode, details);
                throw new InvalidOperationException(
                    "The email provider rejected the message. Check the Brevo sender and API configuration.");
            }

        }
        catch (Exception exception) when (exception is not OperationCanceledException and not InvalidOperationException)
        {
            logger.LogError(exception, "Failed to send email through Brevo to {RecipientEmail}", recipientEmail);
            throw new InvalidOperationException(
                "The email could not be sent. Check the Brevo API configuration.", exception);
        }
    }

    private Task SendCodeAsync(
        string recipientEmail,
        string? displayName,
        string subject,
        string verificationCode,
        string description,
        CancellationToken cancellationToken) =>
        SendAsync(
            recipientEmail,
            displayName,
            subject,
            verificationCode,
            description,
            cancellationToken,
            isCode: true);

    private string BuildLink(string path, string token) =>
        $"{_options.FrontendBaseUrl.TrimEnd('/')}{path}?token={Uri.EscapeDataString(token)}";

    private void ValidateConfiguration()
    {
        if (!_options.Enabled)
            throw new InvalidOperationException("Email sending is disabled. Set Email:Enabled to true.");
        if (string.IsNullOrWhiteSpace(_options.ApiKey) ||
            string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException(
                "Brevo configuration is missing. Set Email:ApiKey and Email:FromAddress.");
        }
    }
}
