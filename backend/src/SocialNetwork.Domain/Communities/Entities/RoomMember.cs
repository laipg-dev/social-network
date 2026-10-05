using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Communities.Entities;

public class RoomMember
{
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Room Room { get; set; } = null!;
    public User User { get; set; } = null!;
}