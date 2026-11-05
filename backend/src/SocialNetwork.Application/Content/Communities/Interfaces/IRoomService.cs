using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IRoomService
{
    Task<IReadOnlyList<RoomResponse>> GetRoomsAsync(
        Guid currentUserId,
        Guid communityId,
        CancellationToken cancellationToken = default);

    Task<RoomResponse> GetRoomAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<RoomResponse> CreateAsync(
        Guid currentUserId,
        Guid communityId,
        CreateRoomRequest request,
        CancellationToken cancellationToken = default);

    Task<RoomResponse> UpdateAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        UpdateRoomRequest request,
        CancellationToken cancellationToken = default);

    Task<RoomResponse> UpdateStatusAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        UpdateRoomStatusRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomResponse>> ReorderAsync(
        Guid currentUserId,
        Guid communityId,
        ReorderRoomsRequest request,
        CancellationToken cancellationToken = default);
}
