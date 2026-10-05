namespace SocialNetwork.Api.Common.Errors;

public sealed class ValidationErrorResponse
{
    public int Status { get; init; } = StatusCodes.Status400BadRequest;

    public string Code { get; init; } = "VALIDATION_ERROR";

    public string Message { get; init; } = "One or more validation errors occurred.";

    public IDictionary<string, string[]> Errors { get; init; }
        = new Dictionary<string, string[]>();
}