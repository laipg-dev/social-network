using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Infrastructure.Persistence.Seed.Communities;

public static class PermissionSeeder
{
    private static readonly (string Code, string Description)[] Catalog =
    [
        (CommunityPermissions.CommunityEdit, "Update community basic information."),
        (CommunityPermissions.CommunityDelete, "Delete a community."),
        (CommunityPermissions.CommunityMemberView, "View community members."),
        (CommunityPermissions.CommunityMemberManage, "Manage community members."),
        (CommunityPermissions.CommunityRoleView, "View community roles."),
        (CommunityPermissions.CommunityRoleManage, "Manage community roles and permissions."),
        (CommunityPermissions.RoomCreate, "Create rooms."),
        (CommunityPermissions.RoomManage, "Manage room settings and permission overrides."),
        (CommunityPermissions.RoomDelete, "Delete rooms."),
        (CommunityPermissions.RoomAccessManage, "Manage room access."),
        (CommunityPermissions.MessageSend, "Send messages."),
        (CommunityPermissions.MessageDeleteOwn, "Delete own messages."),
        (CommunityPermissions.MessageDeleteAny, "Delete any message."),
        (CommunityPermissions.MessagePin, "Pin messages."),
        (CommunityPermissions.RoomMemberView, "View room members."),
        (CommunityPermissions.RoomMemberManage, "Manage room members."),
        (CommunityPermissions.CallMuteMember, "Mute community call members."),
        (CommunityPermissions.CallRemoveMember, "Remove community call members.")
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
