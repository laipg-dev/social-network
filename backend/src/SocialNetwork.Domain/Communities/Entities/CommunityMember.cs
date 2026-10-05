using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Communities.Entities;

public class CommunityMember
{
    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;


    // Navigation
    public Community Community { get; set; } = null!;
    public User User { get; set; } = null!;

    public ICollection<CommunityMemberRole> Roles { get; set; }
        = new List<CommunityMemberRole>();
}