using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Posts.Requests;

public sealed class UpdatePostRequest
{
    [MaxLength(10000)]
    public string? Content { get; set; }

    [Required, MaxLength(20)]
    public string Visibility { get; set; } = "PUBLIC";
}
