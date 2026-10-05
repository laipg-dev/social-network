namespace SocialNetwork.Domain.Communities.Entities;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CommunityId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Type { get; set; } = "TEXT";

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Community Community { get; set; } = null!;

    public ICollection<RoomMember> Members { get; set; }
        = new List<RoomMember>();

    public ICollection<RoomRolePermission> RolePermissions { get; set; }
        = new List<RoomRolePermission>();
}