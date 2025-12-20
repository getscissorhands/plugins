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
        var siteUrl = GetSiteUrl(site);
        var contentUrl = document?.Metadata.Slug.TrimStart('/');

        return $"{siteUrl}/{contentUrl}".TrimEnd('/');
    }

    /// <summary>
    /// Gets the hero image URL.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="site"><see cref="SiteManifest"/> instance.</param>
    /// <returns>Returns the hero image URL.</returns>
    public static string GetHeroImageUrl(ContentDocument? document, SiteManifest? site)
    {
        var siteUrl = GetSiteUrl(site);
        var siteHeroImage = site?.HeroImage?.TrimStart('/');
        var contentImage = document?.Metadata.HeroImage?.TrimStart('/');

        return contentImage is null
               ? $"{siteUrl}/{siteHeroImage}"
               : $"{siteUrl}/{contentImage}";
    }

    private static string GetSiteUrl(SiteManifest? site)
    {
        var siteUrl = site?.SiteUrl.TrimEnd('/');
        var baseUrl = site?.BaseUrl.Trim('/');

        return $"{siteUrl}/{baseUrl}".TrimEnd('/');
    }
}
