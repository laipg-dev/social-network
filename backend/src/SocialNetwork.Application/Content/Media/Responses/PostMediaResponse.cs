namespace SocialNetwork.Application.Content.Media.Responses;

public sealed record PostMediaResponse(
    Guid Id,
    Guid PostId,
    string? Url,
    DateTime? ExpiresAtUtc,
    string MediaType,
    int Position,
    DateTime CreatedAt);
