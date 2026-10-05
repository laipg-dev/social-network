using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Moderation.Entities;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReporterId { get; set; }

    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Status { get; set; } = "PENDING";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public User Reporter { get; set; } = null!;

    public ICollection<ModerationAction> Actions { get; set; }
        = new List<ModerationAction>();
}