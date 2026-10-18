using SocialNetwork.Application.Identity.Settings.Requests;
using SocialNetwork.Application.Identity.Settings.Responses;

namespace SocialNetwork.Application.Identity.Settings.Interfaces;

public interface IUserSettingsService
{
    Task<UserSettingsResponse> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserSettingsResponse> UpdateAsync(Guid userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken);
}
