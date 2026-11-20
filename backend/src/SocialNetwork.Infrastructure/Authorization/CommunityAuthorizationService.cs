using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities.Authorization;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Models;
using SocialNetwork.Domain.Communities.Enums;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Authorization;

public class CommunityAuthorizationService : ICommunityAuthorization
{
    private readonly AppDbContext _dbContext;

    public CommunityAuthorizationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommunityAccess> GetAccessAsync(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var community = await _dbContext.Communities
            .AsNoTracking()
            .Where(x => x.Id == communityId)
            .Select(x => new { x.OwnerId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Community", communityId);

        var membership = await _dbContext.CommunityMembers
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId
                && x.UserId == userId)
            .Select(x => new { x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        var roles = await _dbContext.CommunityMemberRoles
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId
                && x.UserId == userId)
            .OrderByDescending(x => x.Role.Position)
            .Select(x => new CommunityRoleSummary
            {
                Id = x.Role.Id,
                Name = x.Role.Name,
                Position = x.Role.Position
            })
            .ToListAsync(cancellationToken);

        var isOwner = community.OwnerId == userId;

        var isActiveMember = membership is not null
            && membership.Status == CommunityMemberStatus.Active;

        var hasMembership = membership is not null
            && membership.Status is CommunityMemberStatus.Active
                or CommunityMemberStatus.Pending;

        if (isOwner)
        {
            return new CommunityAccess
            {
                IsOwner = true,
                HasMembership = true,
                IsMember = true,
                MembershipStatus = membership?.Status
                    ?? CommunityMemberStatus.Active,
                Roles = roles,
                Permissions = CommunityPermissions.All
            };
        }

        if (!isActiveMember)
        {
            return new CommunityAccess
            {
                IsOwner = false,
                HasMembership = hasMembership,
                IsMember = false,
                MembershipStatus = membership?.Status,
                Roles = hasMembership ? roles : []
            };
        }

        var permissions = await _dbContext.CommunityMemberRoles
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId
                && x.UserId == userId)
            .SelectMany(x => x.Role.Permissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new CommunityAccess
        {
            IsOwner = false,
            HasMembership = true,
            IsMember = true,
            MembershipStatus = membership!.Status,
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<CommunityAccess> RequireCommunityPermissionAsync(
        Guid communityId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var access = await GetAccessAsync(
            communityId,
            userId,
            cancellationToken);

        if (access.IsOwner)
        {
            return access;
        }

        if (!access.IsMember)
        {
            throw new ForbiddenException(
                "You are not a member of this community.");
        }

        if (!access.Permissions.Contains(permission))
        {
            throw new ForbiddenException(
                "You do not have permission to perform this action.");
        }

        return access;
    }

    public async Task<RoomAccess> GetRoomAccessAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var room = await _dbContext.Rooms
            .AsNoTracking()
            .Where(x => x.Id == roomId)
            .Select(x => new { x.CommunityId, x.IsPublic })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Room", roomId);

        var access = await GetAccessAsync(
            room.CommunityId,
            userId,
            cancellationToken);

        var isRoomMember = await _dbContext.RoomMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.RoomId == roomId
                    && x.UserId == userId,
                cancellationToken);

        var roleIds = access.Roles
            .Select(x => x.Id)
            .ToArray();

        var overrides = await GetRoomOverridesAsync(
            roomId,
            roleIds,
            cancellationToken);

        return BuildRoomAccess(
            access,
            room.IsPublic,
            isRoomMember,
            overrides);
    }

    public async Task<IReadOnlyDictionary<Guid, RoomAccess>> GetRoomAccessesAsync(
        Guid communityId,
        Guid userId,
        IReadOnlyCollection<Guid> roomIds,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, RoomAccess>();

        if (roomIds.Count == 0)
        {
            return result;
        }

        var access = await GetAccessAsync(
            communityId,
            userId,
            cancellationToken);

        var rooms = await _dbContext.Rooms
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId
                && roomIds.Contains(x.Id))
            .Select(x => new { x.Id, x.IsPublic })
            .ToListAsync(cancellationToken);

        var joinedRoomIds = await _dbContext.RoomMembers
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && roomIds.Contains(x.RoomId))
            .Select(x => x.RoomId)
            .ToListAsync(cancellationToken);

        var roleIds = access.Roles
            .Select(x => x.Id)
            .ToArray();

        List<OverrideRow> overrideRows = [];

        if (roleIds.Length > 0)
        {
            overrideRows = await _dbContext.RoomRolePermissions
                .AsNoTracking()
                .Where(x => roomIds.Contains(x.RoomId)
                    && roleIds.Contains(x.RoleId))
                .Select(x => new OverrideRow
                {
                    RoomId = x.RoomId,
                    Code = x.Permission.Code,
                    IsAllowed = x.IsAllowed
                })
                .ToListAsync(cancellationToken);
        }

        foreach (var room in rooms)
        {
            var overrides = overrideRows
                .Where(x => x.RoomId == room.Id)
                .Select(x => new RoomPermissionOverride(
                    x.Code,
                    x.IsAllowed))
                .ToArray();

            result[room.Id] = BuildRoomAccess(
                access,
                room.IsPublic,
                joinedRoomIds.Contains(room.Id),
                overrides);
        }

        return result;
    }

