namespace SocialNetwork.Application.Content.Communities.Responses;

public class RoomResponse
{
    public Guid Id { get; set; }

    public Guid CommunityId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Type { get; set; } = "TEXT";

    public int Position { get; set; }

    public bool IsPublic { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public int MemberCount { get; set; }

    public bool IsRoomMember { get; set; }

    public IReadOnlyList<string> MyPermissions { get; set; }
        = Array.Empty<string>();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
