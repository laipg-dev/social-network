using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface ICommunityMemberService
{
    Task<CommunityMemberResponse> JoinAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task LeaveAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<CommunityMemberResponse> GetMeAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<CommunityMemberResponse> GetMemberAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<CommunityMemberResponse>> GetPagedAsync(
        Guid currentUserId,
        Guid communityId,
        CommunityMemberQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CommunityMemberResponse> ApproveAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CommunityMemberResponse> RejectAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task KickAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CommunityMemberResponse> UpdateStatusAsync(
        Guid currentUserId,
        Guid communityId,
        Guid userId,
        UpdateCommunityMemberStatusRequest request,
        CancellationToken cancellationToken = default);
}
