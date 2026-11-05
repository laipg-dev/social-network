using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Infrastructure.Persistence.Seed.Core;

namespace SocialNetwork.Infrastructure.Persistence.Seed.Communities;

public static class CommunitySeeder
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        // Community phụ thuộc User nên UserSeeder phải chạy trước.
        var demoUser1Exists = await dbContext.Users.AnyAsync(
            x => x.Id == SeedIds.DemoUser1Id,
            cancellationToken);

        var demoUser2Exists = await dbContext.Users.AnyAsync(
            x => x.Id == SeedIds.DemoUser2Id,
            cancellationToken);

        if (!demoUser1Exists || !demoUser2Exists)
        {
            return;
        }

        if (!await dbContext.Communities.AnyAsync(
                x => x.Id == CommunitySeedIds.DemoPublicCommunityId,
                cancellationToken))
        {
            var publicCommunity = new Community
            {
                Id = CommunitySeedIds.DemoPublicCommunityId,
                OwnerId = SeedIds.DemoUser1Id,
                Name = "Demo Community",
                Slug = "demo-community",
                Description = "Cộng đồng demo công khai để test CRUD.",
                IsPublic = true,
                CreatedAt = DateTime.UtcNow
            };

            publicCommunity.Members.Add(new CommunityMember
            {
                CommunityId = publicCommunity.Id,
                UserId = SeedIds.DemoUser1Id,
                JoinedAt = DateTime.UtcNow
            });

            publicCommunity.Members.Add(new CommunityMember
            {
                CommunityId = publicCommunity.Id,
                UserId = SeedIds.DemoUser2Id,
                JoinedAt = DateTime.UtcNow
            });

            dbContext.Communities.Add(publicCommunity);
        }

        if (!await dbContext.Communities.AnyAsync(
                x => x.Id == CommunitySeedIds.DemoPrivateCommunityId,
                cancellationToken))
        {
            var privateCommunity = new Community
            {
                Id = CommunitySeedIds.DemoPrivateCommunityId,
                OwnerId = SeedIds.DemoUser2Id,
                Name = "Private Demo Community",
                Slug = "private-demo-community",
                Description = "Cộng đồng demo riêng tư để test phân quyền.",
                IsPublic = false,
                CreatedAt = DateTime.UtcNow
            };

            privateCommunity.Members.Add(new CommunityMember
            {
                CommunityId = privateCommunity.Id,
                UserId = SeedIds.DemoUser2Id,
                JoinedAt = DateTime.UtcNow
            });

            dbContext.Communities.Add(privateCommunity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
