namespace SocialNetwork.Domain.Communities.Entities;

public class CommunityMemberRole
{
    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public CommunityMember CommunityMember { get; set; } = null!;
    public CommunityRole Role { get; set; } = null!;
}