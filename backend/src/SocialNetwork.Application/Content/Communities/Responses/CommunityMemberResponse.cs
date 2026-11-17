using SocialNetwork.Application.Content.Communities.Models;

namespace SocialNetwork.Application.Content.Communities.Responses;

public class CommunityMemberResponse
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? RequestedAt { get; set; }

    public DateTime? JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    public IReadOnlyList<CommunityRoleSummary> Roles { get; set; }
        = Array.Empty<CommunityRoleSummary>();
}
