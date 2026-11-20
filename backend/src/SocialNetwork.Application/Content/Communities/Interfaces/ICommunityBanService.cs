using SocialNetwork.Application.Common.Responses;

namespace SocialNetwork.Application.Content.Communities.Interfaces;

public sealed record CommunityBanResponse(Guid Id, Guid CommunityId, Guid UserId, string Username, string? DisplayName, Guid BannedById, string? Reason, DateTime CreatedAt, DateTime? ExpiresAt);
public sealed record BanCommunityMemberRequest(
    [property: System.ComponentModel.DataAnnotations.MaxLength(1000)] string? Reason,
    DateTime? ExpiresAt);
public interface ICommunityBanService
{
    Task<CommunityBanResponse> BanAsync(Guid actorId, Guid communityId, Guid userId, BanCommunityMemberRequest request, CancellationToken ct);
    Task UnbanAsync(Guid actorId, Guid communityId, Guid userId, CancellationToken ct);
    Task<PagedResponse<CommunityBanResponse>> GetBannedAsync(Guid actorId, Guid communityId, int page, int pageSize, CancellationToken ct);
    Task<PagedResponse<QuarantinedPostResponse>> GetQuarantinedPostsAsync(Guid actorId, Guid communityId, int page, int pageSize, Uri requestBaseUri, CancellationToken ct);
    Task RestorePostAsync(Guid actorId, Guid communityId, Guid postId, CancellationToken ct);
    Task<PagedResponse<QuarantinedCommentResponse>> GetQuarantinedCommentsAsync(Guid actorId, Guid communityId, int page, int pageSize, CancellationToken ct);
    Task RestoreCommentAsync(Guid actorId, Guid communityId, Guid commentId, CancellationToken ct);
}

public sealed record QuarantinedPostResponse(Guid Id, Guid AuthorId, string? Username, string? Content, DateTime CreatedAt, IReadOnlyList<QuarantinedMediaResponse> Media);
public sealed record QuarantinedMediaResponse(Guid Id, string? Url, DateTime? ExpiresAtUtc, string MediaType, int Position);
public sealed record QuarantinedCommentResponse(Guid Id, Guid PostId, Guid? ParentCommentId, Guid AuthorId, string Content, DateTime CreatedAt, DateTime QuarantinedAt);
