using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Messaging.Entities;

public class MessageReaction
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }

    public string Type { get; set; } = "LIKE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DirectMessage Message { get; set; } = null!;
    public User User { get; set; } = null!;
}