namespace SocialNetwork.Application.Common.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.")
        : base("UNAUTHORIZED", message) { }
}
