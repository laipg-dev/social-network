using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Auth.Requests;

/// <summary>Payload used to authenticate with an email and password.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; init; }

    [Required, MaxLength(128)]
    public required string Password { get; init; }
}
