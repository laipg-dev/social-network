namespace SocialNetwork.Application.Communities.Requests;

public sealed record UpdateCommunityRequest(
    string Name,
    string? Description,
    string? AvatarUrl,
    string? CoverUrl,
    bool IsPublic);
