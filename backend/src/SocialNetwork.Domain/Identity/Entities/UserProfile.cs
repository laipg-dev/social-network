namespace SocialNetwork.Domain.Identity.Entities;

public class UserProfile
{
    public Guid UserId { get; set; }

    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }

    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}