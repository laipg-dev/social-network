using SocialNetwork.Infrastructure.Persistence.Seed.Communities;
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

        // Permission là danh mục dùng chung, phải có trước khi tạo role.
        await PermissionSeeder.SeedAsync(dbContext, cancellationToken);

        // Sau này đăng ký module seeder tại đây, theo đúng thứ tự dependency.
        await CommunitySeeder.SeedAsync(dbContext, cancellationToken);

        // Bổ sung role mặc định cho community tạo trước khi có RBAC.
        await CommunityRoleBackfill.EnsureAsync(dbContext, cancellationToken);
        // await ContentSeeder.SeedAsync(dbContext, cancellationToken);
        // await MessagingSeeder.SeedAsync(dbContext, cancellationToken);
    }
}
