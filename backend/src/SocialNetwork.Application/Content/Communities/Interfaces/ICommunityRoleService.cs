using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface ICommunityRoleService
{
    Task<IReadOnlyList<CommunityRoleResponse>> GetRolesAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<CommunityRoleResponse> GetRoleAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<CommunityRoleResponse> CreateAsync(
        Guid currentUserId,
        Guid communityId,
        CreateCommunityRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<CommunityRoleResponse> UpdateAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        UpdateCommunityRoleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        Guid? reassignToRoleId,
        CancellationToken cancellationToken = default);

    Task<CommunityRoleResponse> SetDefaultAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommunityRoleResponse>> ReorderAsync(
        Guid currentUserId,
        Guid communityId,
        ReorderCommunityRolesRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<CommunityRoleResponse> ReplacePermissionsAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roleId,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default);

    Task<CommunityMemberResponse> AssignMemberRolesAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        UpdateMemberRolesRequest request,
        CancellationToken cancellationToken = default);
}
