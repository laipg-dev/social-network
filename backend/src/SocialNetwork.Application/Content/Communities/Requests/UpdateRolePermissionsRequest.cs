using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class UpdateRolePermissionsRequest
{
    [Required(ErrorMessage = "Codes are required.")]
    public List<string> Codes { get; set; } = [];
}
