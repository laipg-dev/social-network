using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Auth.Requests;

/// <summary>Payload used to confirm an email address with its one-time code.</summary>
public sealed record VerifyEmailRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; init; }

    [Required, StringLength(6, MinimumLength = 6)]
    public required string Code { get; init; }
}
