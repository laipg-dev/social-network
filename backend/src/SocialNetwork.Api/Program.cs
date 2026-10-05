using SocialNetwork.Infrastructure;
using SocialNetwork.Api.Middleware;
using SocialNetwork.Api.Extensions;
using SocialNetwork.Api.Hubs;
using Microsoft.Extensions.FileProviders;
using SocialNetwork.Infrastructure.Persistence.Seed;
using SocialNetwork.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using SocialNetwork.Application.Content.Posts.Interfaces;
using SocialNetwork.Application.Content.Posts.Services;
using SocialNetwork.Infrastructure.Persistence.Repositories;
using SocialNetwork.Application.Common.Realtime;
using SocialNetwork.Api.Realtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerDocumentation();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApiValidation();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy(
    builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<
    IRealtimeNotifier,
    SignalRRealtimeNotifier>();
    
    builder.Services.AddScoped<IPostService, PostService>();
    builder.Services.AddScoped<IPostRepository, PostRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await DatabaseSeeder.SeedAsync(dbContext);
}

var uploadPath = Path.Combine(
    app.Environment.ContentRootPath,
    "uploads");

Directory.CreateDirectory(uploadPath);

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider =
            new PhysicalFileProvider(uploadPath),

        RequestPath = "/uploads"
    });
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.PolicyName);

app.UseAuthentication();

app.UseAuthorization();
    
app.MapControllers();

app.MapHub<RealtimeHub>("/hubs/realtime");
app.Run();
