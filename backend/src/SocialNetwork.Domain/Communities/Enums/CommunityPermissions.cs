namespace SocialNetwork.Domain.Communities.Enums;

public static class CommunityPermissions
{
    // =========================
    // COMMUNITY
    // =========================

    public const string CommunityEdit = "COMMUNITY_EDIT";
    public const string CommunityDelete = "COMMUNITY_DELETE";

    // =========================
    // COMMUNITY MEMBER
    // =========================

    public const string CommunityMemberView = "COMMUNITY_MEMBER_VIEW";
    public const string CommunityMemberManage = "COMMUNITY_MEMBER_MANAGE";

    // =========================
    // COMMUNITY ROLE
    // =========================

    public const string CommunityRoleView = "COMMUNITY_ROLE_VIEW";
    public const string CommunityRoleManage = "COMMUNITY_ROLE_MANAGE";

    // =========================
    // ROOM MANAGEMENT
    // =========================

    public const string RoomCreate = "ROOM_CREATE";
    public const string RoomManage = "ROOM_MANAGE";
    public const string RoomDelete = "ROOM_DELETE";
    public const string RoomAccessManage = "ROOM_ACCESS_MANAGE";

    // =========================
    // ROOM ACTIVITIES
    // =========================

    public const string MessageSend = "MESSAGE_SEND";
    public const string MessageDeleteOwn = "MESSAGE_DELETE_OWN";
    public const string MessageDeleteAny = "MESSAGE_DELETE_ANY";
    public const string MessagePin = "MESSAGE_PIN";

    public const string RoomMemberView = "ROOM_MEMBER_VIEW";
    public const string RoomMemberManage = "ROOM_MEMBER_MANAGE";

    public const string CallMuteMember = "CALL_MUTE_MEMBER";
    public const string CallRemoveMember = "CALL_REMOVE_MEMBER";

    // =========================
    // ALL PERMISSIONS
    // =========================

    public static readonly IReadOnlyList<string> All =
    [
        CommunityEdit,
        CommunityDelete,

        CommunityMemberView,
        CommunityMemberManage,

        CommunityRoleView,
        CommunityRoleManage,

        RoomCreate,
        RoomManage,
        RoomDelete,
        RoomAccessManage,

        MessageSend,
        MessageDeleteOwn,
        MessageDeleteAny,
        MessagePin,

        RoomMemberView,
        RoomMemberManage,

        CallMuteMember,
        CallRemoveMember
    ];
}