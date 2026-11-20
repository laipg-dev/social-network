using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Identity;

/// <summary>Central authorization for post content, including community bans and quarantined history.</summary>
public sealed class PostAccessService(AppDbContext db) : IPostAccessService
{
    public async Task EnsureCanViewAsync(Guid postId,Guid? viewerId,CancellationToken ct)
    {
        var post=await db.Posts.AsNoTracking().Where(x=>x.Id==postId&&x.Status=="ACTIVE"&&x.QuarantinedAt==null).Select(x=>new{x.AuthorId,x.Visibility,x.CommunityId}).SingleOrDefaultAsync(ct)
            ??throw new NotFoundException("Post",postId);
        if (post.CommunityId is Guid community)
        {
            if (viewerId is Guid communityViewer && await IsBanned(community, communityViewer, ct))
                throw new NotFoundException("Post", postId);
            var isPublicCommunity = await db.Communities.AnyAsync(x => x.Id == community && x.IsPublic, ct);
            var isMember = viewerId is Guid memberId && await db.CommunityMembers.AnyAsync(x => x.CommunityId == community && x.UserId == memberId && x.Status == "ACTIVE", ct);
            var isOwner = viewerId is Guid ownerId && await db.Communities.AnyAsync(x => x.Id == community && x.OwnerId == ownerId, ct);
            if (!isPublicCommunity && !isMember && !isOwner) throw new NotFoundException("Post", postId);
        }
        if (viewerId is null)
        {
            if (post.Visibility != "PUBLIC") throw new NotFoundException("Post", postId);
            if (post.CommunityId is Guid publicCommunity && !await db.Communities.AnyAsync(x => x.Id == publicCommunity && x.IsPublic, ct))
                throw new NotFoundException("Post", postId);
            return;
        }
        var id=viewerId.Value;
        if (post.CommunityId is Guid restrictedCommunity && await IsBanned(restrictedCommunity, id, ct)) throw new NotFoundException("Post", postId);
        if(post.AuthorId==id)return;
        var blocked=await db.UserBlocks.AnyAsync(x=>(x.BlockerId==id&&x.BlockedId==post.AuthorId)||(x.BlockerId==post.AuthorId&&x.BlockedId==id),ct);
        if(blocked)throw new NotFoundException("Post",postId);
        if(post.Visibility=="FRIENDS")
        {
            if (await db.FriendRequests.AnyAsync(x => x.Status == "ACCEPTED" && ((x.SenderId == id && x.ReceiverId == post.AuthorId) || (x.SenderId == post.AuthorId && x.ReceiverId == id)), ct)) return;
            throw new NotFoundException("Post", postId);
        }
        if(post.Visibility!="PUBLIC")throw new NotFoundException("Post",postId);
    }

    private Task<bool> IsBanned(Guid communityId, Guid userId, CancellationToken ct) => db.CommunityBans.AnyAsync(x => x.CommunityId == communityId && x.UserId == userId && x.RevokedAt == null && (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow), ct);
}
