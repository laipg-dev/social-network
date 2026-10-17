namespace SocialNetwork.Domain.Identity.Entities;

public class UserSettings
{
    public Guid UserId { get; set; }

    public string Theme { get; set; } = "light";
    public bool NotificationEnabled { get; set; } = true;
    public string Language { get; set; } = "vi";
    public string AllowDirectMessagesFrom { get; set; } = "EVERYONE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
