using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Seed.Communities;

// Đồng bộ role mặc định cho mọi community:
// - tạo role còn thiếu (Owner/Admin/Moderator/Member)
// - cập nhật position chuẩn
// - bổ sung permission còn thiếu theo mapping mặc định
// - đảm bảo owner có membership + role Owner
// Idempotent: chạy nhiều lần không tạo duplicate.
public static class CommunityRoleBackfill
{
    public static async Task EnsureAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var permissionIds = await dbContext.Permissions
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.Code,
                x => x.Id,
                cancellationToken);

        var communities = await dbContext.Communities
            .AsNoTracking()
            .Select(x => new { x.Id, x.OwnerId })
            .ToListAsync(cancellationToken);

        foreach (var community in communities)
        {
            var roles = await dbContext.CommunityRoles
                .Where(x => x.CommunityId == community.Id)
                .ToListAsync(cancellationToken);

            var rolesByName = roles.ToDictionary(
                x => x.Name,
                StringComparer.Ordinal);

            var roleIds = roles.Select(x => x.Id).ToArray();

            List<RolePermission> existingLinks = roleIds.Length == 0
                ? []
                : await dbContext.RolePermissions
                    .Where(x => roleIds.Contains(x.RoleId))
                    .ToListAsync(cancellationToken);

            foreach (var roleName in CommunityRoleDefaults.RoleNames)
            {
                if (!rolesByName.TryGetValue(roleName, out var role))
                {
                    role = new CommunityRole
                    {
                        Id = Guid.NewGuid(),
                        CommunityId = community.Id,
                        Name = roleName,
                        Position = CommunityRoleDefaults.GetDefaultPosition(roleName),
                        IsDefault = roleName == CommunityRoleDefaults.Member
                    };

                    dbContext.CommunityRoles.Add(role);
                    rolesByName[roleName] = role;
                }
                else
                {
                    role.Position = CommunityRoleDefaults.GetDefaultPosition(roleName);
                    role.IsDefault = roleName == CommunityRoleDefaults.Member;
                }

                foreach (var code in CommunityRoleDefaults.GetDefaultPermissionCodes(roleName))
                {
                    if (!permissionIds.TryGetValue(code, out var permissionId))
                    {
                        throw new InvalidOperationException(
                            $"Permission '{code}' has not been seeded.");
                    }

                    var linkExists = existingLinks.Any(x =>
                        x.RoleId == role.Id
                        && x.PermissionId == permissionId);

                    if (linkExists)
                    {
                        continue;
                    }

                    var link = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    };

                    dbContext.RolePermissions.Add(link);
                    existingLinks.Add(link);
                }
            }

            var ownerRole = rolesByName[CommunityRoleDefaults.Owner];

            var ownerMembershipExists = await dbContext.CommunityMembers
                .AnyAsync(
                    x => x.CommunityId == community.Id
                        && x.UserId == community.OwnerId,
                    cancellationToken);

            if (!ownerMembershipExists)
            {
                dbContext.CommunityMembers.Add(new CommunityMember
                {
                    CommunityId = community.Id,
                    UserId = community.OwnerId,
                    JoinedAt = DateTime.UtcNow
                });
            }

            var ownerAssignmentExists = await dbContext.CommunityMemberRoles
                .AnyAsync(
                    x => x.CommunityId == community.Id
                        && x.UserId == community.OwnerId
                        && x.RoleId == ownerRole.Id,
                    cancellationToken);

            if (!ownerAssignmentExists)
            {
                dbContext.CommunityMemberRoles.Add(new CommunityMemberRole
                {
                    CommunityId = community.Id,
                    UserId = community.OwnerId,
                    RoleId = ownerRole.Id
                });
            }

            // Mọi member phải có ít nhất một role; member chưa có role
            // nhận role mặc định (Member).
            var memberRole = rolesByName[CommunityRoleDefaults.Member];

            var rolelessUserIds = await dbContext.CommunityMembers
                .Where(x => x.CommunityId == community.Id
                    && !x.Roles.Any())
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            foreach (var userId in rolelessUserIds)
            {
                dbContext.CommunityMemberRoles.Add(new CommunityMemberRole
                {
                    CommunityId = community.Id,
                    UserId = userId,
                    RoleId = memberRole.Id
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
