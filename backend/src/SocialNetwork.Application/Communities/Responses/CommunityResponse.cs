namespace SocialNetwork.Application.Communities.Responses;

public sealed record CommunityResponse(
    Guid Id,
    Guid OwnerId,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? CoverUrl,
    bool IsPublic,
    int MemberCount,
    bool IsMember,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
