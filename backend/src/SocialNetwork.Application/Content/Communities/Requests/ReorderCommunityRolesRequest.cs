using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class ReorderCommunityRolesRequest
{
    [Required(ErrorMessage = "Items are required.")]
    [MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<RolePositionItem> Items { get; set; } = [];
}

public class RolePositionItem
{
    public Guid RoleId { get; set; }

    [Range(0, 100, ErrorMessage = "Position must be between 0 and 100.")]
    public int Position { get; set; }
}
