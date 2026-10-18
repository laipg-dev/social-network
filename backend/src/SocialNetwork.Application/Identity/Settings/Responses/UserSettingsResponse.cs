namespace SocialNetwork.Application.Identity.Settings.Responses;

public sealed record UserSettingsResponse(
    Guid UserId,
    string Theme,
    bool NotificationEnabled,
    string Language,
    string AllowDirectMessagesFrom,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
