namespace SocialNetwork.Application.Content.Media.Responses;

public sealed record MediaResponse(
    Guid Id,
    Guid UploaderId,
    string MediaType,
    string Url,
    string MimeType,
    long FileSize,
    string ProcessingStatus,
    DateTime CreatedAt);
