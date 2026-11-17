using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class UpdateRoomRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
    public string? Description { get; set; }

    [StringLength(20, ErrorMessage = "Type must not exceed 20 characters.")]
    public string Type { get; set; } = "TEXT";

    public bool IsPublic { get; set; } = true;

    [Range(0, int.MaxValue, ErrorMessage = "Position must be greater than or equal to 0.")]
    public int Position { get; set; }
}
