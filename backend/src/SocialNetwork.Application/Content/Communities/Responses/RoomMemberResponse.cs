namespace SocialNetwork.Application.Content.Communities.Responses;

public class RoomMemberResponse
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime JoinedAt { get; set; }
}
