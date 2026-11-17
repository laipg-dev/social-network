using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities.Authorization;

namespace SocialNetwork.Application.Content.Communities.Authorization;

public static class RolePositionRules
{
    public static void EnsureCanManagePosition(
        CommunityAccess access,
        int position)
    {
        if (access.IsOwner)
        {
            return;
        }

        var actorPosition = access.Roles.Count == 0
            ? 0
            : access.Roles.Max(x => x.Position);

        if (position >= actorPosition)
        {
            throw new ForbiddenException(
                "You cannot manage a role at or above your own position.");
        }
    }
}
