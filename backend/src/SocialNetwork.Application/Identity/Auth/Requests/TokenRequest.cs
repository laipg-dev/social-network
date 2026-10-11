using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Auth.Requests;

/// <summary>Payload containing an account action token.</summary>
public sealed record TokenRequest
{
    [Required]
    public required string Token { get; init; }
}
