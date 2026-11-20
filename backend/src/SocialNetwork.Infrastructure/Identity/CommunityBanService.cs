using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Enums;
using SocialNetwork.Domain.Moderation.Entities;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Application.Common.Storage;

namespace SocialNetwork.Infrastructure.Identity;

public sealed class CommunityBanService(AppDbContext db, ICommunityAuthorization auth, IStorageService storage) : ICommunityBanService
{
    private static CommunityBanResponse Map(CommunityBan b)=>new(b.Id,b.CommunityId,b.UserId,b.User.Username,b.User.Profile?.DisplayName,b.BannedById,b.Reason,b.CreatedAt,b.ExpiresAt);
    public async Task<CommunityBanResponse> BanAsync(Guid actor,Guid community,Guid user,BanCommunityMemberRequest request,CancellationToken ct)
    {
        await auth.RequireCommunityPermissionAsync(community,actor,CommunityPermissions.CommunityMemberManage,ct);
        var owner=await db.Communities.Where(x=>x.Id==community).Select(x=>x.OwnerId).SingleOrDefaultAsync(ct);
        if(owner==Guid.Empty)throw new NotFoundException("Community",community);
        if(owner==user)throw new BusinessRuleException("The community owner cannot be banned.");
        if(request.ExpiresAt is not null && request.ExpiresAt<=DateTime.UtcNow)throw new ValidationException(new Dictionary<string,string[]>{{"expiresAt",["Expiration must be in the future."]}});
        if(!await db.Users.AnyAsync(x=>x.Id==user,ct))throw new NotFoundException("User",user);
        if(!await db.CommunityMembers.AnyAsync(x=>x.CommunityId==community&&x.UserId==user&&x.Status==CommunityMemberStatus.Active,ct))throw new NotFoundException("Active community membership",user);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var now=DateTime.UtcNow;
        if(await db.CommunityBans.AnyAsync(x=>x.CommunityId==community&&x.UserId==user&&x.RevokedAt==null&&(x.ExpiresAt==null||x.ExpiresAt>now),ct))throw new ConflictException("This member is already banned.");
        var ban=await db.CommunityBans.Include(x=>x.User).ThenInclude(x=>x.Profile).Where(x=>x.CommunityId==community&&x.UserId==user).OrderByDescending(x=>x.CreatedAt).FirstOrDefaultAsync(ct);
        if(ban==null){ban=new CommunityBan{CommunityId=community,UserId=user};db.CommunityBans.Add(ban);}
        ban.BannedById=actor;ban.Reason=request.Reason?.Trim();ban.CreatedAt=now;ban.ExpiresAt=request.ExpiresAt;ban.RevokedAt=null;
        var membership = await db.CommunityMembers.Include(x => x.Roles)
            .SingleAsync(x => x.CommunityId == community && x.UserId == user, ct);
        membership.Status = CommunityMemberStatus.Left;
        membership.LeftAt = now;
        membership.UpdatedAt = now;
        db.CommunityMemberRoles.RemoveRange(membership.Roles);
        // Quarantine the user's existing authored content in this community. These markers
        // survive unban/rejoin until a moderator explicitly restores each item.
        await db.Posts.Where(x => x.CommunityId == community && x.AuthorId == user && x.QuarantinedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(x => x.QuarantinedAt, now), ct);
        await db.Comments.Where(x => x.Post.CommunityId == community && x.AuthorId == user && x.QuarantinedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(x => x.QuarantinedAt, now), ct);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        await db.Entry(ban).Reference(x=>x.User).LoadAsync(ct);await db.Entry(ban.User).Reference(x=>x.Profile).LoadAsync(ct);return Map(ban);
    }
    public async Task UnbanAsync(Guid actor,Guid community,Guid user,CancellationToken ct)
    {
        await auth.RequireCommunityPermissionAsync(community,actor,CommunityPermissions.CommunityMemberManage,ct);var now=DateTime.UtcNow;
        var b=await db.CommunityBans.Where(x=>x.CommunityId==community&&x.UserId==user&&x.RevokedAt==null&&(x.ExpiresAt==null||x.ExpiresAt>now)).OrderByDescending(x=>x.CreatedAt).FirstOrDefaultAsync(ct)??throw new NotFoundException("Active community ban",user);
        b.RevokedAt=now;await db.SaveChangesAsync(ct);
    }
    public async Task<PagedResponse<CommunityBanResponse>> GetBannedAsync(Guid actor,Guid community,int page,int pageSize,CancellationToken ct)
    {
        await auth.RequireCommunityPermissionAsync(community,actor,CommunityPermissions.CommunityMemberManage,ct);page=Math.Max(1,page);pageSize=Math.Clamp(pageSize,1,50);var now=DateTime.UtcNow;
        var q=db.CommunityBans.AsNoTracking().Include(x=>x.User).ThenInclude(x=>x.Profile).Where(x=>x.CommunityId==community&&x.RevokedAt==null&&(x.ExpiresAt==null||x.ExpiresAt>now));var count=await q.CountAsync(ct);
        return new(){Items=(await q.OrderByDescending(x=>x.CreatedAt).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct)).Select(Map).ToArray(),Page=page,PageSize=pageSize,TotalCount=count};
    }

    public async Task<PagedResponse<QuarantinedPostResponse>> GetQuarantinedPostsAsync(Guid actor, Guid community, int page, int pageSize, Uri requestBaseUri, CancellationToken ct)
    {
        await auth.RequireCommunityPermissionAsync(community, actor, CommunityPermissions.CommunityMemberManage, ct);
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 50);
        var query = db.Posts.AsNoTracking().Where(x => x.CommunityId == community && x.QuarantinedAt != null && x.Status == "ACTIVE");
        var total = await query.CountAsync(ct);
        var posts = await query.Include(x => x.Author).Include(x => x.Media)
            .OrderByDescending(x => x.QuarantinedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        var items = new List<QuarantinedPostResponse>();
        foreach (var post in posts)
        {
            var media = new List<QuarantinedMediaResponse>();
            foreach (var item in post.Media.OrderBy(m => m.Position))
            {
                SignedStorageUrl? signed = string.IsNullOrWhiteSpace(item.StorageKey) ? null : await storage.CreateSignedUrlAsync(item.StorageKey, requestBaseUri, ct);
                media.Add(new(item.Id, signed?.Url, signed?.ExpiresAtUtc, item.MediaType, item.Position));
            }
            items.Add(new(post.Id, post.AuthorId, post.Author.Username, post.Content, post.CreatedAt, media));
        }
        return new() { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task RestorePostAsync(Guid actor, Guid community, Guid postId, CancellationToken ct)
    {
        await auth.RequireCommunityPermissionAsync(community, actor, CommunityPermissions.CommunityMemberManage, ct);
        var post = await db.Posts.SingleOrDefaultAsync(x => x.Id == postId && x.CommunityId == community && x.QuarantinedAt != null && x.Status == "ACTIVE", ct)
            ?? throw new NotFoundException("Quarantined post", postId);
        var now = DateTime.UtcNow;
        if (await db.CommunityBans.AnyAsync(x => x.CommunityId == community && x.UserId == post.AuthorId && x.RevokedAt == null && (x.ExpiresAt == null || x.ExpiresAt > now), ct))
            throw new BusinessRuleException("A post cannot be restored while its author is banned from this community.");
        post.QuarantinedAt = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<QuarantinedCommentResponse>> GetQuarantinedCommentsAsync(Guid actor, Guid community, int page, int pageSize, CancellationToken ct)
    {
        await auth.RequireCommunityPermissionAsync(community, actor, CommunityPermissions.CommunityMemberManage, ct);
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Comments.AsNoTracking().Where(x => x.Post.CommunityId == community && x.Status == "ACTIVE" && x.QuarantinedAt != null);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.QuarantinedAt).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new QuarantinedCommentResponse(x.Id, x.PostId, x.ParentCommentId, x.AuthorId, x.Content, x.CreatedAt, x.QuarantinedAt!.Value))
            .ToListAsync(ct);
        return new() { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task RestoreCommentAsync(Guid actor, Guid community, Guid commentId, CancellationToken ct)
    {
        await auth.RequireCommunityPermissionAsync(community, actor, CommunityPermissions.CommunityMemberManage, ct);
        var comment = await db.Comments.Include(x => x.Post)
            .SingleOrDefaultAsync(x => x.Id == commentId && x.Post.CommunityId == community && x.Status == "ACTIVE" && x.QuarantinedAt != null, ct)
            ?? throw new NotFoundException("Quarantined comment", commentId);
        if (comment.Post.Status != "ACTIVE" || comment.Post.QuarantinedAt != null)
            throw new BusinessRuleException("A comment cannot be restored while its post is deleted or quarantined.");
        var now = DateTime.UtcNow;
        if (await db.CommunityBans.AnyAsync(x => x.CommunityId == community && x.UserId == comment.AuthorId && x.RevokedAt == null && (x.ExpiresAt == null || x.ExpiresAt > now), ct))
            throw new BusinessRuleException("A comment cannot be restored while its author is banned from this community.");
        comment.QuarantinedAt = null;
        await db.SaveChangesAsync(ct);
    }
}
