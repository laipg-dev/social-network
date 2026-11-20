using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Application.Content.Communities;

public static class CommunityRoleDefaults
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Moderator = "Moderator";
    public const string Member = "Member";

    private sealed record RoleDefinition(
        int Position,
        IReadOnlyList<string> Permissions);

    private static readonly IReadOnlyDictionary<string, RoleDefinition> Definitions =
        new Dictionary<string, RoleDefinition>(StringComparer.Ordinal)
        {
            [Owner] = new(
                100,
                CommunityPermissions.All),

            [Admin] = new(
                80,
                CommunityPermissions.All
                    .Where(code => code != CommunityPermissions.CommunityDelete)
                    .ToArray()),

            [Moderator] = new(
                50,
                [
                    CommunityPermissions.CommunityMemberView,
                    CommunityPermissions.CommunityMemberManage,
                    CommunityPermissions.CommunityRoleView,
                    CommunityPermissions.RoomCreate,
                    CommunityPermissions.RoomManage,
                    CommunityPermissions.RoomMemberView,
                    CommunityPermissions.RoomMemberManage,
                    CommunityPermissions.MessageSend,
                    CommunityPermissions.MessageDeleteAny
                ]),

            [Member] = new(
                0,
                [
                    CommunityPermissions.CommunityMemberView,
                    CommunityPermissions.CommunityRoleView,
                    CommunityPermissions.RoomMemberView,
                    CommunityPermissions.MessageSend
                ])
        };

    public static IReadOnlyList<string> RoleNames =>
        Definitions.Keys.ToArray();

    public static int GetDefaultPosition(string roleName)
    {
        return Definitions.TryGetValue(roleName, out var definition)
            ? definition.Position
            : 0;
    }

    public static IReadOnlyList<string> GetDefaultPermissionCodes(
        string roleName)
    {
        return Definitions.TryGetValue(roleName, out var definition)
            ? definition.Permissions
            : [];
    }

    public static IReadOnlyList<CommunityRole> CreateRoles(
        Guid communityId,
        IReadOnlyDictionary<string, Guid> permissionIds)
    {
        return Definitions
            .Select(definition => CreateRole(
                communityId,
                definition.Key,
                definition.Value,
                permissionIds))
            .ToArray();
    }

    private static CommunityRole CreateRole(
        Guid communityId,
        string roleName,
        RoleDefinition definition,
        IReadOnlyDictionary<string, Guid> permissionIds)
    {
        var role = new CommunityRole
        {
            Id = Guid.NewGuid(),
            CommunityId = communityId,
            Name = roleName,
            Position = definition.Position,
            IsDefault = string.Equals(
                roleName,
                Member,
                StringComparison.Ordinal)
        };

        foreach (var code in definition.Permissions)
        {
            if (!permissionIds.TryGetValue(code, out var permissionId))
            {
                throw new InvalidOperationException(
                    $"Permission '{code}' has not been seeded.");
            }

            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
        }

        return role;
    }
}
