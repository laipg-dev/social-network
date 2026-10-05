namespace SocialNetwork.Application.Common.Interfaces;
public interface ICurrentUser { Guid UserId { get; } string? Email { get; } string? Username { get; } bool IsAuthenticated { get; } }
