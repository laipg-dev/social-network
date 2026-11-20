using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Infrastructure.Content.Media;
using SocialNetwork.Infrastructure.Identity;
using SocialNetwork.Infrastructure.Persistence;
using Xunit;

namespace SocialNetwork.Tests;

public sealed class MediaValidationTests
{
    [Theory]
    [InlineData("empty", "empty.png", "image/png", 0)]
    [InlineData("oversize", "large.png", "image/png", 10485761)]
    [InlineData("mismatch", "wrong.jpg", "image/png", 12)]
    [InlineData("signature", "bad.png", "image/png", 12)]
    [InlineData("unsupported", "movie.mp4", "video/mp4", 12)]
    public async Task InvalidImagesAreRejectedBeforePostOrStorageWork(string scenario, string filename, string contentType, long length)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options;
        await using var db = new AppDbContext(options);
        var service = new PostMediaService(db, null!, new PostAccessService(db), null!, NullLogger<PostMediaService>.Instance);
        var payload = scenario == "signature" ? new byte[12] : System.Text.Encoding.ASCII.GetBytes("not-an-image");
        await using var stream = new MemoryStream(payload);
        var file = new FileUploadRequest(stream, filename, contentType, length);

        await Assert.ThrowsAsync<ValidationException>(() => service.CreatePostWithMediaAsync(Guid.NewGuid(),
            new CreatePostRequest { Content = "test", Visibility = "PUBLIC" }, [file], new Uri("http://localhost"), default));
        Assert.Empty(db.Posts);
        Assert.Empty(db.PostMedia);
    }
}
