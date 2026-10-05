namespace SocialNetwork.Application.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base("NOT_FOUND", message) { }

    public NotFoundException(string resourceName, object key)
        : base("NOT_FOUND", $"{resourceName} with id '{key}' was not found.") { }
}
