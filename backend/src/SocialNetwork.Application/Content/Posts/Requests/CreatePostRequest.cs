namespace SocialNetwork.Application.Content.Posts.Requests;

public sealed class CreatePostRequest
{
    public string? Content {get; set; }

    public string Visibility { get; set; } = "PUBLIC";
    public Guid? CommunityId { get; set; }
}