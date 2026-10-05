
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Seed.Core;

public static class UserSeeder
{
    private const string DemoPassword = "Demo@123456";

    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var users = new[]
        {
            CreateUser(SeedIds.DemoUser1Id, "demo1@socialnetwork.local", "demo_user_1"),
            CreateUser(SeedIds.DemoUser2Id, "demo2@socialnetwork.local", "demo_user_2"),
            CreateUser(SeedIds.DemoUser3Id, "demo3@socialnetwork.local", "demo_user_3")
        };

        foreach (var user in users)
        {
            var exists = await dbContext.Users.AnyAsync(
                x => x.Id == user.Id || x.Email == user.Email || x.Username == user.Username,
                cancellationToken);

            if (!exists)
            {
                dbContext.Users.Add(user);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static User CreateUser(Guid id, string email, string username)
    {
        var user = new User
        {
            Id = id,
            Email = email,
            Username = username,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = "Demo_Only";
        return user;
    }
}
