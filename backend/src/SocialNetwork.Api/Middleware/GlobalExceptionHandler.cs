using Microsoft.AspNetCore.Diagnostics;
using SocialNetwork.Api.Common.Errors;
using SocialNetwork.Application.Common.Exceptions;

namespace SocialNetwork.Api.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, code, message, errors) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                ex.Code,
                ex.Message,
                ex.Errors),

            UnauthorizedException ex => (
                StatusCodes.Status401Unauthorized,
                ex.Code,
                ex.Message,
                null),

            ForbiddenException ex => (
                StatusCodes.Status403Forbidden,
                ex.Code,
                ex.Message,
                null),

            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                ex.Code,
                ex.Message,
                null),

            ConflictException ex => (
                StatusCodes.Status409Conflict,
                ex.Code,
                ex.Message,
                null),

            BusinessRuleException ex => (
                StatusCodes.Status422UnprocessableEntity,
                ex.Code,
                ex.Message,
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.",
                null)
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "Unhandled exception. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(exception,
                "Request failed with {StatusCode}. TraceId: {TraceId}",
                status,
                httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            status,
            code,
            message,
            errors,
            httpContext.TraceIdentifier);

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}
