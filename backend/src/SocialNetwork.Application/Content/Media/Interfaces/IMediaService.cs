using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Responses;

namespace SocialNetwork.Application.Content.Media.Interfaces;

public interface IMediaService
{
    Task<MediaResponse> UploadAsync(Guid uploaderId, FileUploadRequest file, CancellationToken cancellationToken);
    Task<MediaResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid uploaderId, CancellationToken cancellationToken);
}
