using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Domain.Moderation.Entities;

public class CommunityBan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }
    public Guid BannedById { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // null = ban vĩnh viễn
    public DateTime? ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public Community Community { get; set; } = null!;
    public User User { get; set; } = null!;
    public User BannedBy { get; set; } = null!;
}