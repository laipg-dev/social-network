using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Responses;

namespace SocialNetwork.Application.Content.Media.Interfaces;

public interface IPostMediaService
{
    Task<PostMediaResponse> AddAsync(
        Guid postId,
        Guid authorId,
        FileUploadRequest file,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PostMediaResponse>> GetForPostAsync(
        Guid postId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid mediaId,
        Guid authorId,
        CancellationToken cancellationToken);
}
