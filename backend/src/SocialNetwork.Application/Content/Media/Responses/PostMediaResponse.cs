namespace SocialNetwork.Application.Content.Media.Responses;

public sealed record PostMediaResponse(
    Guid Id,
    Guid PostId,
    string Url,
    string MediaType,
    int Position,
    DateTime CreatedAt);
