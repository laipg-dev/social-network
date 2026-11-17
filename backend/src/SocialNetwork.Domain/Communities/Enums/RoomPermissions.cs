namespace SocialNetwork.Domain.Communities.Enums;

public static class RoomPermissions
{
    // Các permission được phép override ở cấp Room.
    public static readonly IReadOnlyList<string> Overridable =
    [
        CommunityPermissions.RoomView,
        CommunityPermissions.RoomEdit,
        CommunityPermissions.RoomClose,
        CommunityPermissions.RoomDelete,
        CommunityPermissions.RoomManage,
        CommunityPermissions.RoomMemberView,
        CommunityPermissions.RoomMemberManage,
        CommunityPermissions.MessageSend,
        CommunityPermissions.MessageDelete
    ];
}
