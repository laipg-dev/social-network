using SocialNetwork.Application.Identity.Profile.Requests;
using SocialNetwork.Application.Identity.Profile.Responses;
using SocialNetwork.Application.Common.Storage;

namespace SocialNetwork.Application.Identity.Profile.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse> GetAsync(CancellationToken cancellationToken);
    Task<ProfileResponse> UpdateAsync(UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<ProfileResponse> UploadAvatarAsync(FileUploadRequest file, CancellationToken cancellationToken);
    Task<ProfileResponse> UploadCoverAsync(FileUploadRequest file, CancellationToken cancellationToken);
}
