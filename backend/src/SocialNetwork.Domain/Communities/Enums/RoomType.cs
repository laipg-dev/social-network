namespace SocialNetwork.Domain.Communities.Enums;

public static class RoomType
{
    public const string Text = "TEXT";
    public const string Voice = "VOICE";
    public const string Announcement = "ANNOUNCEMENT";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        StringComparer.Ordinal)
    {
        Text,
        Voice,
        Announcement
    };
}
