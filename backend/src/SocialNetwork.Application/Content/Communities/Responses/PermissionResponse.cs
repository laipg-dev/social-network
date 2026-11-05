namespace SocialNetwork.Application.Content.Communities.Responses;

public class PermissionResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
}
