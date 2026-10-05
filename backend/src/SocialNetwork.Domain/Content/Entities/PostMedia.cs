namespace SocialNetwork.Domain.Content.Entities;

public class PostMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PostId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; } = null!;
}