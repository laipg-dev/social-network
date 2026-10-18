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
using SocialNetwork.Application.Identity.Profile.Interfaces;
using SocialNetwork.Infrastructure.Identity;
using SocialNetwork.Application.Content.Comments.Interfaces;
using SocialNetwork.Infrastructure.Content.Comments;
using SocialNetwork.Application.Content.Reactions.Interfaces;
using SocialNetwork.Infrastructure.Content.Reactions;
using SocialNetwork.Application.Content.Bookmarks.Interfaces;
using SocialNetwork.Infrastructure.Content.Bookmarks;
using SocialNetwork.Application.Content.Media.Interfaces;
using SocialNetwork.Infrastructure.Content.Media;
using SocialNetwork.Application.Communities.Interfaces;
using SocialNetwork.Infrastructure.Communities;

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
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IPostReactionService, PostReactionService>();
    builder.Services.AddScoped<ICommentReactionService, CommentReactionService>();
    builder.Services.AddScoped<IBookmarkService, BookmarkService>();
    builder.Services.AddScoped<IPostMediaService, PostMediaService>();
    builder.Services.AddScoped<ICommunityService, CommunityService>();


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

app.UseCors(CorsExtensions.PolicyName);

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider =
            new PhysicalFileProvider(uploadPath),
        RequestPath = "/uploads",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        }
    });
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();
    
app.MapControllers();

app.MapHub<RealtimeHub>("/hubs/realtime");
app.Run();
