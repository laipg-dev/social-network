using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Infrastructure.Storage;
using Xunit;

namespace SocialNetwork.Tests;

public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "social-network-tests", Guid.NewGuid().ToString("N"));
    private readonly LocalFileStorage _storage;
    private const string SigningKey = "test-signing-key-with-at-least-32-bytes";

    public LocalFileStorageTests()
    {
        _storage = new LocalFileStorage(Options.Create(new StorageOptions
        {
            RootPath = "private-media",
            SigningKey = SigningKey,
            SignedUrlExpirationMinutes = 15
        }), new TestHostEnvironment(_tempRoot), NullLogger<LocalFileStorage>.Instance);
    }

    [Fact]
    public async Task SignedUrlReadsOnlyItsObjectAndUsesRelativeOrigin()
    {
        var first = await UploadAsync("first");
        var other = await UploadAsync("other");
        var signed = await _storage.CreateSignedUrlAsync(first, new Uri("https://untrusted.invalid"));
        Assert.StartsWith("/api/media/", signed.Url);
        Assert.DoesNotContain("untrusted.invalid", signed.Url);

        var (expiry, token) = Parse(signed.Url);
        await using var opened = await _storage.OpenSignedReadAsync(first, expiry, token);
        using var reader = new StreamReader(opened);
        Assert.Equal("first", await reader.ReadToEndAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _storage.OpenSignedReadAsync(other, expiry, token));
    }

    [Fact]
    public async Task InvalidTamperedExpiredAndTraversalTokensAreRejected()
    {
        var key = await UploadAsync("payload");
        var signed = await _storage.CreateSignedUrlAsync(key, new Uri("http://localhost"));
        var (expiry, token) = Parse(signed.Url);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _storage.OpenSignedReadAsync(key, expiry, token + "00"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _storage.OpenSignedReadAsync(key, 1, token));
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.ExistsAsync("../secret"));
    }

    [Fact]
    public async Task MissingObjectCannotReceiveSignedUrl()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _storage.CreateSignedUrlAsync($"posts/{Guid.NewGuid():N}", new Uri("http://localhost")));
    }

    [Fact]
    public async Task DeleteIsSafeToRepeat()
    {
        var key = await UploadAsync("remove-me");
        await _storage.DeleteAsync(key);
        await _storage.DeleteAsync(key);
        Assert.False(await _storage.ExistsAsync(key));
    }

    private async Task<string> UploadAsync(string payload)
    {
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload));
        return (await _storage.UploadAsync(new FileUploadRequest(stream, "file.bin", "application/octet-stream", stream.Length), "posts")).StorageKey;
    }

    private static (long Expiry, string Token) Parse(string relativeUrl)
    {
        var query = new Uri(new Uri("http://localhost"), relativeUrl).Query.TrimStart('?');
        var values = query.Split('&').Select(part => part.Split('=', 2)).ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));
        return (long.Parse(values["expires"]), values["token"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true);
    }

    private sealed class TestHostEnvironment(string contentRoot) : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "SocialNetwork.Tests";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
