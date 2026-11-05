namespace SocialNetwork.Application.Content.Communities.Responses;

public class CommunityRoleResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Position { get; set; }

    public bool IsDefault { get; set; }

    public int MemberCount { get; set; }

    public IReadOnlyList<PermissionResponse> Permissions { get; set; }
        = Array.Empty<PermissionResponse>();
}
