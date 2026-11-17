using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Infrastructure.Persistence.Seed.Communities;

public static class PermissionSeeder
{
    private static readonly (string Code, string Description)[] Catalog =
    [
        (CommunityPermissions.CommunityView, "View a community."),
        (CommunityPermissions.CommunityEdit, "Update community basic information."),
        (CommunityPermissions.CommunityDelete, "Delete a community."),
        (CommunityPermissions.CommunityClose, "Close or reopen a community."),
        (CommunityPermissions.MemberView, "View community members."),
        (CommunityPermissions.MemberManage, "Manage community members."),
        (CommunityPermissions.RoleView, "View community roles."),
        (CommunityPermissions.RoleManage, "Manage community roles and permissions."),
        (CommunityPermissions.RoomView, "View rooms."),
        (CommunityPermissions.RoomCreate, "Create rooms."),
        (CommunityPermissions.RoomEdit, "Update rooms."),
        (CommunityPermissions.RoomClose, "Close or reopen rooms."),
        (CommunityPermissions.RoomDelete, "Delete rooms."),
        (CommunityPermissions.RoomManage, "Manage room settings and permission overrides."),
        (CommunityPermissions.RoomMemberView, "View room members."),
        (CommunityPermissions.RoomMemberManage, "Manage room members."),
        (CommunityPermissions.MessageSend, "Send messages."),
        (CommunityPermissions.MessageDelete, "Delete messages.")
    ];

    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        foreach (var (code, description) in Catalog)
        {
            var exists = await dbContext.Permissions.AnyAsync(
                x => x.Code == code,
                cancellationToken);

            if (!exists)
            {
                dbContext.Permissions.Add(new Permission
                {
                    Code = code,
                    Description = description
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
