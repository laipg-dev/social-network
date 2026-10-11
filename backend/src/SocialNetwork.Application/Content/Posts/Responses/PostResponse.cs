namespace SocialNetwork.Application.Content.Posts.Responses;

public sealed class PostResponse
{
    public Guid Id { get; set; }

    public string? Content { get; set; }

    public string Visibility { get; set; } = "Public";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorUsername { get; set; }
    public string? AuthorDisplayName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public Guid? CommunityId { get; set; }
    public string Status { get; set; } = "ACTIVE";

}