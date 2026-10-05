using SocialNetwork.Infrastructure.Persistence.Seed.Core;

namespace SocialNetwork.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        // Seed dữ liệu nền trước vì các module khác có thể phụ thuộc vào User.
        await UserSeeder.SeedAsync(dbContext, cancellationToken);
        
        // Sau này đăng ký module seeder tại đây, theo đúng thứ tự dependency.
        // await CommunitySeeder.SeedAsync(dbContext, cancellationToken);
        // await ContentSeeder.SeedAsync(dbContext, cancellationToken);
        // await MessagingSeeder.SeedAsync(dbContext, cancellationToken);
    }
}
