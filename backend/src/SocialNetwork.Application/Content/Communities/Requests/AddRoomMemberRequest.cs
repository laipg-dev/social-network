using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class AddRoomMemberRequest
{
    public Guid UserId { get; set; }
}
