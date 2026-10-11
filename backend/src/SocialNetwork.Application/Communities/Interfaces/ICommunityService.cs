using SocialNetwork.Application.Communities.Requests;
using SocialNetwork.Application.Communities.Responses;

namespace SocialNetwork.Application.Communities.Interfaces;

public interface ICommunityService
{
    Task<IReadOnlyList<CommunityResponse>> GetPageAsync(
        Guid? userId,
        string? search,
        CancellationToken cancellationToken);

    Task<CommunityResponse> GetByIdAsync(
        Guid id,
        Guid? userId,
        CancellationToken cancellationToken);

    Task<CommunityResponse> CreateAsync(
        Guid ownerId,
        CreateCommunityRequest request,
        CancellationToken cancellationToken);

    Task<CommunityResponse> UpdateAsync(
        Guid id,
        Guid ownerId,
        UpdateCommunityRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);
    Task JoinAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task LeaveAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
