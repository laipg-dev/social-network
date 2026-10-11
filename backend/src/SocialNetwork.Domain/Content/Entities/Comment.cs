using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Content.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Post Post { get; set; } = null!;
    public User Author { get; set; } = null!;

    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; }
        = new List<Comment>();

    public ICollection<CommentReaction> Reactions { get; set; }
        = new List<CommentReaction>();
}