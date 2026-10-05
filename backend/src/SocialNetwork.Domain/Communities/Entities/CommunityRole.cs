namespace SocialNetwork.Domain.Communities.Entities;

public class CommunityRole
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CommunityId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Position { get; set; }

    public Community Community { get; set; } = null!;

    public ICollection<CommunityMemberRole> Members { get; set; }
        = new List<CommunityMemberRole>();

    public ICollection<RolePermission> Permissions { get; set; }
        = new List<RolePermission>();
}