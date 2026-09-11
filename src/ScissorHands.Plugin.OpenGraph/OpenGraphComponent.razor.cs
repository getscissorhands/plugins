using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph;

/// <summary>
/// This represents the UI component entity for Open Graph.
/// </summary>
public partial class OpenGraphComponent : PluginComponentBase
{
    /// <summary>
    /// Gets or sets the content title.
    /// </summary>
    protected string? ContentTitle { get; set; }

    /// <summary>
    /// Gets or sets the content description.
    /// </summary>
    protected string? ContentDescription { get; set; }

    /// <summary>
    /// Gets or sets the content locale.
    /// </summary>
    protected string? ContentLocale { get; set; }

    /// <summary>
    /// Gets or sets the content URL.
    /// </summary>
    protected string? ContentUrl { get; set; }

    /// <summary>
    /// Gets or sets the hero image URL.
    /// </summary>
    protected string? HeroImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the site name.
    /// </summary>
    protected string? SiteName { get; set; }

    /// <summary>
    /// Gets or sets the Twitter site ID.
    /// </summary>
    protected string? TwitterSiteId { get; set; }

    /// <summary>
    /// Gets or sets the Twitter creator ID.
    /// </summary>
    protected string? TwitterCreatorId { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ContentTitle = default;
        ContentDescription = default;
        ContentLocale = default;
        ContentUrl = default;
        HeroImageUrl = default;
        SiteName = default;
        TwitterSiteId = default;
        TwitterCreatorId = default;

        if (Plugin is null || Site is null)
        {
            return;
        }

        if (Plugin.Options is not null)
        {
            TwitterSiteId = OpenGraphPluginHelper.GetOptionValue<string>(Plugin, "TwitterSiteId");
            TwitterCreatorId = OpenGraphPluginHelper.GetOptionValue<string>(Plugin, "TwitterCreatorId");
        }

        TwitterCreatorId = string.IsNullOrWhiteSpace(Document?.Metadata.TwitterHandle)
            ? TwitterCreatorId
            : Document.Metadata.TwitterHandle;

        var useContentMetadata = OpenGraphPluginHelper.UseContentMetadata(Documents, Document);
        if (useContentMetadata == false || Document?.Kind == ContentKind.Page)
        {
            TwitterCreatorId = default;
        }

        ContentTitle = useContentMetadata ? $"{Document?.Metadata.Title} | {Site.Title}" : Site.Title;
        ContentDescription = useContentMetadata ? Document?.Metadata.Description ?? Site.Description : Site.Description;
        ContentLocale = Site.Locale;

        ContentUrl = OpenGraphPluginHelper.GetContentUrl(Document, Site);
        HeroImageUrl = OpenGraphPluginHelper.GetHeroImageUrl(Document, Site);
        SiteName = Site.Title;
    }
}