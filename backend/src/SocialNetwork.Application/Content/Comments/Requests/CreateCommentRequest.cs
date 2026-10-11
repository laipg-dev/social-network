using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Comments.Requests;

public sealed class CreateCommentRequest
{
    [Required, MaxLength(5000)]
    public required string Content { get; set; }

    public Guid? ParentCommentId { get; set; }
}
