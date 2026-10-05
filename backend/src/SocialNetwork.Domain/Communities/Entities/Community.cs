using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Communities.Entities;

public class Community
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User Owner { get; set; } = null!;

    public ICollection<CommunityMember> Members { get; set; }
        = new List<CommunityMember>();

    public ICollection<CommunityRole> Roles { get; set; }
        = new List<CommunityRole>();

    public ICollection<Room> Rooms { get; set; }
        = new List<Room>();
}