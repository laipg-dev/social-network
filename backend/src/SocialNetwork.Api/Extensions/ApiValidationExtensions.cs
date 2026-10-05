using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Api.Common.Errors;

namespace SocialNetwork.Api.Extensions;

public static class ApiValidationExtensions
{
    public static IServiceCollection AddApiValidation(
        this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors
                            .Select(error =>
                                string.IsNullOrWhiteSpace(error.ErrorMessage)
                                    ? "Invalid value."
                                    : error.ErrorMessage)
                            .ToArray());

                var response = new ValidationErrorResponse
                {
                    Errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}