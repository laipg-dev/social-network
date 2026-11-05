using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class UpdateRoomRolePermissionsRequest
{
    [Required(ErrorMessage = "Overrides are required.")]
    public List<RoomPermissionOverrideItem> Overrides { get; set; } = [];
}

public class RoomPermissionOverrideItem
{
    [Required(ErrorMessage = "Code is required.")]
    [StringLength(100, ErrorMessage = "Code must not exceed 100 characters.")]
    public string Code { get; set; } = string.Empty;

    public bool IsAllowed { get; set; }
}
