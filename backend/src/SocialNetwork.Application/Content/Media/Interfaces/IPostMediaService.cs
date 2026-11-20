using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Application.Content.Media.Responses;
using SocialNetwork.Application.Content.Posts.Requests;
using SocialNetwork.Application.Content.Posts.Responses;

namespace SocialNetwork.Application.Content.Media.Interfaces;

public interface IPostMediaService
{
    Task<PostResponse> CreatePostWithMediaAsync(Guid authorId, CreatePostRequest request, IReadOnlyList<FileUploadRequest> files, Uri requestBaseUri, CancellationToken cancellationToken);
    Task<PostMediaResponse> AddAsync(
        Guid postId,
        Guid authorId,
        FileUploadRequest file,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PostMediaResponse>> GetForPostAsync(
        Guid postId,
        Guid? viewerId,
        Uri requestBaseUri,
        CancellationToken cancellationToken);

    Task<PostMediaResponse> GetAccessAsync(Guid postId, Guid mediaId, Guid? viewerId, Uri requestBaseUri, CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid postId,
        Guid mediaId,
        Guid authorId,
        CancellationToken cancellationToken);

    Task DeleteForPostAsync(Guid postId, Guid authorId, CancellationToken cancellationToken);
}
