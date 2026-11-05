using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class UpdateMemberRolesRequest
{
    [Required(ErrorMessage = "RoleIds are required.")]
    public List<Guid> RoleIds { get; set; } = [];
}
