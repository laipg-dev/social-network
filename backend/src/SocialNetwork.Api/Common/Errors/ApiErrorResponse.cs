namespace SocialNetwork.Api.Common.Errors;

public sealed record ApiErrorResponse(
    int Status,
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors = null,
    string? TraceId = null);
