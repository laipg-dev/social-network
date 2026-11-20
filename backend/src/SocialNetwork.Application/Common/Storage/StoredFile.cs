namespace SocialNetwork.Application.Common.Storage;

public sealed record StoredFile(string StorageKey);

public sealed record StorageObjectMetadata(string StorageKey, long Length, DateTime LastModifiedUtc);
