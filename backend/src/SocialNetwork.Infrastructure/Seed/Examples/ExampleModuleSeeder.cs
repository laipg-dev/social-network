using Microsoft.EntityFrameworkCore;
using SocialNetwork.Infrastructure.Persistence.Seed.Core;

namespace SocialNetwork.Infrastructure.Persistence.Seed.Examples;

// FILE MẪU: copy file này và đổi tên theo module, ví dụ CommunitySeeder.
// Không đăng ký ExampleModuleSeeder vào DatabaseSeeder.
public static class ExampleModuleSeeder
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        // 1. Dùng dữ liệu nền có sẵn khi entity cần tham chiếu User.
        var demoUserId = SeedIds.DemoUser1Id;

        // 2. Kiểm tra dữ liệu phụ thuộc thực sự tồn tại.
        var userExists = await dbContext.Users.AnyAsync(
            x => x.Id == demoUserId,
            cancellationToken);

        if (!userExists)
        {
            return;
        }

        // 3. Ví dụ khi làm Community:
        // var exists = await dbContext.Communities.AnyAsync(
        //     x => x.Id == CommunitySeedIds.DemoCommunityId,
        //     cancellationToken);
        //
        // if (!exists)
        // {
        //     dbContext.Communities.Add(new Community
        //     {
        //         Id = CommunitySeedIds.DemoCommunityId,
        //         OwnerId = SeedIds.DemoUser1Id,
        //         ...
        //     });
        // }

        // 4. SaveChanges sau khi Add dữ liệu của module.
        // await dbContext.SaveChangesAsync(cancellationToken);
    }
}
