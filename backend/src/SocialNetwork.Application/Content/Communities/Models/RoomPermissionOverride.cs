namespace SocialNetwork.Application.Content.Communities.Models;

public sealed record RoomPermissionOverride(
    string Code,
    bool IsAllowed);
