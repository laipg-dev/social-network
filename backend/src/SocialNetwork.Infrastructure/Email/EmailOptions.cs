namespace SocialNetwork.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; init; }
    public string ApiKey { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "Social Network";
    public string FrontendBaseUrl { get; init; } = "http://localhost:3000";
}
