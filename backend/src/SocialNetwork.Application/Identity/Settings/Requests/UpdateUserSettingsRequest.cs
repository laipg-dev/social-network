namespace SocialNetwork.Application.Identity.Settings.Requests;

public sealed record UpdateUserSettingsRequest(
    string? Theme,
    bool? NotificationEnabled,
    string? Language,
    string? AllowDirectMessagesFrom);
