using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public interface IRoomMemberService
{
    Task<IReadOnlyList<RoomMemberResponse>> GetMembersAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<RoomMemberResponse> GetMeAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<RoomMemberResponse> JoinAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task LeaveAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    Task<RoomMemberResponse> AddAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        AddRoomMemberRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
