namespace SocialNetwork.Application.Common.Storage;

public sealed record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);