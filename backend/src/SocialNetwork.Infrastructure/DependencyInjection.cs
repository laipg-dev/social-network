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
        
        services.Configure<StorageOptions>(
            configuration.GetSection(StorageOptions.SectionName));
        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));
        services.AddHttpClient<IEmailSender, BrevoEmailSender>(client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com/");
        });
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}