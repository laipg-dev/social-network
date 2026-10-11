namespace SocialNetwork.Application.Content.Posts.Responses;

public sealed record PostPageResponse(
    IReadOnlyList<PostResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
