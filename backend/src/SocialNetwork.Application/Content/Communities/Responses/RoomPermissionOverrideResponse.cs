namespace SocialNetwork.Application.Content.Communities.Responses;

public class RoomPermissionOverrideResponse
{
    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool IsAllowed { get; set; }
}
