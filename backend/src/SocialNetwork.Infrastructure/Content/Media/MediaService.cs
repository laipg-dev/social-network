using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Interfaces;
using SocialNetwork.Application.Content.Media.Responses;
using DomainMedia = SocialNetwork.Domain.Content.Entities.Media;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Content.Media;

public sealed class MediaService(
    AppDbContext dbContext,
    IFileStorage fileStorage) : IMediaService
{
    private const long MaxFileSize = 25 * 1024 * 1024; // 25 MB

    private static readonly HashSet<string> AllowedMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif",
            "video/mp4", "video/webm", "video/quicktime",
            "application/pdf"
        };

    public async Task<MediaResponse> UploadAsync(
        Guid uploaderId, FileUploadRequest file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Tệp tải lên không được rỗng."]
            });

        if (file.Length > MaxFileSize)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Kích thước tệp vượt quá giới hạn cho phép (25 MB)."]
            });

        if (!AllowedMimeTypes.Contains(file.ContentType))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Định dạng tệp không được hỗ trợ."]
            });

        var mediaType = DetermineMediaType(file.ContentType);
        var storedFile = await fileStorage.UploadAsync(file, "media", cancellationToken);

        var media = new DomainMedia
        {
            UploaderId = uploaderId,
            MediaType = mediaType,
            Url = storedFile.Url,
            MimeType = file.ContentType,
            FileSize = file.Length,
            ProcessingStatus = "PENDING"
        };

        dbContext.Media.Add(media);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(media);
    }

    public async Task<MediaResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var media = await dbContext.Media
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (media is null)
            throw new NotFoundException("Media", id);

        return Map(media);
    }

    public async Task DeleteAsync(Guid id, Guid uploaderId, CancellationToken cancellationToken)
    {
        var media = await dbContext.Media
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (media is null)
            throw new NotFoundException("Media", id);

        if (media.UploaderId != uploaderId)
            throw new ForbiddenException("Bạn không có quyền xóa tệp media này.");

        dbContext.Media.Remove(media);
        await dbContext.SaveChangesAsync(cancellationToken);

        var key = media.Url.Replace("/uploads/", string.Empty, StringComparison.OrdinalIgnoreCase);
        await fileStorage.DeleteAsync(key, cancellationToken);
    }

    private static string DetermineMediaType(string mimeType)
    {
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return "IMAGE";
        if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return "VIDEO";
        return "FILE";
    }

    private static MediaResponse Map(DomainMedia media) =>
        new(
            media.Id,
            media.UploaderId,
            media.MediaType,
            media.Url,
            media.MimeType,
            media.FileSize,
            media.ProcessingStatus,
            media.CreatedAt);
}
