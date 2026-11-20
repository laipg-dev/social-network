using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SocialNetwork.Application.Common.Storage;

namespace SocialNetwork.Api.Filters;

/// <summary>Resolves persisted avatar keys on response DTOs using the existing private storage signer.</summary>
public sealed partial class AvatarSignedUrlResultFilter(
    IStorageService storage) : IAsyncResultFilter
{
    private static readonly HashSet<string> ProfileAvatarProperties = new(StringComparer.Ordinal)
    {
        "ProfileResponse.AvatarUrl",
        "PublicUserResponse.AvatarUrl",
        "CommunityMemberResponse.AvatarUrl",
        "RoomMemberResponse.AvatarUrl",
        "PostResponse.AuthorAvatarUrl",
        "CommunityResponse.OwnerAvatarUrl"
    };

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var value = context.Result switch
        {
            ObjectResult result => result.Value,
            JsonResult result => result.Value,
            _ => null
        };

        if (value is not null)
        {
            var cache = new Dictionary<string, string?>(StringComparer.Ordinal);
            await ResolveObjectAsync(value, new HashSet<object>(ReferenceEqualityComparer.Instance), cache,
                context.HttpContext.Request, context.HttpContext.RequestAborted);
        }

        await next();
    }

    private async Task ResolveObjectAsync(object value, HashSet<object> visited, Dictionary<string, string?> cache,
        HttpRequest request, CancellationToken cancellationToken)
    {
        var type = value.GetType();
        if (value is string || type.IsPrimitive || type.IsEnum || type.IsValueType || !visited.Add(value)) return;

        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
                if (item is not null)
                    await ResolveObjectAsync(item, visited, cache, request, cancellationToken);
            return;
        }

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
        {
            var current = property.GetValue(value);
            if (property.PropertyType == typeof(string) && current is string candidate
                && ProfileAvatarProperties.Contains($"{type.Name}.{property.Name}")
                && AvatarKeyRegex().IsMatch(candidate))
            {
                if (!cache.TryGetValue(candidate, out var signedUrl))
                {
                    try
                    {
                        var origin = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}");
                        signedUrl = (await storage.CreateSignedUrlAsync(candidate, origin, cancellationToken)).Url;
                    }
                    catch (FileNotFoundException)
                    {
                        signedUrl = null;
                    }
                    cache[candidate] = signedUrl;
                }

                if (property.SetMethod is not null)
                    property.SetValue(value, signedUrl);
            }
            else if (current is not null && property.PropertyType != typeof(string))
            {
                await ResolveObjectAsync(current, visited, cache, request, cancellationToken);
            }
        }
    }

    [GeneratedRegex("^avatars/[0-9a-fA-F]{32}$", RegexOptions.CultureInvariant)]
    private static partial Regex AvatarKeyRegex();
}
