using SocialNetwork.Application.Content.Communities.Models;

namespace SocialNetwork.Application.Content.Communities.Responses;

public class CommunityResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    public bool IsPublic { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public Guid OwnerId { get; set; }

    public string OwnerUsername { get; set; } = string.Empty;

    public string? OwnerDisplayName { get; set; }

    public string? OwnerAvatarUrl { get; set; }

    public int MemberCount { get; set; }

    public string? MyMembershipStatus { get; set; }

    public IReadOnlyList<CommunityRoleSummary> MyRoles { get; set; }
        = Array.Empty<CommunityRoleSummary>();

    public IReadOnlyList<string> MyPermissions { get; set; }
        = Array.Empty<string>();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
