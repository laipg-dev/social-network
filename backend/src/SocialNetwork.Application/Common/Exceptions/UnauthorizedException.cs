namespace SocialNetwork.Application.Common.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(
        string message = "Authentication is required.",
        string code = "UNAUTHORIZED")
        : base(code, message) { }
}
