namespace SocialNetwork.Domain.Communities.Enums;

public static class CommunityStatus
{
    public const string Active = "ACTIVE";
    public const string Closed = "CLOSED";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        StringComparer.Ordinal)
    {
        Active,
        Closed
    };
}
