using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Domain.Communities.Entities;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CommunityId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Type { get; set; } = RoomType.Text;

    public int Position { get; set; }

    public bool IsPublic { get; set; } = true;

    public string Status { get; set; } = RoomStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Community Community { get; set; } = null!;

    public ICollection<RoomMember> Members { get; set; }
        = new List<RoomMember>();

    public ICollection<RoomRolePermission> RolePermissions { get; set; }
        = new List<RoomRolePermission>();
}
