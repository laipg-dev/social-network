using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Moderation.Entities;

public class ModerationAction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReportId { get; set; }
    public Guid ModeratorId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Report Report { get; set; } = null!;
    public User Moderator { get; set; } = null!;
}