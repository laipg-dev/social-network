namespace SocialNetwork.Application.Identity.Auth.Responses;

/// <summary>Result of an account action that normally sends a token by email.</summary>
public sealed record ActionTokenResponse(string Message, string? DevelopmentToken);
