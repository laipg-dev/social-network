namespace SocialNetwork.Application.Common.Storage;

/// <summary>Provider-neutral operations over private stored objects.</summary>
public interface IStorageService
{
    Task<StoredFile> UploadAsync(FileUploadRequest file, string category, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<StorageObjectMetadata?> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<SignedStorageUrl> CreateSignedUrlAsync(string storageKey, Uri requestBaseUri, CancellationToken cancellationToken = default);
    Task<Stream> OpenSignedReadAsync(string storageKey, long expiresAt, string signature, CancellationToken cancellationToken = default);
}

public sealed record SignedStorageUrl(string Url, DateTime ExpiresAtUtc);
