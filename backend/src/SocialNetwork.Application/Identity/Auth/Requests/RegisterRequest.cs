using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Auth.Requests;

/// <summary>Payload used to create a new user account.</summary>
public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; init; }

    [Required, MinLength(3), MaxLength(100)]
    public required string Username { get; init; }

    [Required, MinLength(8), MaxLength(128)]
    public required string Password { get; init; }

    [MaxLength(100)]
    public string? DisplayName { get; init; }
}
