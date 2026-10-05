namespace SocialNetwork.Application.Content.Posts.Requests;

public class CreatePostRequest
{
    public string? Content {get; set; }

    public string Visibility {get; set; } = "Public";
}