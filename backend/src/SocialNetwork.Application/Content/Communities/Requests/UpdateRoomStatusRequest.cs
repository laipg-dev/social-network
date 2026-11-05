using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class UpdateRoomStatusRequest
{
    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status must not exceed 20 characters.")]
    public string Status { get; set; } = string.Empty;
}
