using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Application.Common.Text;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Models;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Application.Content.Communities.Services;

public class CommunityService : ICommunityService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;
    private const string DefaultSlug = "community";
    private const int MaxSlugBaseLength = 190;

    private readonly ICommunityRepository _communityRepository;
    private readonly IPermissionCatalogRepository _permissionRepository;
    private readonly ICommunityAuthorization _authorization;

    public CommunityService(
        ICommunityRepository communityRepository,
        IPermissionCatalogRepository permissionRepository,
        ICommunityAuthorization authorization)
    {
        _communityRepository = communityRepository;
        _permissionRepository = permissionRepository;
        _authorization = authorization;
    }

    public async Task<CommunityResponse> CreateAsync(
        Guid currentUserId,
        CreateCommunityRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(
            request.Name,
            nameof(CreateCommunityRequest.Name));

        var slug = await GenerateUniqueSlugAsync(
            name,
            cancellationToken);

        var permissionIds = await _permissionRepository.GetIdsByCodesAsync(
            CommunityPermissions.All,
            cancellationToken);

        var community = new Community
        {
            OwnerId = currentUserId,
            Name = name,
            Slug = slug,
            Status = CommunityStatus.Active,
            Description = NormalizeOptional(request.Description),
            IsPublic = request.IsPublic,
            AvatarUrl = NormalizeOptional(request.AvatarUrl),
            CoverUrl = NormalizeOptional(request.CoverUrl)
        };

        // Bootstrap bộ role mặc định theo thiết kế quyền.
        var roles = CommunityRoleDefaults.CreateRoles(
            community.Id,
            permissionIds);

        foreach (var role in roles)
        {
            community.Roles.Add(role);
        }

        var ownerRole = roles.First(role =>
            role.Name == CommunityRoleDefaults.Owner);

        var ownerMembership = new CommunityMember
        {
            CommunityId = community.Id,
            UserId = currentUserId,
            JoinedAt = DateTime.UtcNow
        };

        // Người tạo cộng đồng mặc định là Owner.
        ownerMembership.Roles.Add(new CommunityMemberRole
        {
            CommunityId = community.Id,
            UserId = currentUserId,
            RoleId = ownerRole.Id
        });

        community.Members.Add(ownerMembership);

        await _communityRepository.AddAsync(
            community,
            cancellationToken);

        var created = await _communityRepository.GetByIdAsync(
            community.Id,
            cancellationToken)
            ?? throw new NotFoundException("Community", community.Id);

        return ToResponse(
            created,
            memberCount: 1,
            CommunityPermissions.All,
            CommunityMemberStatus.Active,
            [
                new CommunityRoleSummary
                {
                    Id = ownerRole.Id,
                    Name = ownerRole.Name,
                    Position = ownerRole.Position
                }
            ]);
    }

    public async Task<CommunityResponse> GetByIdAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        var community = await _communityRepository.GetByIdAsync(
            communityId,
            cancellationToken)
            ?? throw new NotFoundException("Community", communityId);

        var access = await _authorization.GetAccessAsync(
            communityId,
            currentUserId,
            cancellationToken);

        if (access.IsBanned)
        {
            throw new NotFoundException("Community", communityId);
        }

        // Cộng đồng private chỉ hiển thị với người có membership
        // (kể cả PENDING để FE hiển thị trạng thái chờ duyệt).
        if (!community.IsPublic && !access.HasMembership)
        {
            throw new NotFoundException("Community", communityId);
        }

        var memberCount = await _communityRepository.GetMemberCountAsync(
            communityId,
            cancellationToken);

        return ToResponse(
            community,
            memberCount,
            access.Permissions,
            access.MembershipStatus,
            access.Roles);
    }

    public async Task<PagedResponse<CommunityResponse>> GetPagedAsync(
        Guid currentUserId,
        CommunityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var page = parameters.Page < 1
            ? DefaultPage
            : parameters.Page;

        var pageSize = parameters.PageSize < 1
            ? DefaultPageSize
            : Math.Min(parameters.PageSize, MaxPageSize);

        var (items, totalCount) = await _communityRepository.GetPagedAsync(
            currentUserId,
            NormalizeOptional(parameters.Search),
            page,
            pageSize,
            cancellationToken);

        var memberCounts = items.Count == 0
            ? new Dictionary<Guid, int>()
            : await _communityRepository.GetMemberCountsAsync(
                items.Select(x => x.Id).ToArray(),
                cancellationToken);

        return new PagedResponse<CommunityResponse>
        {
            Items = items
                .Select(community => ToResponse(
                    community,
                    memberCounts.TryGetValue(community.Id, out var count)
                        ? count
                        : 0,
                    // Danh sách không truy vấn permission cho từng item
                    // để tránh N+1; chi tiết community mới trả MyPermissions.
                    Array.Empty<string>()))
                .ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CommunityResponse> UpdateAsync(
        Guid currentUserId,
        Guid communityId,
        UpdateCommunityRequest request,
        CancellationToken cancellationToken = default)
    {
        var community = await GetCommunityAsync(
            communityId,
            cancellationToken);

        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.CommunityEdit,
            cancellationToken);

        community.Name = NormalizeName(
            request.Name,
            nameof(UpdateCommunityRequest.Name));
        community.Description = NormalizeOptional(request.Description);
        community.IsPublic = request.IsPublic;
        community.AvatarUrl = NormalizeOptional(request.AvatarUrl);
        community.CoverUrl = NormalizeOptional(request.CoverUrl);
        community.UpdatedAt = DateTime.UtcNow;

        await _communityRepository.UpdateAsync(
            community,
            cancellationToken);

        var memberCount = await _communityRepository.GetMemberCountAsync(
            communityId,
            cancellationToken);

        return ToResponse(
            community,
            memberCount,
            access.Permissions,
            access.MembershipStatus,
            access.Roles);
    }

    public async Task<CommunityResponse> UpdateStatusAsync(
        Guid currentUserId,
        Guid communityId,
        UpdateCommunityStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);

        var community = await GetCommunityAsync(
            communityId,
            cancellationToken);

        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.CommunityEdit,
            cancellationToken);

        if (!string.Equals(
                community.Status,
                status,
                StringComparison.Ordinal))
        {
            community.Status = status;
            community.UpdatedAt = DateTime.UtcNow;

            await _communityRepository.UpdateAsync(
                community,
                cancellationToken);
        }

        var memberCount = await _communityRepository.GetMemberCountAsync(
            communityId,
            cancellationToken);

        return ToResponse(
            community,
            memberCount,
            access.Permissions,
            access.MembershipStatus,
            access.Roles);
    }

    public async Task DeleteAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        var community = await GetCommunityAsync(
            communityId,
            cancellationToken);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.CommunityDelete,
            cancellationToken);

        await _communityRepository.DeleteAsync(
            community,
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

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(name);

        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = DefaultSlug;
        }

        if (baseSlug.Length > MaxSlugBaseLength)
        {
            baseSlug = baseSlug[..MaxSlugBaseLength].TrimEnd('-');
        }

        var takenSlugs = await _communityRepository
            .GetSlugsWithPrefixAsync(
                baseSlug,
                cancellationToken);

        if (!takenSlugs.Contains(baseSlug, StringComparer.Ordinal))
        {
            return baseSlug;
        }

        var suffix = 2;

        while (takenSlugs.Contains(
            $"{baseSlug}-{suffix}",
            StringComparer.Ordinal))
        {
            suffix++;
        }

        return $"{baseSlug}-{suffix}";
    }

    private static string NormalizeName(
        string? name,
        string fieldName)
    {
        var normalized = name?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [fieldName] = ["Name is required."]
                });
        }

        return normalized;
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = status?.Trim().ToUpperInvariant()
            ?? string.Empty;

        if (!CommunityStatus.All.Contains(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(UpdateCommunityStatusRequest.Status)] =
                        [$"Status must be one of: {string.Join(", ", CommunityStatus.All)}."]
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

    private static CommunityResponse ToResponse(
        Community community,
        int memberCount,
        IReadOnlyList<string> myPermissions,
        string? myMembershipStatus = null,
        IReadOnlyList<CommunityRoleSummary>? myRoles = null)
    {
        return new CommunityResponse
        {
            Id = community.Id,
            Name = community.Name,
            Slug = community.Slug,
            Description = community.Description,
            AvatarUrl = community.AvatarUrl,
            CoverUrl = community.CoverUrl,
            IsPublic = community.IsPublic,
            Status = community.Status,
            OwnerId = community.OwnerId,
            OwnerUsername = community.Owner?.Username ?? string.Empty,
            OwnerDisplayName = community.Owner?.Profile?.DisplayName,
            OwnerAvatarUrl = community.Owner?.Profile?.AvatarUrl,
            MemberCount = memberCount,
            MyMembershipStatus = myMembershipStatus,
            MyRoles = myRoles ?? Array.Empty<CommunityRoleSummary>(),
            MyPermissions = myPermissions,
            CreatedAt = community.CreatedAt,
            UpdatedAt = community.UpdatedAt
        };
    }
}
