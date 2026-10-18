namespace SocialNetwork.Application.Identity.Sessions.Responses;

public sealed record SessionResponse(
    Guid Id,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    bool IsActive,
    bool IsCurrent);
