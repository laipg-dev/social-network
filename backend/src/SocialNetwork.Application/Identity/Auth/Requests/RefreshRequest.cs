using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Auth.Requests;

/// <summary>Payload used to exchange a refresh token for a new access token.</summary>
public sealed record RefreshRequest
{
    [Required]
    public required string RefreshToken { get; init; }
}
