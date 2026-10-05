namespace SocialNetwork.Domain.Identity.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public UserProfile? Profile { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();

    public ICollection<Follow> Followers { get; set; }
    = new List<Follow>();

    public ICollection<Follow> Following { get; set; }
        = new List<Follow>();

    public ICollection<FriendRequest> SentFriendRequests { get; set; }
        = new List<FriendRequest>();

    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; }
        = new List<FriendRequest>();

    public ICollection<UserBlock> BlockedUsers { get; set; }
        = new List<UserBlock>();

    public ICollection<UserBlock> BlockedByUsers { get; set; }
        = new List<UserBlock>();
}