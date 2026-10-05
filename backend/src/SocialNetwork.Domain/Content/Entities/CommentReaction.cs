using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Content.Entities;

public class CommentReaction
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }

    public string Type { get; set; } = "LIKE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}