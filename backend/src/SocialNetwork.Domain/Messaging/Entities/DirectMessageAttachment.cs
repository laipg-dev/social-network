namespace SocialNetwork.Domain.Messaging.Entities;

public class DirectMessageAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Foreign Key
    public Guid DirectMessageId { get; set; }

    public string Url { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public DirectMessage DirectMessage { get; set; } = null!;
}