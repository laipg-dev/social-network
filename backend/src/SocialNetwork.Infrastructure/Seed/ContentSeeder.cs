using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Infrastructure.Persistence.Seed.Core;

namespace SocialNetwork.Infrastructure.Persistence.Seed;

public static class ContentSeeder
{
    private static readonly Guid[] DemoPostIds =
    [
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5")
    ];

    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Posts.AnyAsync(post => DemoPostIds.Contains(post.Id), cancellationToken))
            return;

        var posts = new[]
        {
            CreatePost(DemoPostIds[0], SeedIds.DemoUser1Id,
                "Chào mừng mọi người đến với Orbit! Hãy chia sẻ điều bạn đang xây dựng hôm nay."),
            CreatePost(DemoPostIds[1], SeedIds.DemoUser2Id,
                "Mình vừa hoàn thành một tính năng mới. Rất muốn nghe góp ý từ cộng đồng."),
            CreatePost(DemoPostIds[2], SeedIds.DemoUser3Id,
                "Một lời nhắc nhỏ: những cuộc trò chuyện tử tế luôn làm cộng đồng tốt hơn."),
            CreatePost(DemoPostIds[3], SeedIds.DemoUser1Id,
                "Cuối tuần này mọi người đang học hoặc làm dự án gì?"),
            CreatePost(DemoPostIds[4], SeedIds.DemoUser2Id,
                "Đã cập nhật trang Home với feed thật, comments, reactions, bookmarks và media.")
        };

        dbContext.Posts.AddRange(posts);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Post CreatePost(Guid id, Guid authorId, string content) =>
        new()
        {
            Id = id,
            AuthorId = authorId,
            Content = content,
            Visibility = "PUBLIC",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(5, 240))
        };
}
