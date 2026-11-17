using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Models;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Application.Content.Communities.Services;

public class CommunityMemberService : ICommunityMemberService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly ICommunityRepository _communityRepository;
    private readonly ICommunityMemberRepository _memberRepository;
    private readonly ICommunityAuthorization _authorization;

    public CommunityMemberService(
        ICommunityRepository communityRepository,
        ICommunityMemberRepository memberRepository,
        ICommunityAuthorization authorization)
    {
        _communityRepository = communityRepository;
        _memberRepository = memberRepository;
        _authorization = authorization;
    }

    public async Task<CommunityMemberResponse> JoinAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        var community = await GetCommunityAsync(
            communityId,
            cancellationToken);

        if (!string.Equals(
                community.Status,
                CommunityStatus.Active,
                StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                "This community is closed and does not accept new members.");
        }

        var member = await _memberRepository.GetAsync(
            communityId,
            currentUserId,
            includeRoles: true,
            cancellationToken);

        if (member is not null
            && member.Status == CommunityMemberStatus.Active)
        {
            throw new ConflictException(
                "You are already a member of this community.");
        }

        if (member is not null
            && member.Status == CommunityMemberStatus.Pending)
        {
            throw new ConflictException(
                "Your join request is already pending.");
        }

        var now = DateTime.UtcNow;
        var becomesActive = community.IsPublic;

        if (member is null)
        {
            member = new CommunityMember
            {
                CommunityId = communityId,
                UserId = currentUserId,
                Status = becomesActive
                    ? CommunityMemberStatus.Active
                    : CommunityMemberStatus.Pending,
                RequestedAt = now,
                JoinedAt = becomesActive ? now : null
            };

            if (becomesActive)
            {
                await EnsureDefaultRoleAsync(
                    member,
                    communityId,
                    cancellationToken);
            }

            await _memberRepository.AddAsync(
                member,
                cancellationToken);
        }
        else
        {
            member.Status = becomesActive
                ? CommunityMemberStatus.Active
                : CommunityMemberStatus.Pending;
            member.RequestedAt = now;
            member.JoinedAt = becomesActive ? now : null;
            member.LeftAt = null;
            member.UpdatedAt = now;

            if (becomesActive)
            {
                await EnsureDefaultRoleAsync(
                    member,
                    communityId,
                    cancellationToken);
            }

            await _memberRepository.UpdateAsync(
                member,
                cancellationToken);
        }

        return await ReloadAsync(
            communityId,
            currentUserId,
            cancellationToken);
    }

    public async Task LeaveAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        var community = await GetCommunityAsync(
            communityId,
            cancellationToken);

        if (community.OwnerId == currentUserId)
        {
            throw new BusinessRuleException(
                "The community owner cannot leave. Transfer ownership first.");
        }

        var member = await _memberRepository.GetAsync(
            communityId,
            currentUserId,
            includeRoles: true,
            cancellationToken);

        if (member is null
            || member.Status is CommunityMemberStatus.Left
                or CommunityMemberStatus.Rejected)
        {
            throw new NotFoundException(
                "Membership",
                currentUserId);
        }

        await MarkAsLeftAsync(
            member,
            cancellationToken);

        await _memberRepository.UpdateAsync(
            member,
            cancellationToken);
    }

    public async Task<CommunityMemberResponse> GetMeAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetAsync(
            communityId,
            currentUserId,
            includeRoles: true,
            cancellationToken)
            ?? throw new NotFoundException(
                "Membership",
                currentUserId);

        return ToResponse(member);
    }

    public async Task<CommunityMemberResponse> GetMemberAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.MemberView,
            cancellationToken);

        var member = await _memberRepository.GetAsync(
            communityId,
            userId,
            includeRoles: true,
            cancellationToken)
            ?? throw new NotFoundException("Membership", userId);

        return ToResponse(member);
    }

    public async Task<PagedResponse<CommunityMemberResponse>> GetPagedAsync(
        Guid currentUserId,
        Guid communityId,
        CommunityMemberQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.MemberView,
            cancellationToken);

        var status = NormalizeStatusFilter(parameters.Status);

        var page = parameters.Page < 1
            ? DefaultPage
            : parameters.Page;

        var pageSize = parameters.PageSize < 1
            ? DefaultPageSize
            : Math.Min(parameters.PageSize, MaxPageSize);

        var (items, totalCount) = await _memberRepository.GetPagedAsync(
            communityId,
            status,
            parameters.RoleId,
            NormalizeOptional(parameters.Search),
            page,
            pageSize,
            cancellationToken);

        return new PagedResponse<CommunityMemberResponse>
        {
            Items = items.Select(ToResponse).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CommunityMemberResponse> ApproveAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.MemberManage,
            cancellationToken);

        var member = await GetMemberForUpdateAsync(
            communityId,
            userId,
            cancellationToken);

        if (member.Status != CommunityMemberStatus.Pending)
        {
            throw new BusinessRuleException(
                "Only pending join requests can be approved.");
        }

        member.Status = CommunityMemberStatus.Active;
        member.JoinedAt = DateTime.UtcNow;
        member.LeftAt = null;
        member.UpdatedAt = DateTime.UtcNow;

        await EnsureDefaultRoleAsync(
            member,
            communityId,
            cancellationToken);

        await _memberRepository.UpdateAsync(
            member,
            cancellationToken);

        return await ReloadAsync(
            communityId,
            userId,
            cancellationToken);
    }

    public async Task<CommunityMemberResponse> RejectAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.MemberManage,
            cancellationToken);

        var member = await GetMemberForUpdateAsync(
            communityId,
            userId,
            cancellationToken);

        if (member.Status != CommunityMemberStatus.Pending)
        {
            throw new BusinessRuleException(
                "Only pending join requests can be rejected.");
        }

        member.Status = CommunityMemberStatus.Rejected;
        member.UpdatedAt = DateTime.UtcNow;

        await _memberRepository.UpdateAsync(
            member,
            cancellationToken);

        return await ReloadAsync(
            communityId,
            userId,
            cancellationToken);
    }

    public async Task KickAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.MemberManage,
            cancellationToken);

        if (userId == currentUserId)
        {
            throw new BusinessRuleException(
                "Use the leave endpoint to leave the community.");
        }

        var community = await GetCommunityAsync(
            communityId,
            cancellationToken);

        if (community.OwnerId == userId)
        {
            throw new BusinessRuleException(
                "The community owner cannot be removed.");
        }

        var member = await GetMemberForUpdateAsync(
            communityId,
            userId,
            cancellationToken);

        if (member.Status is CommunityMemberStatus.Left
            or CommunityMemberStatus.Rejected)
        {
            throw new BusinessRuleException(
                "This user is not an active member of the community.");
        }

        await MarkAsLeftAsync(member, cancellationToken);

        await _memberRepository.UpdateAsync(
            member,
            cancellationToken);
    }

    public async Task<CommunityMemberResponse> UpdateStatusAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        UpdateCommunityMemberStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.MemberManage,
            cancellationToken);

        if (userId == currentUserId)
        {
            throw new BusinessRuleException(
                "Use the leave endpoint to change your own membership.");
        }

        var community = await GetCommunityAsync(
            communityId,
            cancellationToken);

        if (community.OwnerId == userId)
        {
            throw new BusinessRuleException(
                "The community owner status cannot be changed.");
        }

        var member = await GetMemberForUpdateAsync(
            communityId,
            userId,
            cancellationToken);

        if (status == CommunityMemberStatus.Active)
        {
            member.Status = CommunityMemberStatus.Active;
            member.JoinedAt ??= DateTime.UtcNow;
            member.LeftAt = null;
            member.UpdatedAt = DateTime.UtcNow;

            await EnsureDefaultRoleAsync(
                member,
                communityId,
                cancellationToken);
        }
        else
        {
            await MarkAsLeftAsync(member, cancellationToken);
        }

        await _memberRepository.UpdateAsync(
            member,
            cancellationToken);

        return await ReloadAsync(
            communityId,
            userId,
            cancellationToken);
    }

    private async Task<Community> GetCommunityAsync(
        Guid communityId,
        CancellationToken cancellationToken)
    {
        return await _communityRepository.GetByIdAsync(
            communityId,
            cancellationToken)
            ?? throw new NotFoundException("Community", communityId);
    }

    private async Task<CommunityMember> GetMemberForUpdateAsync(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _memberRepository.GetAsync(
            communityId,
            userId,
            includeRoles: true,
            cancellationToken)
            ?? throw new NotFoundException("Membership", userId);
    }

    private async Task MarkAsLeftAsync(
        CommunityMember member,
        CancellationToken cancellationToken)
    {
        member.Status = CommunityMemberStatus.Left;
        member.LeftAt = DateTime.UtcNow;
        member.UpdatedAt = DateTime.UtcNow;

        await _memberRepository.RemoveRolesAsync(
            member,
            cancellationToken);
    }

    private async Task EnsureDefaultRoleAsync(
        CommunityMember member,
        Guid communityId,
        CancellationToken cancellationToken)
    {
        if (member.Roles.Count > 0)
        {
            return;
        }

        var defaultRole = await _memberRepository.GetDefaultRoleAsync(
            communityId,
            cancellationToken)
            ?? throw new BusinessRuleException(
                "This community has no default role configured.");

        member.Roles.Add(new CommunityMemberRole
        {
            CommunityId = communityId,
            UserId = member.UserId,
            RoleId = defaultRole.Id
        });
    }

    private async Task<CommunityMemberResponse> ReloadAsync(
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetAsync(
            communityId,
            userId,
            includeRoles: true,
            cancellationToken)
            ?? throw new NotFoundException("Membership", userId);

        return ToResponse(member);
    }

    private static CommunityMemberResponse ToResponse(
        CommunityMember member)
    {
        return new CommunityMemberResponse
        {
            UserId = member.UserId,
            Username = member.User?.Username ?? string.Empty,
            DisplayName = member.User?.Profile?.DisplayName,
            AvatarUrl = member.User?.Profile?.AvatarUrl,
            Status = member.Status,
            RequestedAt = member.RequestedAt,
            JoinedAt = member.JoinedAt,
            LeftAt = member.LeftAt,
            Roles = member.Roles
                .OrderByDescending(x => x.Role.Position)
                .Select(x => new CommunityRoleSummary
                {
                    Id = x.Role.Id,
                    Name = x.Role.Name,
                    Position = x.Role.Position
                })
                .ToArray()
        };
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = status?.Trim().ToUpperInvariant()
            ?? string.Empty;

        if (normalized is not (
            CommunityMemberStatus.Active
            or CommunityMemberStatus.Left))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(UpdateCommunityMemberStatusRequest.Status)] =
                        ["Status must be one of: ACTIVE, LEFT."]
                });
        }

        return normalized;
    }

    private static string? NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var normalized = status.Trim().ToUpperInvariant();

        if (!CommunityMemberStatus.All.Contains(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(CommunityMemberQueryParameters.Status)] =
                        [$"Status must be one of: {string.Join(", ", CommunityMemberStatus.All)}."]
                });
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();

        return string.IsNullOrEmpty(normalized)
            ? null
            : normalized;
    }
}
