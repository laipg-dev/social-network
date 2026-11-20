using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Responses;
using SocialNetwork.Application.Identity.SocialGraph;
using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Identity;

public sealed class SocialGraphService(AppDbContext db) : ISocialGraphService
{
    private IQueryable<User> PublicUsers => db.Users.AsNoTracking().Include(x => x.Profile);
    private static PublicUserResponse Map(User u) => new(u.Id, u.Username, u.Profile?.DisplayName, u.Profile?.Bio, u.Profile?.AvatarUrl, u.CreatedAt);
    private static int Page(int p) => Math.Max(1, p);
    private static int Size(int s) => Math.Clamp(s, 1, 50);
    private async Task EnsureUser(Guid id, CancellationToken ct) { if (!await db.Users.AnyAsync(x => x.Id == id, ct)) throw new NotFoundException("User", id); }
    private async Task EnsureUnblocked(Guid a, Guid b, CancellationToken ct)
    {
        if (await db.UserBlocks.AnyAsync(x => (x.BlockerId == a && x.BlockedId == b) || (x.BlockerId == b && x.BlockedId == a), ct))
            throw new ForbiddenException("This relationship is unavailable because one user has blocked the other.");
    }
    private static async Task<PagedResponse<T>> PageAsync<T>(IQueryable<T> q, int page, int size, CancellationToken ct)
    {
        page = Page(page); size = Size(size); var count = await q.CountAsync(ct);
        return new PagedResponse<T> { Items = await q.Skip((page - 1) * size).Take(size).ToListAsync(ct), Page = page, PageSize = size, TotalCount = count };
    }
    private async Task<PagedResponse<PublicUserResponse>> UsersPage(IQueryable<User> q, int p, int s, CancellationToken ct)
    {
        p = Page(p); s = Size(s); var count = await q.CountAsync(ct);
        return new PagedResponse<PublicUserResponse> { Items = (await q.OrderBy(x => x.Username).Skip((p-1)*s).Take(s).ToListAsync(ct)).Select(Map).ToArray(), Page=p, PageSize=s, TotalCount=count };
    }
    public async Task<PublicUserResponse> GetPublicProfileAsync(Guid id,Guid? viewer,CancellationToken ct)
    {
        if(viewer is Guid v&&v!=id)await EnsureUnblocked(v,id,ct);
        return Map(await PublicUsers.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new NotFoundException("User",id));
    }
    public async Task<PagedResponse<PublicUserResponse>> SearchAsync(string? query,int p,int s,Guid viewer,CancellationToken ct)
    {
        var q=PublicUsers.Where(x=>x.Id!=viewer);
        if(!string.IsNullOrWhiteSpace(query)){var term=query.Trim(); q=q.Where(x=>x.Username.Contains(term)||(x.Profile!=null&&x.Profile.DisplayName!=null&&x.Profile.DisplayName.Contains(term)));}
        q=q.Where(u=>!db.UserBlocks.Any(b=>(b.BlockerId==viewer&&b.BlockedId==u.Id)||(b.BlockerId==u.Id&&b.BlockedId==viewer)));
        return await UsersPage(q.OrderBy(x=>x.Username),p,s,ct);
    }
    public async Task FollowAsync(Guid v,Guid id,CancellationToken ct)
    {
        if(v==id) throw new BusinessRuleException("You cannot follow yourself."); await EnsureUser(id,ct); await EnsureUnblocked(v,id,ct);
        if(await db.Follows.AnyAsync(x=>x.FollowerId==v&&x.FollowingId==id,ct)) throw new ConflictException("You already follow this user.");
        db.Follows.Add(new Follow{FollowerId=v,FollowingId=id}); await db.SaveChangesAsync(ct);
    }
    public async Task UnfollowAsync(Guid v,Guid id,CancellationToken ct){var x=await db.Follows.FindAsync([v,id],ct);if(x!=null){db.Follows.Remove(x);await db.SaveChangesAsync(ct);}}
    public async Task<PagedResponse<PublicUserResponse>> GetFollowersAsync(Guid id,int p,int s,Guid viewer,CancellationToken ct){await EnsureUser(id,ct);await EnsureUnblocked(viewer,id,ct);return await UsersPage(PublicUsers.Where(u=>db.Follows.Any(f=>f.FollowerId==u.Id&&f.FollowingId==id)&&!db.UserBlocks.Any(b=>(b.BlockerId==viewer&&b.BlockedId==u.Id)||(b.BlockerId==u.Id&&b.BlockedId==viewer))),p,s,ct);}
    public async Task<PagedResponse<PublicUserResponse>> GetFollowingAsync(Guid id,int p,int s,Guid viewer,CancellationToken ct){await EnsureUser(id,ct);await EnsureUnblocked(viewer,id,ct);return await UsersPage(PublicUsers.Where(u=>db.Follows.Any(f=>f.FollowerId==id&&f.FollowingId==u.Id)&&!db.UserBlocks.Any(b=>(b.BlockerId==viewer&&b.BlockedId==u.Id)||(b.BlockerId==u.Id&&b.BlockedId==viewer))),p,s,ct);}
    public async Task<FollowStatusResponse> GetFollowStatusAsync(Guid v,Guid id,CancellationToken ct){await EnsureUser(id,ct);await EnsureUnblocked(v,id,ct);return new(await db.Follows.AnyAsync(x=>x.FollowerId==v&&x.FollowingId==id,ct));}
    public async Task<FriendshipStatusResponse> GetFriendshipStatusAsync(Guid v,Guid id,CancellationToken ct)
    {
        await EnsureUser(id,ct);
        if(v==id) return new("FRIENDS");
        await EnsureUnblocked(v,id,ct);
        var request=await db.FriendRequests.AsNoTracking().Where(x=>((x.SenderId==v&&x.ReceiverId==id)||(x.SenderId==id&&x.ReceiverId==v))&&(x.Status=="PENDING"||x.Status=="ACCEPTED")).OrderByDescending(x=>x.Status=="ACCEPTED").ThenByDescending(x=>x.CreatedAt).FirstOrDefaultAsync(ct);
        return new(request?.Status switch { "ACCEPTED"=>"FRIENDS", "PENDING" when request.SenderId==v=>"REQUEST_SENT", "PENDING"=>"REQUEST_RECEIVED", _=>"NONE" });
    }
    private static FriendRequestResponse Map(FriendRequest x)=>new(x.Id,x.SenderId,x.ReceiverId,x.Status,x.CreatedAt);
    public async Task<FriendRequestResponse> SendFriendRequestAsync(Guid v,Guid id,CancellationToken ct)
    {
        if(v==id)throw new BusinessRuleException("You cannot send a friend request to yourself."); await EnsureUser(id,ct);await EnsureUnblocked(v,id,ct);
        if(await db.FriendRequests.AnyAsync(x=>x.Status=="ACCEPTED"&&((x.SenderId==v&&x.ReceiverId==id)||(x.SenderId==id&&x.ReceiverId==v)),ct))throw new ConflictException("You are already friends.");
        var old=await db.FriendRequests.SingleOrDefaultAsync(x=>x.Status=="PENDING"&&((x.SenderId==v&&x.ReceiverId==id)||(x.SenderId==id&&x.ReceiverId==v)),ct);
        if(old!=null){if(old.SenderId==id){old.Status="ACCEPTED";old.RespondedAt=DateTime.UtcNow;await db.SaveChangesAsync(ct);return Map(old);}throw new ConflictException("A friend request is already pending.");}
        var r=new FriendRequest{SenderId=v,ReceiverId=id};db.FriendRequests.Add(r);await db.SaveChangesAsync(ct);return Map(r);
    }
    public async Task<FriendRequestResponse> RespondFriendRequestAsync(Guid v,Guid id,bool accept,CancellationToken ct){var r=await db.FriendRequests.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new NotFoundException("Friend request",id);if(r.ReceiverId!=v)throw new ForbiddenException("Only the recipient can respond to this request.");if(r.Status!="PENDING")throw new ConflictException("Friend request is no longer pending.");await EnsureUnblocked(v,r.SenderId,ct);r.Status=accept?"ACCEPTED":"REJECTED";r.RespondedAt=DateTime.UtcNow;await db.SaveChangesAsync(ct);return Map(r);}
    public async Task CancelFriendRequestAsync(Guid v,Guid id,CancellationToken ct){var r=await db.FriendRequests.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new NotFoundException("Friend request",id);if(r.SenderId!=v)throw new ForbiddenException("Only the sender can cancel this request.");if(r.Status!="PENDING")throw new ConflictException("Friend request is no longer pending.");r.Status="CANCELLED";r.RespondedAt=DateTime.UtcNow;await db.SaveChangesAsync(ct);}
    public async Task RemoveFriendAsync(Guid v,Guid id,CancellationToken ct){await EnsureUser(id,ct);var r=await db.FriendRequests.SingleOrDefaultAsync(x=>x.Status=="ACCEPTED"&&((x.SenderId==v&&x.ReceiverId==id)||(x.SenderId==id&&x.ReceiverId==v)),ct)??throw new NotFoundException("Friendship",id);r.Status="REMOVED";r.RespondedAt=DateTime.UtcNow;await db.SaveChangesAsync(ct);}
    public async Task<PagedResponse<PublicUserResponse>> GetFriendsAsync(Guid id,int p,int s,Guid viewer,CancellationToken ct){await EnsureUser(id,ct);await EnsureUnblocked(viewer,id,ct);var q=PublicUsers.Where(u=>db.FriendRequests.Any(r=>r.Status=="ACCEPTED"&&((r.SenderId==id&&r.ReceiverId==u.Id)||(r.SenderId==u.Id&&r.ReceiverId==id)))&&!db.UserBlocks.Any(b=>(b.BlockerId==viewer&&b.BlockedId==u.Id)||(b.BlockerId==u.Id&&b.BlockedId==viewer)));return await UsersPage(q,p,s,ct);}
    public async Task<PagedResponse<FriendRequestResponse>> GetFriendRequestsAsync(Guid v,bool incoming,int p,int s,CancellationToken ct){var q=db.FriendRequests.AsNoTracking().Where(x=>x.Status=="PENDING"&&(incoming?x.ReceiverId==v:x.SenderId==v)&&!db.UserBlocks.Any(b=>(b.BlockerId==x.SenderId&&b.BlockedId==x.ReceiverId)||(b.BlockerId==x.ReceiverId&&b.BlockedId==x.SenderId)));p=Page(p);s=Size(s);var count=await q.CountAsync(ct);return new(){Items=(await q.OrderByDescending(x=>x.CreatedAt).Skip((p-1)*s).Take(s).ToListAsync(ct)).Select(Map).ToArray(),Page=p,PageSize=s,TotalCount=count};}
    public async Task BlockAsync(Guid v,Guid id,CancellationToken ct){if(v==id)throw new BusinessRuleException("You cannot block yourself.");await EnsureUser(id,ct);if(await db.UserBlocks.AnyAsync(x=>x.BlockerId==v&&x.BlockedId==id,ct))throw new ConflictException("User is already blocked.");db.UserBlocks.Add(new UserBlock{BlockerId=v,BlockedId=id});await db.SaveChangesAsync(ct);}
    public async Task UnblockAsync(Guid v,Guid id,CancellationToken ct){var b=await db.UserBlocks.FindAsync([v,id],ct);if(b==null&&await db.UserBlocks.AnyAsync(x=>x.BlockerId==id&&x.BlockedId==v,ct))throw new ForbiddenException("Only the blocker can unblock this user.");if(b!=null){db.UserBlocks.Remove(b);await db.SaveChangesAsync(ct);}}
    public async Task<PagedResponse<PublicUserResponse>> GetBlockedAsync(Guid v,int p,int s,CancellationToken ct)=>await UsersPage(PublicUsers.Where(u=>db.UserBlocks.Any(b=>b.BlockerId==v&&b.BlockedId==u.Id)),p,s,ct);
}
