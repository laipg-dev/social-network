using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Domain.Content.Entities;

public class Post

{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AuthorId { get; set; }
    public Guid? CommunityId { get; set; }

    public string? Content { get; set; }

    public string Visibility { get; set; } = "PUBLIC";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public User Author { get; set; } = null!;
    public Community? Community { get; set; }

    public ICollection<PostMedia> Media { get; set; }
        = new List<PostMedia>();

    public ICollection<Comment> Comments { get; set; }
        = new List<Comment>();

    public ICollection<PostReaction> Reactions { get; set; }
        = new List<PostReaction>();

    public ICollection<Bookmark> Bookmarks { get; set; }
        = new List<Bookmark>();
}