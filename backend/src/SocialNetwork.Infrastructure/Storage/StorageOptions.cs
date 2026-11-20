namespace SocialNetwork.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string RootPath { get; init; } = "private-storage";
    public int SignedUrlExpirationMinutes { get; init; } = 15;
    public string SigningKey { get; init; } = string.Empty;
}
