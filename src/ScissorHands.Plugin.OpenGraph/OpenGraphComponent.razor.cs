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
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (Plugin is null)
        {
            return;
        }

        TwitterSiteId = Plugin.Options?["TwitterSiteId"] is string siteIdValue ? siteIdValue : default;

        TwitterCreatorId = Plugin.Options?["TwitterCreatorId"] is string creatorIdValue ? creatorIdValue : default;
        TwitterCreatorId = Document?.Metadata.TwitterHandle is null ? TwitterCreatorId : Document.Metadata.TwitterHandle;
        if (Documents?.Any() == true || Document?.Kind == ContentKind.Page)
        {
            TwitterCreatorId = default;
        }

        ContentTitle = Documents?.Any() == true ? Site!.Title : Document?.Metadata.Title;
        ContentDescription = Documents?.Any() == true ? Site!.Description : Document?.Metadata.Description;
        ContentLocale = Site!.Locale;

        ContentUrl = $"{OpenGraphPluginHelper.GetContentUrl(Document, Site)}";
        HeroImageUrl = $"{OpenGraphPluginHelper.GetHeroImageUrl(Document, Site)}";
        SiteName = Site!.Title;
    }
}