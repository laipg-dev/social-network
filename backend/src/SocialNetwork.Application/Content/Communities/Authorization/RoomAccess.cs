namespace SocialNetwork.Application.Content.Communities.Authorization;

public sealed class RoomAccess
{
    public bool CanView { get; init; }

    public bool IsRoomMember { get; init; }

    public IReadOnlyList<string> Permissions { get; init; }
        = Array.Empty<string>();
}
