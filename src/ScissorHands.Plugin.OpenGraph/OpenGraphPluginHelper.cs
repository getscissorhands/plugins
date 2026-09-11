using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

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
    /// <returns>Returns the content URL.</returns>
    public static string GetContentUrl(ContentDocument? document, SiteManifest? site)
    {
        return CombineSiteUrl(site, document?.Metadata.Slug);
    }

    /// <summary>
    /// Gets the hero image URL.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="site"><see cref="SiteManifest"/> instance.</param>
    /// <returns>Returns the hero image URL.</returns>
    public static string GetHeroImageUrl(ContentDocument? document, SiteManifest? site)
    {
        var imageUrl = string.IsNullOrWhiteSpace(document?.Metadata.HeroImage)
            ? site?.HeroImage
            : document.Metadata.HeroImage;

        return string.IsNullOrWhiteSpace(imageUrl)
            ? string.Empty
            : CombineSiteUrl(site, imageUrl);
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
        var siteUrl = site?.SiteUrl?.TrimEnd('/') ?? string.Empty;
        var baseUrl = site?.BaseUrl?.Trim('/') ?? string.Empty;

        if (string.IsNullOrEmpty(siteUrl))
        {
            return baseUrl;
        }

        return string.IsNullOrEmpty(baseUrl)
            ? siteUrl
            : $"{siteUrl}/{baseUrl}".TrimEnd('/');
    }

    private static string CombineSiteUrl(SiteManifest? site, string? path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        var siteUrl = GetSiteUrl(site);
        var relativePath = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim('/');

        if (string.IsNullOrEmpty(relativePath))
        {
            return siteUrl;
        }

        return string.IsNullOrEmpty(siteUrl)
            ? relativePath
            : $"{siteUrl}/{relativePath}";
    }
}
