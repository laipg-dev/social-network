using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface ICommunityService
{
    Task<CommunityResponse> CreateAsync(
        Guid currentUserId,
        CreateCommunityRequest request,
        CancellationToken cancellationToken = default);

    Task<CommunityResponse> GetByIdAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<CommunityResponse>> GetPagedAsync(
        Guid currentUserId,
        CommunityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CommunityResponse> UpdateAsync(
        Guid currentUserId,
        Guid communityId,
        UpdateCommunityRequest request,
        CancellationToken cancellationToken = default);

    Task<CommunityResponse> UpdateStatusAsync(
        Guid currentUserId,
        Guid communityId,
        UpdateCommunityStatusRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default);
}
