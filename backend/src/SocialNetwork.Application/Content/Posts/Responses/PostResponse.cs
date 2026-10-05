namespace SocialNetwork.Application.Content.Posts.Responses;

public class PostResponse
{
    public Guid Id { get; set; }

    public string? Content { get; set; }

    public string Visibility { get; set; } = "Public";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}