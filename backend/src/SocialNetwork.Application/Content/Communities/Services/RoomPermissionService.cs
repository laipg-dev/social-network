using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities;
using SocialNetwork.Application.Content.Communities.Authorization;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Application.Content.Communities.Services;

public class RoomPermissionService : IRoomPermissionService
{
    private readonly IRoomRepository _roomRepository;
    private readonly ICommunityRoleRepository _roleRepository;
    private readonly IRoomPermissionRepository _roomPermissionRepository;
    private readonly IPermissionCatalogRepository _permissionRepository;
    private readonly ICommunityAuthorization _authorization;

    public RoomPermissionService(
        IRoomRepository roomRepository,
        ICommunityRoleRepository roleRepository,
        IRoomPermissionRepository roomPermissionRepository,
        IPermissionCatalogRepository permissionRepository,
        ICommunityAuthorization authorization)
    {
        _roomRepository = roomRepository;
        _roleRepository = roleRepository;
        _roomPermissionRepository = roomPermissionRepository;
        _permissionRepository = permissionRepository;
        _authorization = authorization;
    }

    public async Task<IReadOnlyList<RoomPermissionOverrideResponse>> GetOverridesAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRoomAsync(communityId, roomId, cancellationToken);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoomManage,
            cancellationToken);

        var overrides = await _roomPermissionRepository.GetByRoomAsync(
            roomId,
            cancellationToken);

        return overrides
            .Where(x => RoomPermissions.Overridable.Contains(
                x.Permission.Code))
            .Select(x => new RoomPermissionOverrideResponse
            {
                RoleId = x.RoleId,
                RoleName = x.Role.Name,
                Code = x.Permission.Code,
                IsAllowed = x.IsAllowed
            })
            .OrderBy(x => x.RoleName, StringComparer.Ordinal)
            .ThenBy(x => x.Code, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<IReadOnlyList<RoomPermissionOverrideResponse>> ReplaceRoleOverridesAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        Guid roleId,
        UpdateRoomRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureRoomAsync(communityId, roomId, cancellationToken);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoomManage,
            cancellationToken);

        var communityAccess = await _authorization.GetAccessAsync(
            communityId,
            currentUserId,
            cancellationToken);

        var role = await _roleRepository.GetAsync(
            communityId,
            roleId,
            includePermissions: false,
            cancellationToken)
            ?? throw new NotFoundException("Role", roleId);

        EnsureNotOwnerRole(role.Name);
        RolePositionRules.EnsureCanManagePosition(
            communityAccess,
            role.Position);

        var items = request.Overrides;

        var normalized = items
            .Select(item => new RoomPermissionOverrideItem
            {
                Code = item.Code?.Trim().ToUpperInvariant()
                    ?? string.Empty,
                IsAllowed = item.IsAllowed
            })
            .ToArray();

        if (normalized.Any(item => !RoomPermissions.Overridable.Contains(item.Code)))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Overrides"] =
                        [$"Only these permissions can be overridden: {string.Join(", ", RoomPermissions.Overridable)}."]
                });
        }

        if (normalized.Select(x => x.Code).Distinct().Count() != normalized.Length)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Overrides"] = ["Code must be unique in the request."]
                });
        }

        var codes = normalized.Select(x => x.Code).ToArray();

        var permissions = await _permissionRepository.GetByCodesAsync(
            codes,
            cancellationToken);

        var foundCodes = permissions
            .Select(x => x.Code)
            .ToHashSet(StringComparer.Ordinal);

        var unknownCodes = codes
            .Where(code => !foundCodes.Contains(code))
            .ToArray();

        if (unknownCodes.Length > 0)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Overrides"] =
                        [$"Unknown permission(s): {string.Join(", ", unknownCodes)}."]
                });
        }

        if (!communityAccess.IsOwner)
        {
            var forbiddenCodes = codes
                .Where(code => !communityAccess.Permissions.Contains(code))
                .ToArray();

            if (forbiddenCodes.Length > 0)
            {
                throw new ForbiddenException(
                    $"You cannot override permissions you do not have: {string.Join(", ", forbiddenCodes)}.");
            }
        }

        var allowedByCode = normalized.ToDictionary(
            x => x.Code,
            x => x.IsAllowed,
            StringComparer.Ordinal);

        await _roomPermissionRepository.ReplaceRoleOverridesAsync(
            roomId,
            roleId,
            permissions
                .Select(permission => (
                    permission.Id,
                    allowedByCode[permission.Code]))
                .ToArray(),
            cancellationToken);

        return permissions
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .Select(permission => new RoomPermissionOverrideResponse
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Code = permission.Code,
                IsAllowed = allowedByCode[permission.Code]
            })
            .ToArray();
    }

    public async Task RemoveOverrideAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        Guid roleId,
        string code,
        CancellationToken cancellationToken = default)
    {
        await EnsureRoomAsync(communityId, roomId, cancellationToken);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoomManage,
            cancellationToken);

        var communityAccess = await _authorization.GetAccessAsync(
            communityId,
            currentUserId,
            cancellationToken);

        var role = await _roleRepository.GetAsync(
            communityId,
            roleId,
            includePermissions: false,
            cancellationToken)
            ?? throw new NotFoundException("Role", roleId);

        EnsureNotOwnerRole(role.Name);
        RolePositionRules.EnsureCanManagePosition(
            communityAccess,
            role.Position);

        var normalized = code?.Trim().ToUpperInvariant()
            ?? string.Empty;

        if (!RoomPermissions.Overridable.Contains(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["code"] =
                    [
                        $"Permission '{normalized}' cannot be overridden at room level."
                    ]
                });
        }

        var permissions = await _permissionRepository.GetByCodesAsync(
            [normalized],
            cancellationToken);

        var permission = permissions.FirstOrDefault()
            ?? throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["code"] = [$"Unknown permission '{code}'."]
                });

        await _roomPermissionRepository.RemoveOverrideAsync(
            roomId,
            roleId,
            permission.Id,
            cancellationToken);
    }

    private async Task EnsureRoomAsync(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        _ = await _roomRepository.GetAsync(
            communityId,
            roomId,
            cancellationToken)
            ?? throw new NotFoundException("Room", roomId);
    }

    private static void EnsureNotOwnerRole(string roleName)
    {
        if (string.Equals(
                roleName,
                CommunityRoleDefaults.Owner,
                StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                "The Owner role cannot have room permission overrides.");
        }
    }
}
