namespace SocialNetwork.Application.Content.Reactions.Responses;

public sealed record PostReactionResponse(
    Guid PostId,
    string? CurrentUserReaction,
    IReadOnlyDictionary<string, int> Counts,
    int TotalCount);
