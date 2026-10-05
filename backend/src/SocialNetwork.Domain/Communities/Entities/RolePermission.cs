namespace SocialNetwork.Domain.Communities.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public CommunityRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}