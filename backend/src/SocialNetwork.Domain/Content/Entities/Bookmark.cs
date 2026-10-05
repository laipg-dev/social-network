using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Content.Entities;

public class Bookmark
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}