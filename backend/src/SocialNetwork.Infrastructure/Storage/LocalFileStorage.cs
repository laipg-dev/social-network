using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.Common.Storage;

namespace SocialNetwork.Infrastructure.Storage;

/// <summary>Private local object store. Storage keys are logical identifiers, never file paths.</summary>
public sealed class LocalFileStorage : IStorageService
{
    private static readonly Regex CategoryPattern = new("^[a-z0-9-]{1,32}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly string _root;
    private readonly StorageOptions _options;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment environment, ILogger<LocalFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        var configuredRoot = _options.RootPath;
        if (string.IsNullOrWhiteSpace(configuredRoot))
            throw new InvalidOperationException("Storage:RootPath must be configured.");
        _root = Path.GetFullPath(Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot));
        Directory.CreateDirectory(_root);
        if (_options.SignedUrlExpirationMinutes is < 1 or > 1440)
            throw new InvalidOperationException("Storage:SignedUrlExpirationMinutes must be between 1 and 1440.");
        if (Encoding.UTF8.GetByteCount(_options.SigningKey) < 32)
            throw new InvalidOperationException("Storage:SigningKey must contain at least 32 UTF-8 bytes.");
    }

    public async Task<StoredFile> UploadAsync(FileUploadRequest file, string category, CancellationToken cancellationToken = default)
    {
        ValidateCategory(category);
        if (file.Length < 0 || file.Content is null || !file.Content.CanRead)
            throw new ArgumentException("A readable file stream and non-negative length are required.", nameof(file));

        var id = Guid.NewGuid().ToString("N");
        var storageKey = $"{category}/{id}";
        var fullPath = ResolvePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        try
        {
            await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 81920, options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            await file.Content.CopyToAsync(output, cancellationToken);
        }
        catch (Exception uploadError)
        {
            try { if (File.Exists(fullPath)) File.Delete(fullPath); }
            catch (Exception cleanupError) { _logger.LogError(cleanupError, "Failed to remove partial upload object {StorageKey}", storageKey); }
            _logger.LogError(uploadError, "Local storage upload failed for object {StorageKey}", storageKey);
            throw;
        }
        return new StoredFile(storageKey);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ResolvePath(storageKey)));
    }

    public Task<StorageObjectMetadata?> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var info = new FileInfo(ResolvePath(storageKey));
        return Task.FromResult(info.Exists ? new StorageObjectMetadata(storageKey, info.Length, info.LastWriteTimeUtc) : null);
    }

    public Task<SignedStorageUrl> CreateSignedUrlAsync(string storageKey, Uri requestBaseUri, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        if (!File.Exists(path)) throw new FileNotFoundException("Stored object was not found.");
        var expiry = DateTimeOffset.UtcNow.AddMinutes(_options.SignedUrlExpirationMinutes).ToUnixTimeSeconds();
        var signature = Sign(storageKey, expiry);
        var parts = storageKey.Split('/');
        var relative = $"/api/media/{Uri.EscapeDataString(parts[0])}/{parts[1]}?expires={expiry}&token={Uri.EscapeDataString(signature)}";
        cancellationToken.ThrowIfCancellationRequested();
        // Local URLs are origin-relative so an untrusted Host header cannot redirect a valid token.
        return Task.FromResult(new SignedStorageUrl(relative, DateTimeOffset.FromUnixTimeSeconds(expiry).UtcDateTime));
    }

    public Task<Stream> OpenSignedReadAsync(string storageKey, long expiresAt, string signature, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ResolvePath(storageKey);
        if (expiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds() || expiresAt > DateTimeOffset.UtcNow.AddMinutes(_options.SignedUrlExpirationMinutes + 1).ToUnixTimeSeconds())
            throw new UnauthorizedAccessException("Signed media URL is expired or invalid.");
        var expected = Encoding.ASCII.GetBytes(Sign(storageKey, expiresAt));
        var supplied = Encoding.ASCII.GetBytes(signature ?? string.Empty);
        if (expected.Length != supplied.Length || !CryptographicOperations.FixedTimeEquals(expected, supplied))
            throw new UnauthorizedAccessException("Signed media URL is expired or invalid.");
        var path = ResolvePath(storageKey);
        if (!File.Exists(path)) throw new FileNotFoundException("Stored object was not found.");
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    private string Sign(string storageKey, long expiresAt)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningKey));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{storageKey}\n{expiresAt}"))).ToLowerInvariant();
    }

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("Storage key is required.", nameof(storageKey));
        var parts = storageKey.Split('/');
        if (parts.Length != 2 || !CategoryPattern.IsMatch(parts[0]) || !Guid.TryParseExact(parts[1], "N", out _))
            throw new ArgumentException("Storage key is invalid.", nameof(storageKey));
        var candidate = Path.GetFullPath(Path.Combine(_root, parts[0], parts[1]));
        var rootPrefix = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Storage key is invalid.", nameof(storageKey));
        return candidate;
    }

    private static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category) || !CategoryPattern.IsMatch(category))
            throw new ArgumentException("Storage category must be a lowercase logical name.", nameof(category));
    }
}
