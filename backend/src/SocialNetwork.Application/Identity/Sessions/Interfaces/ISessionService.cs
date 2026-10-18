using SocialNetwork.Application.Identity.Sessions.Responses;

namespace SocialNetwork.Application.Identity.Sessions.Interfaces;

public interface ISessionService
{
    Task<IReadOnlyList<SessionResponse>> GetActiveSessionsAsync(Guid userId, string? currentRefreshToken, CancellationToken cancellationToken);
    Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);
    Task RevokeAllOtherSessionsAsync(Guid userId, string? currentRefreshToken, CancellationToken cancellationToken);
}
