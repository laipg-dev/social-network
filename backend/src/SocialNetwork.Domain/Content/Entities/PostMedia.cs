namespace SocialNetwork.Domain.Content.Entities;

public class PostMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PostId { get; set; }

    public string? StorageKey { get; set; }

    // Kept only to preserve pre-migration URL values. New code must not read or write this field.
    public string? LegacyUrl { get; private set; }

    public string MediaType { get; set; } = string.Empty;

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; } = null!;
}
