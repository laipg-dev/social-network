using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Notification.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Người nhận notification
    public Guid UserId { get; set; }

    // Người gây ra hành động, có thể null
    // Ví dụ system notification thì không có Actor
    public Guid? ActorId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Title { get; set; }
    public string? Content { get; set; }

    // ID của đối tượng liên quan:
    // Post, Comment, Community...
    public Guid? ReferenceId { get; set; }

    // Loại đối tượng mà ReferenceId đang trỏ tới
    public string? ReferenceType { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
    public User? Actor { get; set; }
}