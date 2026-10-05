namespace SocialNetwork.Application.Common.Storage;

public interface IFileStorage
{
    Task<StoredFile> UploadAsync(
        FileUploadRequest file,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string key,
        CancellationToken cancellationToken = default);
}