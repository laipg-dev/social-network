using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class ReorderRoomsRequest
{
    [Required(ErrorMessage = "Items are required.")]
    [MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<RoomPositionItem> Items { get; set; } = [];
}

public class RoomPositionItem
{
    public Guid RoomId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Position must be greater than or equal to 0.")]
    public int Position { get; set; }
}
