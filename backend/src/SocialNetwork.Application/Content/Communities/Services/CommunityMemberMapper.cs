using SocialNetwork.Application.Content.Communities.Models;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Services;

public static class CommunityMemberMapper
{
    public static CommunityMemberResponse ToResponse(CommunityMember member)
    {
        return new CommunityMemberResponse
        {
            UserId = member.UserId,
            Username = member.User?.Username ?? string.Empty,
            DisplayName = member.User?.Profile?.DisplayName,
            AvatarUrl = member.User?.Profile?.AvatarUrl,
            Status = member.Status,
            RequestedAt = member.RequestedAt,
            JoinedAt = member.JoinedAt,
            LeftAt = member.LeftAt,
            Roles = member.Roles
                .OrderByDescending(x => x.Role.Position)
                .Select(x => new CommunityRoleSummary
                {
                    Id = x.Role.Id,
                    Name = x.Role.Name,
                    Position = x.Role.Position
                })
                .ToArray()
        };
    }
}
