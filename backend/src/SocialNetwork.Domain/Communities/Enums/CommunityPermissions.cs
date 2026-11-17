namespace SocialNetwork.Domain.Communities.Enums;

public static class CommunityPermissions
{
    public const string CommunityView = "COMMUNITY_VIEW";
    public const string CommunityEdit = "COMMUNITY_EDIT";
    public const string CommunityDelete = "COMMUNITY_DELETE";
    public const string CommunityClose = "COMMUNITY_CLOSE";

    public const string MemberView = "MEMBER_VIEW";
    public const string MemberManage = "MEMBER_MANAGE";

    public const string RoleView = "ROLE_VIEW";
    public const string RoleManage = "ROLE_MANAGE";

    public const string RoomView = "ROOM_VIEW";
    public const string RoomCreate = "ROOM_CREATE";
    public const string RoomEdit = "ROOM_EDIT";
    public const string RoomClose = "ROOM_CLOSE";
    public const string RoomDelete = "ROOM_DELETE";
    public const string RoomManage = "ROOM_MANAGE";
    public const string RoomMemberView = "ROOM_MEMBER_VIEW";
    public const string RoomMemberManage = "ROOM_MEMBER_MANAGE";

    public const string MessageSend = "MESSAGE_SEND";
    public const string MessageDelete = "MESSAGE_DELETE";

    public static readonly IReadOnlyList<string> All =
    [
        CommunityView,
        CommunityEdit,
        CommunityDelete,
        CommunityClose,
        MemberView,
        MemberManage,
        RoleView,
        RoleManage,
        RoomView,
        RoomCreate,
        RoomEdit,
        RoomClose,
        RoomDelete,
        RoomManage,
        RoomMemberView,
        RoomMemberManage,
        MessageSend,
        MessageDelete
    ];
}
