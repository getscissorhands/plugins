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

        if (Plugin is null)
        {
            return;
        }

        var metadata = OpenGraphMetadata.Create(Plugin, Site, Document, Documents);
        ContentTitle = metadata.Title;
        ContentDescription = metadata.Description;
        ContentLocale = metadata.Locale;
        ContentUrl = metadata.Url;
        HeroImageUrl = metadata.ImageUrl;
        SiteName = metadata.SiteName;
        TwitterSiteId = metadata.TwitterSiteId;
        TwitterCreatorId = metadata.TwitterCreatorId;
    }
}