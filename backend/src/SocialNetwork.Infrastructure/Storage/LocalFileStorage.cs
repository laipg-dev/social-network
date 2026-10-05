using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SocialNetwork.Application.Common.Storage;

namespace SocialNetwork.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly StorageOptions _options;
    private readonly IHostEnvironment _environment;

    public LocalFileStorage(
        IOptions<StorageOptions> options,
        IHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task<StoredFile> UploadAsync(
        FileUploadRequest file,
        string folder,
        CancellationToken cancellationToken = default)
    {
        // Lấy extension của file gốc.
        // Ví dụ: avatar.jpg -> .jpg
        var extension = Path.GetExtension(file.FileName);

        // Tạo tên file mới để tránh trùng tên.
        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        // Ví dụ:
        // avatars/abc123.jpg
        var relativePath =
            Path.Combine(folder, storedFileName);

        // Ví dụ:
        // D:\...\SocialNetwork.Api\uploads\avatars\abc123.jpg
        var fullPath =
            Path.Combine(
                _environment.ContentRootPath,
                _options.RootPath,
                relativePath);

        var directory =
            Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var output =
            new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

        await file.Content.CopyToAsync(
            output,
            cancellationToken);

        // Luôn dùng "/" cho key lưu trong database.
        var key =
            relativePath.Replace('\\', '/');

        // Ví dụ:
        // /uploads/avatars/abc123.jpg
        var url =
            $"{_options.RequestPath.TrimEnd('/')}/{key}";

        return new StoredFile(
            key,
            url);
    }

    public Task DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey =
            key.Replace(
                '/',
                Path.DirectorySeparatorChar);

        var fullPath =
            Path.Combine(
                _environment.ContentRootPath,
                _options.RootPath,
                normalizedKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}