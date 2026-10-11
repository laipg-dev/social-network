using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Interfaces;
using SocialNetwork.Application.Content.Media.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Content.Media;

public sealed class PostMediaService(
    AppDbContext dbContext,
    IFileStorage fileStorage) : IPostMediaService
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif",
            "video/mp4", "video/webm"
        };

    public async Task<PostMediaResponse> AddAsync(
        Guid postId,
        Guid authorId,
        FileUploadRequest file,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Media file cannot be empty."]
            });
        if (file.Length > MaxFileSize)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Media file must be 10 MB or smaller."]
            });
        if (!AllowedTypes.Contains(file.ContentType))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Supported media types are JPG, PNG, WEBP, GIF, MP4 and WEBM."]
            });

        var post = await dbContext.Posts
            .SingleOrDefaultAsync(item => item.Id == postId && item.Status == "ACTIVE", cancellationToken)
            ?? throw new NotFoundException("Post", postId);
        EnsureAuthor(post, authorId);

        var position = await dbContext.PostMedia
            .Where(media => media.PostId == postId)
            .Select(media => (int?)media.Position)
            .MaxAsync(cancellationToken) ?? -1;
        var storedFile = await fileStorage.UploadAsync(file, "posts", cancellationToken);
        var media = new PostMedia
        {
            PostId = postId,
            Url = storedFile.Url,
            MediaType = file.ContentType,
            Position = position + 1
        };

        dbContext.PostMedia.Add(media);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(media);
    }

    public async Task<IReadOnlyList<PostMediaResponse>> GetForPostAsync(
        Guid postId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Posts.AnyAsync(
                post => post.Id == postId && post.Status == "ACTIVE",
                cancellationToken))
            throw new NotFoundException("Post", postId);

        return await dbContext.PostMedia
            .AsNoTracking()
            .Where(media => media.PostId == postId)
            .OrderBy(media => media.Position)
            .ThenBy(media => media.CreatedAt)
            .Select(media => new PostMediaResponse(
                media.Id, media.PostId, media.Url, media.MediaType,
                media.Position, media.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid mediaId,
        Guid authorId,
        CancellationToken cancellationToken)
    {
        var media = await dbContext.PostMedia
            .Include(item => item.Post)
            .SingleOrDefaultAsync(item => item.Id == mediaId, cancellationToken)
            ?? throw new NotFoundException("Post media", mediaId);
        EnsureAuthor(media.Post, authorId);

        dbContext.PostMedia.Remove(media);
        await dbContext.SaveChangesAsync(cancellationToken);

        var key = media.Url
            .Replace("/uploads/", string.Empty, StringComparison.OrdinalIgnoreCase);
        await fileStorage.DeleteAsync(key, cancellationToken);
    }

    private static void EnsureAuthor(Post post, Guid authorId)
    {
        if (post.AuthorId != authorId)
            throw new ForbiddenException("Only the post author can manage post media.");
    }

    private static PostMediaResponse Map(PostMedia media) =>
        new(media.Id, media.PostId, media.Url, media.MediaType, media.Position, media.CreatedAt);
}
