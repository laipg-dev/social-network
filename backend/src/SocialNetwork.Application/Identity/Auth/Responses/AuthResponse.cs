namespace SocialNetwork.Application.Identity.Auth.Responses;

/// <summary>Authentication result containing the access token and user summary.</summary>
public sealed record AuthResponse(
    string AccessToken,
    Guid UserId,
    string Email,
    string Username,
    string? DisplayName,
    string RefreshToken);
