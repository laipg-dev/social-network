using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class CommunityMemberQueryParameters
{
    [StringLength(20, ErrorMessage = "Status must not exceed 20 characters.")]
    public string? Status { get; set; }

    public Guid? RoleId { get; set; }

    [StringLength(100, ErrorMessage = "Search must not exceed 100 characters.")]
    public string? Search { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50.")]
    public int PageSize { get; set; } = 10;
}
