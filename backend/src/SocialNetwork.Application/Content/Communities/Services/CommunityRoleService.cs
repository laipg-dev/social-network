using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities.Authorization;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Application.Content.Communities.Services;

public class CommunityRoleService : ICommunityRoleService
{
    private const int DefaultNewRolePosition = 10;

    private readonly ICommunityRepository _communityRepository;
    private readonly ICommunityRoleRepository _roleRepository;
    private readonly ICommunityMemberRepository _memberRepository;
    private readonly IPermissionCatalogRepository _permissionRepository;
    private readonly ICommunityAuthorization _authorization;

    public CommunityRoleService(
        ICommunityRepository communityRepository,
        ICommunityRoleRepository roleRepository,
        ICommunityMemberRepository memberRepository,
        IPermissionCatalogRepository permissionRepository,
        ICommunityAuthorization authorization)
    {
        _communityRepository = communityRepository;
        _roleRepository = roleRepository;
        _memberRepository = memberRepository;
        _permissionRepository = permissionRepository;
        _authorization = authorization;
    }

    public async Task<IReadOnlyList<CommunityRoleResponse>> GetRolesAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleView,
            cancellationToken);

        var roles = await _roleRepository.GetByCommunityAsync(
            communityId,
            cancellationToken);

        var counts = await _roleRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return roles
            .OrderByDescending(x => x.Position)
            .Select(role => ToResponse(
                role,
                counts.TryGetValue(role.Id, out var count) ? count : 0))
            .ToArray();
    }

    public async Task<CommunityRoleResponse> GetRoleAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleView,
            cancellationToken);

        var role = await GetRoleAsync(
            communityId,
            roleId,
            cancellationToken);

        var counts = await _roleRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return ToResponse(
            role,
            counts.TryGetValue(role.Id, out var count) ? count : 0);
    }

    public async Task<CommunityRoleResponse> CreateAsync(
        Guid currentUserId,
        Guid communityId,
        CreateCommunityRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleManage,
            cancellationToken);

        var name = NormalizeName(request.Name);
        var position = request.Position ?? DefaultNewRolePosition;

        EnsureCanManagePosition(access, position);

        if (await _roleRepository.NameExistsAsync(
                communityId,
                name,
                cancellationToken: cancellationToken))
        {
            throw new ConflictException(
                $"Role '{name}' already exists in this community.");
        }

        var role = new CommunityRole
        {
            Id = Guid.NewGuid(),
            CommunityId = communityId,
            Name = name,
            Position = position,
            IsDefault = false
        };

        await _roleRepository.AddAsync(role, cancellationToken);

        return ToResponse(role, memberCount: 0);
    }

    public async Task<CommunityRoleResponse> UpdateAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        UpdateCommunityRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleManage,
            cancellationToken);

        var role = await GetRoleAsync(
            communityId,
            roleId,
            cancellationToken);

        EnsureNotOwnerRole(role);
        EnsureCanManagePosition(access, role.Position);
        EnsureCanManagePosition(access, request.Position);

        var name = NormalizeName(request.Name);

        if (await _roleRepository.NameExistsAsync(
                communityId,
                name,
                role.Id,
                cancellationToken))
        {
            throw new ConflictException(
                $"Role '{name}' already exists in this community.");
        }

        role.Name = name;
        role.Position = request.Position;

        await _roleRepository.UpdateAsync(role, cancellationToken);

        var counts = await _roleRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return ToResponse(
            role,
            counts.TryGetValue(role.Id, out var count) ? count : 0);
    }

    public async Task DeleteAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        Guid? reassignToRoleId,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleManage,
            cancellationToken);

        var role = await GetRoleAsync(
            communityId,
            roleId,
            cancellationToken);

        EnsureNotOwnerRole(role);
        EnsureCanManagePosition(access, role.Position);

        if (role.IsDefault)
        {
            throw new BusinessRuleException(
                "The default role cannot be deleted.");
        }

        var counts = await _roleRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        var memberCount = counts.TryGetValue(role.Id, out var count)
            ? count
            : 0;

        if (memberCount > 0)
        {
            if (!reassignToRoleId.HasValue)
            {
                throw new ConflictException(
                    $"Role '{role.Name}' has {memberCount} member(s). Provide reassignToRoleId to move them.");
            }

            if (reassignToRoleId.Value == role.Id)
            {
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["reassignToRoleId"] =
                            ["A role cannot be reassigned to itself."]
                    });
            }

            var targetRole = await GetRoleAsync(
                communityId,
                reassignToRoleId.Value,
                cancellationToken);

            if (string.Equals(
                    targetRole.Name,
                    CommunityRoleDefaults.Owner,
                    StringComparison.Ordinal))
            {
                throw new BusinessRuleException(
                    "Members cannot be reassigned to the Owner role.");
            }

            EnsureCanManagePosition(access, targetRole.Position);

            await _roleRepository.ReassignMembersAsync(
                communityId,
                role.Id,
                targetRole.Id,
                cancellationToken);
        }

        await _roleRepository.DeleteAsync(role, cancellationToken);
    }

    public async Task<CommunityRoleResponse> SetDefaultAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleManage,
            cancellationToken);

        var role = await GetRoleAsync(
            communityId,
            roleId,
            cancellationToken);

        EnsureNotOwnerRole(role);
        EnsureCanManagePosition(access, role.Position);

        if (!role.IsDefault)
        {
            await _roleRepository.SetDefaultAsync(role, cancellationToken);
        }

        var counts = await _roleRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return ToResponse(
            role,
            counts.TryGetValue(role.Id, out var count) ? count : 0);
    }

    public async Task<IReadOnlyList<CommunityRoleResponse>> ReorderAsync(
        Guid currentUserId,
        Guid communityId,
        ReorderCommunityRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleManage,
            cancellationToken);

        var roles = await _roleRepository.GetByCommunityAsync(
            communityId,
            cancellationToken);

        var rolesById = roles.ToDictionary(x => x.Id);

        var items = request.Items;

        if (items.Select(x => x.RoleId).Distinct().Count() != items.Count)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Items"] = ["RoleId must be unique in the request."]
                });
        }

        if (items.Select(x => x.Position).Distinct().Count() != items.Count)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Items"] = ["Position must be unique in the request."]
                });
        }

        foreach (var item in items)
        {
            if (!rolesById.TryGetValue(item.RoleId, out var role))
            {
                throw new NotFoundException("Role", item.RoleId);
            }

            if (string.Equals(
                    role.Name,
                    CommunityRoleDefaults.Owner,
                    StringComparison.Ordinal))
            {
                throw new BusinessRuleException(
                    "The Owner role position cannot be changed.");
            }

            EnsureCanManagePosition(access, role.Position);
            EnsureCanManagePosition(access, item.Position);

            role.Position = item.Position;
        }

        await _roleRepository.SaveChangesAsync(cancellationToken);

        var counts = await _roleRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return roles
            .OrderByDescending(x => x.Position)
            .Select(role => ToResponse(
                role,
                counts.TryGetValue(role.Id, out var count) ? count : 0))
            .ToArray();
    }

    public async Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleView,
            cancellationToken);

        var role = await GetRoleAsync(
            communityId,
            roleId,
            cancellationToken);

        return MapPermissions(role);
    }

    public async Task<CommunityRoleResponse> ReplacePermissionsAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleManage,
            cancellationToken);

        var role = await GetRoleAsync(
            communityId,
            roleId,
            cancellationToken);

        EnsureNotOwnerRole(role);
        EnsureCanManagePosition(access, role.Position);

        var codes = request.Codes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

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
                    ["Codes"] =
                        [$"Unknown permission(s): {string.Join(", ", unknownCodes)}."]
                });
        }

        if (!access.IsOwner)
        {
            var forbiddenCodes = codes
                .Where(code => !access.Permissions.Contains(code))
                .ToArray();

            if (forbiddenCodes.Length > 0)
            {
                throw new ForbiddenException(
                    $"You cannot grant permissions you do not have: {string.Join(", ", forbiddenCodes)}.");
            }
        }

        await _roleRepository.ReplacePermissionsAsync(
            role,
            permissions.Select(x => x.Id).ToArray(),
            cancellationToken);

        var counts = await _roleRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return new CommunityRoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Position = role.Position,
            IsDefault = role.IsDefault,
            MemberCount = counts.TryGetValue(role.Id, out var count)
                ? count
                : 0,
            Permissions = permissions
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(MapPermission)
                .ToArray()
        };
    }

    public async Task<CommunityMemberResponse> AssignMemberRolesAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        UpdateMemberRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoleManage,
            cancellationToken);

        var community = await _communityRepository.GetByIdAsync(
            communityId,
            cancellationToken)
            ?? throw new NotFoundException("Community", communityId);

        if (community.OwnerId == userId)
        {
            throw new BusinessRuleException(
                "The owner's roles cannot be modified. Transfer ownership instead.");
        }

        var member = await _memberRepository.GetAsync(
            communityId,
            userId,
            includeRoles: true,
            cancellationToken)
            ?? throw new NotFoundException("Membership", userId);

        if (member.Status != CommunityMemberStatus.Active)
        {
            throw new BusinessRuleException(
                "Only active members can be assigned roles.");
        }

        var roles = await _roleRepository.GetByCommunityAsync(
            communityId,
            cancellationToken);

        var rolesById = roles.ToDictionary(x => x.Id);

        var requestedRoleIds = request.RoleIds
            .Distinct()
            .ToArray();

        foreach (var roleId in requestedRoleIds)
        {
            if (!rolesById.TryGetValue(roleId, out var role))
            {
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["RoleIds"] = [$"Role '{roleId}' does not belong to this community."]
                    });
            }

            if (string.Equals(
                    role.Name,
                    CommunityRoleDefaults.Owner,
                    StringComparison.Ordinal))
            {
                throw new BusinessRuleException(
                    "The Owner role cannot be assigned. Transfer ownership instead.");
            }

            EnsureCanManagePosition(access, role.Position);
        }

        // Không được đổi role của người có role ngang/cao hơn mình.
        foreach (var link in member.Roles)
        {
            if (rolesById.TryGetValue(link.RoleId, out var existingRole))
            {
                EnsureCanManagePosition(access, existingRole.Position);
            }
        }

        Guid[] finalRoleIds;

        if (requestedRoleIds.Length > 0)
        {
            finalRoleIds = requestedRoleIds;
        }
        else
        {
            var defaultRole = roles.FirstOrDefault(x => x.IsDefault)
                ?? throw new BusinessRuleException(
                    "This community has no default role.");

            finalRoleIds = [defaultRole.Id];
        }

        var currentRoleIds = member.Roles
            .Select(x => x.RoleId)
            .ToHashSet();

        var roleIdsToRemove = currentRoleIds
            .Where(roleId => !finalRoleIds.Contains(roleId))
            .ToArray();

        if (roleIdsToRemove.Length > 0)
        {
            await _memberRepository.RemoveRolesAsync(
                member,
                roleIdsToRemove,
                cancellationToken);
        }

        foreach (var roleId in finalRoleIds)
        {
            if (currentRoleIds.Contains(roleId))
            {
                continue;
            }

            member.Roles.Add(new CommunityMemberRole
            {
                CommunityId = communityId,
                UserId = userId,
                RoleId = roleId
            });
        }

        await _memberRepository.UpdateAsync(member, cancellationToken);

        var updated = await _memberRepository.GetAsync(
            communityId,
            userId,
            includeRoles: true,
            cancellationToken)
            ?? throw new NotFoundException("Membership", userId);

        return CommunityMemberMapper.ToResponse(updated);
    }

    private async Task<CommunityRole> GetRoleAsync(
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        return await _roleRepository.GetAsync(
            communityId,
            roleId,
            includePermissions: true,
            cancellationToken)
            ?? throw new NotFoundException("Role", roleId);
    }

    private static void EnsureNotOwnerRole(CommunityRole role)
    {
        if (string.Equals(
                role.Name,
                CommunityRoleDefaults.Owner,
                StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                "The Owner role cannot be modified or deleted.");
        }
    }

    private static void EnsureCanManagePosition(
        CommunityAccess access,
        int position)
    {
        RolePositionRules.EnsureCanManagePosition(access, position);
    }

    private static string NormalizeName(string? name)
    {
        var normalized = name?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(CreateCommunityRoleRequest.Name)] =
                        ["Name is required."]
                });
        }

        return normalized;
    }

    private static CommunityRoleResponse ToResponse(
        CommunityRole role,
        int memberCount)
    {
        return new CommunityRoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Position = role.Position,
            IsDefault = role.IsDefault,
            MemberCount = memberCount,
            Permissions = MapPermissions(role)
        };
    }

    private static IReadOnlyList<PermissionResponse> MapPermissions(
        CommunityRole role)
    {
        return role.Permissions
            .Where(x => x.Permission is not null)
            .OrderBy(x => x.Permission.Code, StringComparer.Ordinal)
            .Select(x => MapPermission(x.Permission))
            .ToArray();
    }

    private static PermissionResponse MapPermission(Permission permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            Code = permission.Code,
            Description = permission.Description
        };
    }
}
