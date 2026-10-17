
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Infrastructure.Authentication;

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
            CreateUser(SeedIds.DemoUser1Id, "demo1@socialnetwork.local", "demo_user_1", "Demo Builder"),
            CreateUser(SeedIds.DemoUser2Id, "demo2@socialnetwork.local", "demo_user_2", "Demo Creator"),
            CreateUser(SeedIds.DemoUser3Id, "demo3@socialnetwork.local", "demo_user_3", "Demo Community")
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

        var profileNames = new Dictionary<Guid, string>
        {
            [SeedIds.DemoUser1Id] = "Demo Builder",
            [SeedIds.DemoUser2Id] = "Demo Creator",
            [SeedIds.DemoUser3Id] = "Demo Community"
        };
        foreach (var profile in profileNames)
        {
            if (!await dbContext.UserProfiles.AnyAsync(
                    item => item.UserId == profile.Key,
                    cancellationToken))
                dbContext.UserProfiles.Add(new UserProfile
                {
                    UserId = profile.Key,
                    DisplayName = profile.Value
                });
        }
        var seededUsers = await dbContext.Users
            .Where(user => profileNames.Keys.Contains(user.Id))
            .ToListAsync(cancellationToken);

        foreach (var user in seededUsers)
        {
            if (!await dbContext.UserSettings.AnyAsync(s => s.UserId == user.Id, cancellationToken))
            {
                dbContext.UserSettings.Add(new UserSettings
                {
                    UserId = user.Id,
                    Theme = "light",
                    NotificationEnabled = true,
                    Language = "vi"
                });
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var seededUser in seededUsers.Where(user => user.PasswordHash == "Demo_Only"))
            seededUser.PasswordHash = PasswordService.Hash(DemoPassword);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static User CreateUser(Guid id, string email, string username, string displayName)
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

        user.Profile = new UserProfile { UserId = id, DisplayName = displayName };
        user.PasswordHash = "Demo_Only";
        return user;
    }
}
