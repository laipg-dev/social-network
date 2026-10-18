namespace SocialNetwork.Application.Content.Comments.Responses;

public sealed record CommentResponse(
    Guid Id,
    Guid PostId,
    Guid AuthorId,
    Guid? ParentCommentId,
    string Content,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<CommentResponse> Replies,
    string? AuthorUsername = null,
    string? AuthorDisplayName = null,
    string? AuthorAvatarUrl = null);
