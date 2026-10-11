using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Auth.Requests;

/// <summary>Payload used to set a new password with a valid reset token.</summary>
public sealed record ResetPasswordRequest
{
    [Required]
    public required string Token { get; init; }

    [Required, MinLength(8), MaxLength(128)]
    public required string Password { get; init; }
}
