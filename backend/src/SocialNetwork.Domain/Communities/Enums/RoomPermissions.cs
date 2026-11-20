namespace SocialNetwork.Domain.Communities.Enums;

public static class RoomPermissions
{
    public static readonly IReadOnlyList<string> Overridable =
    [
        CommunityPermissions.MessageSend,
        CommunityPermissions.MessageDeleteOwn,
        CommunityPermissions.MessageDeleteAny
    ];
}