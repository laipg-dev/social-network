namespace SocialNetwork.Application.Content.Communities.Models;

public sealed class CommunityRoleSummary
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Position { get; init; }
}
