using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class UpdateCommunityRoleRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Position must be between 0 and 100.")]
    public int Position { get; set; }
}
