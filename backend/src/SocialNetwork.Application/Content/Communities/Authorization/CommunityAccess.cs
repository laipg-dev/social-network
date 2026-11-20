using SocialNetwork.Application.Content.Communities.Models;

namespace SocialNetwork.Application.Content.Communities.Authorization;

public sealed class CommunityAccess
{
    public bool IsOwner { get; init; }

    public bool HasMembership { get; init; }

    public bool IsMember { get; init; }
    public bool IsBanned { get; init; }

    public string? MembershipStatus { get; init; }

    public IReadOnlyList<CommunityRoleSummary> Roles { get; init; }
        = Array.Empty<CommunityRoleSummary>();

    public IReadOnlyList<string> Permissions { get; init; }
        = Array.Empty<string>();
}
