using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Domain.Messaging.Entities;

public class RoomMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }

    public Guid? ReplyToMessageId { get; set; }

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Room Room { get; set; } = null!;
    public User Sender { get; set; } = null!;

    public RoomMessage? ReplyToMessage { get; set; }

    public ICollection<RoomMessage> Replies { get; set; }
        = new List<RoomMessage>();

    public ICollection<RoomMessageAttachment> Attachments { get; set; }
        = new List<RoomMessageAttachment>();
}