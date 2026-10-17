using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Identity.Sessions.Interfaces;
using SocialNetwork.Application.Identity.Sessions.Responses;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.Infrastructure.Identity;

public sealed class SessionService(AppDbContext dbContext) : ISessionService
{
    public async Task<IReadOnlyList<SessionResponse>> GetActiveSessionsAsync(
        Guid userId, string? currentRefreshToken, CancellationToken cancellationToken)
    {
        var currentTokenHash = string.IsNullOrWhiteSpace(currentRefreshToken)
            ? null
            : HashToken(currentRefreshToken);

        var sessions = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return sessions.Select(s => new SessionResponse(
            s.Id,
            s.IpAddress,
            s.UserAgent,
            s.CreatedAt,
            s.ExpiresAt,
            s.RevokedAt,
            IsActive: s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow,
            IsCurrent: currentTokenHash != null && s.Token == currentTokenHash
        )).ToList();
    }

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(t => t.Id == sessionId && t.UserId == userId, cancellationToken);

        if (session is null)
            throw new NotFoundException("Phiên đăng nhập không tồn tại.");

        if (session.RevokedAt is null)
        {
            session.RevokedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllOtherSessionsAsync(
        Guid userId, string? currentRefreshToken, CancellationToken cancellationToken)
    {
        var currentTokenHash = string.IsNullOrWhiteSpace(currentRefreshToken)
            ? null
            : HashToken(currentRefreshToken);

        var query = dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null);

        if (currentTokenHash is not null)
        {
            query = query.Where(t => t.Token != currentTokenHash);
        }

        await query.ExecuteUpdateAsync(
            update => update.SetProperty(t => t.RevokedAt, DateTime.UtcNow),
            cancellationToken);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
