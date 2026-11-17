using System.Globalization;
using System.Text;

namespace SocialNetwork.Application.Common.Text;

public static class SlugGenerator
{
    public const int MaxLength = 200;

    public static string Generate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Tách dấu tiếng Việt (NFD) rồi bỏ các combining mark.
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character)
                != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var normalized = builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D')
            .ToLowerInvariant();

        var slug = new StringBuilder(normalized.Length);
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            var isAllowed = character is >= 'a' and <= 'z'
                or >= '0' and <= '9';

            if (isAllowed)
            {
                slug.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if (!previousWasSeparator && slug.Length > 0)
            {
                slug.Append('-');
                previousWasSeparator = true;
            }
        }

        var result = slug.ToString().Trim('-');

        if (result.Length > MaxLength)
        {
            result = result[..MaxLength].Trim('-');
        }

        return result;
    }
}
