namespace SocialNetwork.Domain.Messaging.Entities;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // DIRECT hoặc GROUP
    public string Type { get; set; } = "DIRECT";

    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ConversationMember> Members { get; set; }
        = new List<ConversationMember>();

    public ICollection<DirectMessage> Messages { get; set; }
        = new List<DirectMessage>();
}