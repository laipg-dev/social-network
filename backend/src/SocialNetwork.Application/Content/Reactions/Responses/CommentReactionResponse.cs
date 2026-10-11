namespace SocialNetwork.Application.Content.Reactions.Responses;

public sealed record CommentReactionResponse(
    Guid CommentId,
    string? CurrentUserReaction,
    IReadOnlyDictionary<string, int> Counts,
    int TotalCount);
