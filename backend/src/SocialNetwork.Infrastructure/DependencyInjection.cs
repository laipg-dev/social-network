using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Infrastructure.Authentication;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Infrastructure.Storage;
using SocialNetwork.Application.Identity.Auth.Interfaces;
using SocialNetwork.Application.Common.Email;
using SocialNetwork.Infrastructure.Email;

namespace SocialNetwork.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
    
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.RootPath), "Storage:RootPath is required.")
            .Validate(x => x.SignedUrlExpirationMinutes is >= 1 and <= 1440, "Storage:SignedUrlExpirationMinutes must be between 1 and 1440.")
            .Validate(x => System.Text.Encoding.UTF8.GetByteCount(x.SigningKey ?? string.Empty) >= 32, "Storage:SigningKey must contain at least 32 UTF-8 bytes.")
            .ValidateOnStart();
        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));
        services.AddHttpClient<IEmailSender, BrevoEmailSender>(client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com/");
        });
        services.AddScoped<IStorageService, LocalFileStorage>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
