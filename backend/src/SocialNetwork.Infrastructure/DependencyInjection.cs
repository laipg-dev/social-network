using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.Infrastructure.Persistence;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Infrastructure.Authentication;
using SocialNetwork.Application.Common.Storage;
using SocialNetwork.Infrastructure.Storage;

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
        services.AddScoped<IFileStorage, LocalFileStorage>();
        return services;
    }
}