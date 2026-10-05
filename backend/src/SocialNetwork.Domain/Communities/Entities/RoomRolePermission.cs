namespace SocialNetwork.Domain.Communities.Entities;

public class RoomRolePermission
{
    public Guid RoomId { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public bool IsAllowed { get; set; }

    public Room Room { get; set; } = null!;
    public CommunityRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}