    public async Task<RoomAccess> RequireRoomPermissionAsync(
        Guid roomId,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var access = await GetRoomAccessAsync(
            roomId,
            userId,
            cancellationToken);

        if (!access.CanView)
        {
            throw new NotFoundException("Room", roomId);
        }

        if (!access.Permissions.Contains(permission))
        {
            throw new ForbiddenException(
                "You do not have permission to perform this action.");
        }

        return access;
    }

    private async Task<IReadOnlyList<RoomPermissionOverride>> GetRoomOverridesAsync(
        Guid roomId,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        var rows = await _dbContext.RoomRolePermissions
            .AsNoTracking()
            .Where(x => x.RoomId == roomId
                && roleIds.Contains(x.RoleId))
            .Select(x => new { x.Permission.Code, x.IsAllowed })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new RoomPermissionOverride(
                x.Code,
                x.IsAllowed))
            .ToArray();
    }

    private static RoomAccess BuildRoomAccess(
        CommunityAccess communityAccess,
        bool isPublic,
        bool isRoomMember,
        IReadOnlyList<RoomPermissionOverride> overrides)
    {
        // Người không phải thành viên Active của Community
        // không được truy cập Room.
        if (!communityAccess.IsMember)
        {
            return new RoomAccess
            {
                CanView = false,
                IsRoomMember = false
            };
        }

        // Community Owner có toàn quyền.
        if (communityAccess.IsOwner)
        {
            return new RoomAccess
            {
                CanView = true,
                IsRoomMember = !isPublic && isRoomMember,
                Permissions = CommunityPermissions.All
            };
        }

        // RoomMember chỉ có ý nghĩa đối với Private Room.
        // Các RoomMember row cũ của Public Room không còn
        // được xem là access grant.
        var hasPrivateAccessGrant = !isPublic && isRoomMember;

        // Lấy quyền từ Role của thành viên Community.
        var permissions = new HashSet<string>(
            communityAccess.Permissions,
            StringComparer.Ordinal);

        // Chỉ áp dụng Override cho quyền hoạt động trong Room.
        // Deny luôn ưu tiên hơn Allow.
        foreach (var group in overrides
            .Where(x => RoomPermissions.Overridable.Contains(x.Code))
            .GroupBy(x => x.Code, StringComparer.Ordinal))
        {
            if (group.Any(x => !x.IsAllowed))
            {
                permissions.Remove(group.Key);
            }
            else
            {
                permissions.Add(group.Key);
            }
        }

        // Public Room:
        // Mọi thành viên Active của Community được truy cập.
        //
        // Private Room:
        // Người được cấp access grant hoặc người có quyền
        // quản lý Room / quản lý quyền truy cập.
        var canView = isPublic
            || hasPrivateAccessGrant
            || communityAccess.Permissions.Contains(
                CommunityPermissions.RoomManage)
            || communityAccess.Permissions.Contains(
                CommunityPermissions.RoomMemberManage);

        return new RoomAccess
        {
            CanView = canView,
            IsRoomMember = hasPrivateAccessGrant,
            Permissions = permissions.ToArray()
        };
    }

    private sealed class OverrideRow
    {
        public Guid RoomId { get; init; }

        public string Code { get; init; } = string.Empty;

        public bool IsAllowed { get; init; }
    }
}
