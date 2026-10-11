namespace SocialNetwork.Application.Communities.Requests;

public sealed record CreateCommunityRequest(
    string Name,
    string? Description,
    bool IsPublic = true);
