using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Messaging.Entities;

public class DirectMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }

    public Guid? ReplyToMessageId { get; set; }

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;

    public DirectMessage? ReplyToMessage { get; set; }

    public ICollection<DirectMessage> Replies { get; set; }
        = new List<DirectMessage>();

    public ICollection<DirectMessageAttachment> Attachments { get; set; }
        = new List<DirectMessageAttachment>();

    public ICollection<MessageReaction> Reactions { get; set; }
        = new List<MessageReaction>();
}