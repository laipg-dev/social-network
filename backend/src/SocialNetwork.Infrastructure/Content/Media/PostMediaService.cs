using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Interfaces;
using SocialNetwork.Application.Content.Media.Responses;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;
using Microsoft.Extensions.Logging;

namespace SocialNetwork.Infrastructure.Content.Media;

public sealed class PostMediaService(
    AppDbContext dbContext,
    IStorageService fileStorage,
    IPostAccessService postAccess,
    IPostService postService,
    ILogger<PostMediaService> logger) : IPostMediaService
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private const int MaxFilesPerPost = 5;

    public async Task<PostResponse> CreatePostWithMediaAsync(Guid authorId, CreatePostRequest request,
        IReadOnlyList<FileUploadRequest> files, Uri requestBaseUri, CancellationToken cancellationToken)
    {
        if (files.Count is < 1 or > MaxFilesPerPost)
            throw new ValidationException(new Dictionary<string, string[]> { ["images"] = [$"A post must contain between 1 and {MaxFilesPerPost} uploaded images."] });
        foreach (var file in files) await ValidateImageAsync(file, cancellationToken);

        var uploadedKeys = new List<string>(files.Count);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var post = await postService.CreateAsync(authorId, request, cancellationToken, hasMedia: true);
            var responses = new List<PostMediaResponse>(files.Count);
            for (var position = 0; position < files.Count; position++)
            {
                var file = files[position];
                var stored = await fileStorage.UploadAsync(file, "posts", cancellationToken);
                uploadedKeys.Add(stored.StorageKey);
                var media = new PostMedia { PostId = post.Id, StorageKey = stored.StorageKey,
                    MediaType = NormalizeContentType(file.ContentType), Position = position };
                dbContext.PostMedia.Add(media);
                await dbContext.SaveChangesAsync(cancellationToken);
                responses.Add(await MapAsync(media, requestBaseUri, cancellationToken));
            }
            post.Media = responses;
            await transaction.CommitAsync(cancellationToken);
            return post;
        }
        catch (Exception operationError)
        {
            try { await transaction.RollbackAsync(CancellationToken.None); }
            catch (Exception rollbackError) { logger.LogError(rollbackError, "Rollback failed for multipart post creation {AuthorId}", authorId); }
            foreach (var key in uploadedKeys)
            {
                try { await fileStorage.DeleteAsync(key, CancellationToken.None); }
                catch (Exception cleanupError) { logger.LogError(cleanupError, "Storage compensation failed for {StorageKey} after post creation failure", key); }
            }
            logger.LogError(operationError, "Multipart post creation failed for author {AuthorId}; uploaded objects were compensated where possible", authorId);
            throw;
        }
    }

    public async Task<PostMediaResponse> AddAsync(
        Guid postId,
        Guid authorId,
        FileUploadRequest file,
        CancellationToken cancellationToken)
    {
        await ValidateImageAsync(file, cancellationToken);

        var post = await dbContext.Posts
            .SingleOrDefaultAsync(item => item.Id == postId && item.Status == "ACTIVE", cancellationToken)
            ?? throw new NotFoundException("Post", postId);
        EnsureAuthor(post, authorId);

        var position = await dbContext.PostMedia
            .Where(media => media.PostId == postId)
            .Select(media => (int?)media.Position)
            .MaxAsync(cancellationToken) ?? -1;
        if (position + 1 >= MaxFilesPerPost)
            throw new ValidationException(new Dictionary<string, string[]> { ["images"] = [$"A post can contain at most {MaxFilesPerPost} images."] });
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        StoredFile? storedFile = null;
        var media = new PostMedia
        {
            PostId = postId,
            MediaType = NormalizeContentType(file.ContentType),
            Position = position + 1
        };
        try
        {
            storedFile = await fileStorage.UploadAsync(file, "posts", cancellationToken);
            media.StorageKey = storedFile.StorageKey;
            dbContext.PostMedia.Add(media);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception operationError)
        {
            try { await transaction.RollbackAsync(CancellationToken.None); }
            catch (Exception rollbackError) { logger.LogError(rollbackError, "Rollback failed while adding media to post {PostId}", postId); }
            dbContext.Entry(media).State = EntityState.Detached;
            if (storedFile is not null)
            {
                try { await fileStorage.DeleteAsync(storedFile.StorageKey, CancellationToken.None); }
                catch (Exception cleanupError) { logger.LogError(cleanupError, "Storage compensation failed for {StorageKey} after media persistence failure", storedFile.StorageKey); }
            }
            logger.LogError(operationError, "Failed to persist media for post {PostId}", postId);
            throw;
        }
        return Map(media);
    }

    public async Task<IReadOnlyList<PostMediaResponse>> GetForPostAsync(
        Guid postId,
        Guid? viewerId,
        Uri requestBaseUri,
        CancellationToken cancellationToken)
    {
        await postAccess.EnsureCanViewAsync(postId,viewerId,cancellationToken);

        var responses = new List<PostMediaResponse>();
        foreach (var media in await dbContext.PostMedia.AsNoTracking().Where(media => media.PostId == postId)
                     .OrderBy(media => media.Position).ThenBy(media => media.CreatedAt).ToListAsync(cancellationToken))
            responses.Add(await MapAsync(media, requestBaseUri, cancellationToken));
        return responses;
    }

    public async Task<PostMediaResponse> GetAccessAsync(Guid postId, Guid mediaId, Guid? viewerId, Uri requestBaseUri, CancellationToken cancellationToken)
    {
        var media = await dbContext.PostMedia.AsNoTracking().SingleOrDefaultAsync(x => x.Id == mediaId && x.PostId == postId, cancellationToken)
            ?? throw new NotFoundException("Post media", mediaId);
        await postAccess.EnsureCanViewAsync(media.PostId, viewerId, cancellationToken);
        return await MapAsync(media, requestBaseUri, cancellationToken);
    }

    private async Task<PostMediaResponse> MapAsync(PostMedia media, Uri requestBaseUri, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(media.StorageKey))
            throw new NotFoundException("Post media", media.Id);
        var signed = await fileStorage.CreateSignedUrlAsync(media.StorageKey, requestBaseUri, cancellationToken);
        return new PostMediaResponse(media.Id, media.PostId, signed.Url, signed.ExpiresAtUtc,
                media.MediaType, media.Position, media.CreatedAt);
    }

    public async Task DeleteAsync(
        Guid postId,
        Guid mediaId,
        Guid authorId,
        CancellationToken cancellationToken)
    {
        var media = await dbContext.PostMedia
            .Include(item => item.Post)
            .SingleOrDefaultAsync(item => item.Id == mediaId && item.PostId == postId, cancellationToken)
            ?? throw new NotFoundException("Post media", mediaId);
        EnsureAuthor(media.Post, authorId);

        if (!string.IsNullOrWhiteSpace(media.StorageKey))
        {
            try { await fileStorage.DeleteAsync(media.StorageKey, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Storage deletion failed for media {MediaId} ({StorageKey})", mediaId, media.StorageKey); throw; }
        }
        dbContext.PostMedia.Remove(media);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteForPostAsync(Guid postId, Guid authorId, CancellationToken cancellationToken)
    {
        var post = await dbContext.Posts.Include(x => x.Media).SingleOrDefaultAsync(x => x.Id == postId, cancellationToken)
            ?? throw new NotFoundException("Post", postId);
        EnsureAuthor(post, authorId);
        if (post.Status != "DELETED") throw new BusinessRuleException("Media cleanup is available only after the post has been deleted.");
        foreach (var media in post.Media)
        {
            if (string.IsNullOrWhiteSpace(media.StorageKey)) continue;
            try { await fileStorage.DeleteAsync(media.StorageKey, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Storage cleanup failed for deleted post {PostId}, media {MediaId} ({StorageKey})", postId, media.Id, media.StorageKey); throw; }
        }
        dbContext.PostMedia.RemoveRange(post.Media);
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (Exception ex) { logger.LogError(ex, "Media metadata cleanup failed for deleted post {PostId}; retry deletion to reconcile", postId); throw; }
    }

    private static async Task ValidateImageAsync(FileUploadRequest file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0) throw UploadValidation("Media file cannot be empty.");
        if (file.Length > MaxFileSize) throw UploadValidation("Media file must be 10 MB or smaller.");
        if (file.Content is null || !file.Content.CanRead || !file.Content.CanSeek)
            throw UploadValidation("Media must be a readable, seekable image stream.");
        var extension = Path.GetExtension(Path.GetFileName(file.FileName));
        var contentType = file.ContentType.Trim().ToLowerInvariant();
        var extType = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => null
        };
        if (extType is null || extType != contentType)
            throw UploadValidation("Supported matching image types are JPG/JPEG, PNG, WEBP, and GIF.");

        var originalPosition = file.Content.Position;
        var header = new byte[12];
        var read = 0;
        try
        {
            file.Content.Position = 0;
            while (read < header.Length)
            {
                var count = await file.Content.ReadAsync(header.AsMemory(read, header.Length - read), cancellationToken);
                if (count == 0) break;
                read += count;
            }
        }
        finally { file.Content.Position = originalPosition; }
        var signatureMatches = contentType switch
        {
            "image/jpeg" => read >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            "image/png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "image/gif" => read >= 6 && (System.Text.Encoding.ASCII.GetString(header, 0, 6) is "GIF87a" or "GIF89a"),
            "image/webp" => read >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
            _ => false
        };
        if (!signatureMatches) throw UploadValidation("The file contents do not match the declared image type.");
    }

    private static ValidationException UploadValidation(string message) =>
        new(new Dictionary<string, string[]> { ["file"] = [message] });

    private static string NormalizeContentType(string contentType) => contentType.Trim().ToLowerInvariant();

    private static void EnsureAuthor(Post post, Guid authorId)
    {
        if (post.AuthorId != authorId)
            throw new ForbiddenException("Only the post author can manage post media.");
    }

    private static PostMediaResponse Map(PostMedia media) =>
        new(media.Id, media.PostId, null, null, media.MediaType, media.Position, media.CreatedAt);
}
