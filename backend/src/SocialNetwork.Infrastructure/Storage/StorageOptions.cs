namespace SocialNetwork.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string RootPath { get; init; } = "uploads";

    public string RequestPath { get; init; } = "/uploads";
}