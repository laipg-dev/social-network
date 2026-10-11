using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Identity.Profile.Requests;

/// <summary>Editable profile information for the authenticated user.</summary>
public sealed record UpdateProfileRequest
{
    [Required, MinLength(3), MaxLength(100)]
    public required string Username { get; init; }

    [MaxLength(100)]
    public string? DisplayName { get; init; }

    [MaxLength(1000)]
    public string? Bio { get; init; }

    [Url, MaxLength(500)]
    public string? AvatarUrl { get; init; }

    [Url, MaxLength(500)]
    public string? CoverUrl { get; init; }

    public DateTime? DateOfBirth { get; init; }
}
