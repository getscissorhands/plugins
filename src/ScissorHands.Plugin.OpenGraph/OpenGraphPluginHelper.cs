using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Urls;

namespace ScissorHands.Plugin.OpenGraph;

/// <summary>
/// This represents the helper entity for Open Graph plugin.
/// </summary>
public static class OpenGraphPluginHelper
{
    /// <summary>
    /// Gets the content URL.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="site"><see cref="SiteManifest"/> instance.</param>
    /// <returns>The absolute publication URL, or the site root for an absent/root slug.</returns>
    /// <exception cref="ArgumentException">The site publication context or content slug is invalid.</exception>
    public static string GetContentUrl(ContentDocument? document, SiteManifest? site)
    {
        var siteUrl = GetSiteUrl(site);
        if (string.IsNullOrWhiteSpace(document?.Metadata.Slug))
        {
            return siteUrl;
        }

        string contentUrl;
        try
        {
            contentUrl = ContentUrlHelper.GetContentUrl(document.Metadata.Slug);
        }
        catch (ArgumentException)
        {
            // The dependency's diagnostic may include the supplied slug. Keep context, not payload.
            throw new ArgumentException("Open Graph: Document.Metadata.Slug must not contain literal dot traversal segments.", nameof(document));
        }

        return contentUrl == "."
            ? siteUrl
            : $"{siteUrl}/{contentUrl}";
    }

    /// <summary>
    /// Gets the hero image URL.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="site"><see cref="SiteManifest"/> instance.</param>
    /// <returns>The selected image URL, or an empty string when neither image is available.</returns>
    /// <exception cref="ArgumentException">The site publication context or selected image reference is invalid.</exception>
    public static string GetHeroImageUrl(ContentDocument? document, SiteManifest? site)
    {
        var siteUrl = GetSiteUrl(site);
        var useSiteImage = string.IsNullOrWhiteSpace(document?.Metadata.HeroImage);
        var imageUrl = useSiteImage
            ? site!.HeroImage
            : document!.Metadata.HeroImage;

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return string.Empty;
        }

        var context = useSiteImage ? "Site.HeroImage" : "Document.Metadata.HeroImage";
        if (imageUrl.Any(char.IsControl) || !HasValidPercentEncoding(imageUrl))
        {
            throw InvalidImage(context);
        }

        imageUrl = imageUrl.Trim();
        var suffixStart = imageUrl.IndexOfAny(['?', '#']);
        var path = suffixStart < 0 ? imageUrl : imageUrl[..suffixStart];
        var suffix = suffixStart < 0 ? string.Empty : imageUrl[suffixStart..];
        var normalizedPath = path.Replace('\\', '/');

        // Classify BEFORE Core's leading-slash removal, including disguised network paths.
        if (normalizedPath.StartsWith("//", StringComparison.Ordinal))
        {
            throw InvalidImage(context);
        }

        var firstSlash = normalizedPath.IndexOf('/');
        var firstSegment = firstSlash < 0 ? normalizedPath : normalizedPath[..firstSlash];
        if (firstSegment.Contains(':'))
        {
            if (!IsAbsoluteWebUrl(imageUrl, out _))
            {
                throw InvalidImage(context);
            }

            // Core does not escape images. Keep original encoding, origin, suffix and trailing slash.
            return ContentUrlHelper.GetImageUrl(imageUrl);
        }

        return $"{siteUrl}/{ContentUrlHelper.GetImageUrl(normalizedPath)}{suffix}";
    }

    /// <summary>
    /// Gets the option value from the plugin manifest.
    /// </summary>
    /// <typeparam name="T">Type of the return value.</typeparam>
    /// <param name="plugin"><see cref="PluginManifest"/> instance.</param>
    /// <param name="key">The key of the option value.</param>
    /// <returns>The option value.</returns>
    public static T? GetOptionValue<T>(PluginManifest? plugin, string key)
    {
        if (plugin is null)
        {
            return default;
        }

        if (plugin.Options is null)
        {
            return default;
        }

        if (plugin.Options.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    /// <summary>
    /// Determines whether to use content metadata based on the provided documents and document.
    /// </summary>
    /// <param name="documents">Collection of <see cref="ContentDocument"/> instances.</param>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <returns>Returns <c>true</c> if content metadata should be used; otherwise, <c>false</c>.</returns>
    public static bool UseContentMetadata(IEnumerable<ContentDocument>? documents, ContentDocument? document)
    {
        if (documents is not null)
        {
            return false;
        }

        if (document is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(document.SourcePath) == true)
        {
            return false;
        }

        return true;
    }

    private static string GetSiteUrl(SiteManifest? site)
    {
        if (site is null)
        {
            throw new ArgumentException("Open Graph: Site context is required to emit metadata.", nameof(site));
        }

        if (!IsAbsoluteWebUrl(site.SiteUrl, out var origin)
            || !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment))
        {
            throw new ArgumentException("Open Graph: Site.SiteUrl must be an absolute HTTP(S) publication URL with a host and without a query or fragment.", nameof(site));
        }

        var siteUrl = site.SiteUrl.TrimEnd('/');
        var baseUrl = site.BaseUrl ?? string.Empty;
        if (baseUrl.Any(char.IsControl))
        {
            throw InvalidBaseUrl();
        }

        baseUrl = baseUrl.Trim().Replace('\\', '/');
        if (baseUrl.StartsWith("//", StringComparison.Ordinal) || baseUrl.Contains(':')
            || baseUrl.IndexOfAny(['?', '#']) >= 0
            || !HasValidPercentEncoding(baseUrl))
        {
            throw InvalidBaseUrl();
        }

        baseUrl = baseUrl.Trim('/');
        return string.IsNullOrEmpty(baseUrl) ? siteUrl : $"{siteUrl}/{baseUrl}";
    }

    private static bool IsAbsoluteWebUrl(string? value, out Uri uri)
    {
        uri = null!;
        return !string.IsNullOrWhiteSpace(value)
            && !value.Any(character => char.IsWhiteSpace(character) || char.IsControl(character))
            && !value.Contains('\\')
            && HasValidPercentEncoding(value)
            && (value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            && Uri.TryCreate(value, UriKind.Absolute, out uri!)
            && !string.IsNullOrEmpty(uri.Host)
            && uri.IsWellFormedOriginalString();
    }

    private static bool HasValidPercentEncoding(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '%')
            {
                continue;
            }

            if (index + 2 >= value.Length || !Uri.IsHexDigit(value[index + 1]) || !Uri.IsHexDigit(value[index + 2]))
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static ArgumentException InvalidImage(string context)
        => new($"Open Graph: {context} must be a site-local path or an absolute HTTP(S) image URL with a host; network paths, unsupported schemes and malformed references are not supported.");

    private static ArgumentException InvalidBaseUrl()
        => new("Open Graph: Site.BaseUrl must be a site-local publication subpath without a query or fragment.", "site");
}
