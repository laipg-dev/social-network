namespace SocialNetwork.Application.Identity.Profile.Responses;

/// <summary>Profile information for the authenticated user.</summary>
public sealed record ProfileResponse(
    Guid UserId,
    string Email,
    string Username,
    string? DisplayName,
    string? Bio,
    string? AvatarUrl,
    string? CoverUrl,
    DateTime? DateOfBirth,
    bool IsEmailVerified);
