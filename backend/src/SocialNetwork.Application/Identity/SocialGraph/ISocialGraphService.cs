using SocialNetwork.Application.Common.Responses;

namespace SocialNetwork.Application.Identity.SocialGraph;

public sealed record PublicUserResponse(Guid UserId, string Username, string? DisplayName, string? Bio, string? AvatarUrl, DateTime CreatedAt);
public sealed record FriendRequestResponse(Guid Id, Guid SenderId, Guid ReceiverId, string Status, DateTime CreatedAt);
public sealed record FollowStatusResponse(bool IsFollowing);
public sealed record FriendshipStatusResponse(string Status);

public interface ISocialGraphService
{
    Task<PublicUserResponse> GetPublicProfileAsync(Guid userId, Guid? viewerId, CancellationToken ct);
    Task<PagedResponse<PublicUserResponse>> SearchAsync(string? query, int page, int pageSize, Guid viewerId, CancellationToken ct);
    Task FollowAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task UnfollowAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task<PagedResponse<PublicUserResponse>> GetFollowersAsync(Guid userId, int page, int pageSize, Guid viewerId, CancellationToken ct);
    Task<PagedResponse<PublicUserResponse>> GetFollowingAsync(Guid userId, int page, int pageSize, Guid viewerId, CancellationToken ct);
    Task<FollowStatusResponse> GetFollowStatusAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task<FriendshipStatusResponse> GetFriendshipStatusAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task<FriendRequestResponse> SendFriendRequestAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task<FriendRequestResponse> RespondFriendRequestAsync(Guid viewerId, Guid requestId, bool accept, CancellationToken ct);
    Task CancelFriendRequestAsync(Guid viewerId, Guid requestId, CancellationToken ct);
    Task RemoveFriendAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task<PagedResponse<PublicUserResponse>> GetFriendsAsync(Guid userId, int page, int pageSize, Guid viewerId, CancellationToken ct);
    Task<PagedResponse<FriendRequestResponse>> GetFriendRequestsAsync(Guid viewerId, bool incoming, int page, int pageSize, CancellationToken ct);
    Task BlockAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task UnblockAsync(Guid viewerId, Guid userId, CancellationToken ct);
    Task<PagedResponse<PublicUserResponse>> GetBlockedAsync(Guid viewerId, int page, int pageSize, CancellationToken ct);
}
