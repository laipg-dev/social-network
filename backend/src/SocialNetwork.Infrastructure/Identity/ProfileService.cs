using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Identity.Profile.Interfaces;
using SocialNetwork.Application.Identity.Profile.Requests;
using SocialNetwork.Application.Identity.Profile.Responses;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Identity;

public sealed class ProfileService(
    AppDbContext dbContext,
    ICurrentUser currentUser,
    IFileStorage fileStorage) : IProfileService
{
    public async Task<ProfileResponse> GetAsync(CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(cancellationToken);
        return Map(user);
    }

    public async Task<ProfileResponse> UpdateAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(cancellationToken);
        var username = request.Username.Trim();

        if (await dbContext.Users.AnyAsync(
                candidate => candidate.Id != user.Id && candidate.Username == username,
                cancellationToken))
        {
            throw new ConflictException("Username is already registered.");
        }

        user.Username = username;
        user.Profile ??= new Domain.Identity.Entities.UserProfile { UserId = user.Id };
        user.Profile.DisplayName = request.DisplayName?.Trim();
        user.Profile.Bio = request.Bio?.Trim();
        user.Profile.AvatarUrl = request.AvatarUrl?.Trim();
        user.Profile.CoverUrl = request.CoverUrl?.Trim();
        user.Profile.DateOfBirth = request.DateOfBirth;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<ProfileResponse> UploadAvatarAsync(
        FileUploadRequest file,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(cancellationToken);
        var storedFile = await fileStorage.UploadAsync(file, "avatars", cancellationToken);
        user.Profile ??= new Domain.Identity.Entities.UserProfile { UserId = user.Id };
        user.Profile.AvatarUrl = storedFile.Url;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    private async Task<Domain.Identity.Entities.User> GetUserAsync(CancellationToken cancellationToken) =>
        await dbContext.Users
            .Include(user => user.Profile)
            .SingleOrDefaultAsync(user => user.Id == currentUser.UserId, cancellationToken)
        ?? throw new NotFoundException("User profile was not found.");

    private static ProfileResponse Map(Domain.Identity.Entities.User user) =>
        new(user.Id, user.Email, user.Username, user.Profile?.DisplayName, user.Profile?.Bio,
            user.Profile?.AvatarUrl, user.Profile?.CoverUrl, user.Profile?.DateOfBirth,
            user.IsEmailVerified);
}
