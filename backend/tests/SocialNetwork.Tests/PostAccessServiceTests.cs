using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Domain.Moderation.Entities;
using SocialNetwork.Infrastructure.Identity;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SocialNetwork.Tests;

public sealed class PostAccessServiceTests
{
    [Fact]
    public async Task PublicCommunityPostIsVisibleToGuestAndRepositoryResultsMatchAccess()
    {
        await using var db = CreateDb();
        var owner = new User { Username = "owner", Email = "owner@example.test" };
        var author = new User { Username = "author", Email = "author@example.test" };
        var community = new Community { Owner = owner, OwnerId = owner.Id, Name = "Public", Slug = "public", IsPublic = true };
        var post = new Post { Author = author, AuthorId = author.Id, Community = community, CommunityId = community.Id, Visibility = "PUBLIC" };
        db.AddRange(owner, author, community, post);
        await db.SaveChangesAsync();

        await new PostAccessService(db).EnsureCanViewAsync(post.Id, null, default);
        var repository = new PostRepository(db);
        var page = await repository.GetPageAsync(null, 1, 20, null);
        Assert.Contains(page, item => item.Id == post.Id);
        Assert.Equal(1, await repository.CountAsync(null, null));
    }

    [Fact]
    public async Task AcceptedFriendshipGrantsFriendsVisibilityButPendingAndBlockDoNot()
    {
        await using var db = CreateDb();
        var author = new User { Username = "author", Email = "author@example.test" };
        var viewer = new User { Username = "viewer", Email = "viewer@example.test" };
        var post = new Post { Author = author, AuthorId = author.Id, Visibility = "FRIENDS" };
        var friendship = new FriendRequest { Sender = author, SenderId = author.Id, Receiver = viewer, ReceiverId = viewer.Id, Status = "PENDING" };
        db.AddRange(author, viewer, post, friendship);
        await db.SaveChangesAsync();
        var access = new PostAccessService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => access.EnsureCanViewAsync(post.Id, viewer.Id, default));
        friendship.Status = "ACCEPTED";
        await db.SaveChangesAsync();
        await access.EnsureCanViewAsync(post.Id, viewer.Id, default);

        db.UserBlocks.Add(new UserBlock { BlockerId = viewer.Id, BlockedId = author.Id });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => access.EnsureCanViewAsync(post.Id, viewer.Id, default));
    }

    [Fact]
    public async Task QuarantineAndActiveCommunityBanOverridePublicVisibility()
    {
        await using var db = CreateDb();
        var owner = new User { Username = "owner", Email = "owner@example.test" };
        var author = new User { Username = "author", Email = "author@example.test" };
        var viewer = new User { Username = "viewer", Email = "viewer@example.test" };
        var community = new Community { Owner = owner, OwnerId = owner.Id, Name = "Public", Slug = "public", IsPublic = true };
        var post = new Post { Author = author, AuthorId = author.Id, Community = community, CommunityId = community.Id, Visibility = "PUBLIC" };
        db.AddRange(owner, author, viewer, community, post);
        await db.SaveChangesAsync();
        var access = new PostAccessService(db);
        await access.EnsureCanViewAsync(post.Id, viewer.Id, default);

        db.CommunityBans.Add(new CommunityBan { CommunityId = community.Id, UserId = viewer.Id, BannedById = owner.Id, Community = community, User = viewer, BannedBy = owner });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => access.EnsureCanViewAsync(post.Id, viewer.Id, default));

        db.CommunityBans.RemoveRange(db.CommunityBans);
        post.QuarantinedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => access.EnsureCanViewAsync(post.Id, viewer.Id, default));
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options;
        return new AppDbContext(options);
    }
}
