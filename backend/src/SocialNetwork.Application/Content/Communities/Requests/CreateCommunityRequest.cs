using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class CreateCommunityRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    [StringLength(500, ErrorMessage = "AvatarUrl must not exceed 500 characters.")]
    public string? AvatarUrl { get; set; }

    [StringLength(500, ErrorMessage = "CoverUrl must not exceed 500 characters.")]
    public string? CoverUrl { get; set; }
}
