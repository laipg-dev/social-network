namespace SocialNetwork.Application.Identity.Auth.Responses;

/// <summary>Result returned after an account is created and a verification code is sent.</summary>
public sealed record RegistrationResponse(string Message, string Email);
