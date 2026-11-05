namespace SocialNetwork.Domain.Communities.Enums;

public static class CommunityMemberStatus
{
    public const string Pending = "PENDING";
    public const string Active = "ACTIVE";
    public const string Left = "LEFT";
    public const string Rejected = "REJECTED";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        StringComparer.Ordinal)
    {
        Pending,
        Active,
        Left,
        Rejected
    };
}
