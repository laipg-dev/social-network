namespace SocialNetwork.Application.Identity.Auth.Requests;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
