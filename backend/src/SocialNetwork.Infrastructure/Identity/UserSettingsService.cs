using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Identity.Settings.Interfaces;
using SocialNetwork.Application.Identity.Settings.Requests;
using SocialNetwork.Application.Identity.Settings.Responses;
using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Identity;

public sealed class UserSettingsService(AppDbContext dbContext) : IUserSettingsService
{
    public async Task<UserSettingsResponse> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var settings = await dbContext.UserSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (settings is null)
        {
            // Nếu chưa có, tự tạo mặc định
            var newSettings = new UserSettings { UserId = userId };
            dbContext.UserSettings.Add(newSettings);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(newSettings);
        }

        return ToResponse(settings);
    }

    public async Task<UserSettingsResponse> UpdateAsync(
        Guid userId, UpdateUserSettingsRequest request, CancellationToken cancellationToken)
    {
        var settings = await dbContext.UserSettings
            .SingleOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (settings is null)
        {
            settings = new UserSettings { UserId = userId };
            dbContext.UserSettings.Add(settings);
        }

        if (!string.IsNullOrWhiteSpace(request.Theme))
            settings.Theme = request.Theme.Trim().ToLowerInvariant();

        if (request.NotificationEnabled.HasValue)
            settings.NotificationEnabled = request.NotificationEnabled.Value;

        if (!string.IsNullOrWhiteSpace(request.Language))
            settings.Language = request.Language.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(request.AllowDirectMessagesFrom))
            settings.AllowDirectMessagesFrom = request.AllowDirectMessagesFrom.Trim().ToUpperInvariant();

        settings.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(settings);
    }

    private static UserSettingsResponse ToResponse(UserSettings settings) =>
        new(
            settings.UserId,
            settings.Theme,
            settings.NotificationEnabled,
            settings.Language,
            settings.AllowDirectMessagesFrom,
            settings.CreatedAt,
            settings.UpdatedAt);
}
