using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Content.Communities.Requests;

public class CommunityQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50.")]
    public int PageSize { get; set; } = 10;

    [StringLength(150, ErrorMessage = "Search must not exceed 150 characters.")]
    public string? Search { get; set; }
}
