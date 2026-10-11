using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Auth.Requests;

/// <summary>Payload containing an email address for an account action.</summary>
public sealed record EmailRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; init; }
}
