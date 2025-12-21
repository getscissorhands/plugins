using System.Text.RegularExpressions;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph.Tests;

public class OpenGraphPluginTests
{
    private static readonly Regex OpenGraphTitleRegex = new("property=\"og:title\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [Theory]
    [InlineData("Open Graph")]
    public void When_Instantiated_Then_Name_Should_Be(string name)
    {
        // Arrange
        var pg = new OpenGraphPlugin();

        // Act
        var result = pg.Name;

        // Assert
        result.ShouldBe(name);
    }

    [Theory]
    [InlineData(typeof(TaskCanceledException))]
    public void Given_CancellationToken_When_PostHtmlAsync_Invoked_Then_It_Should_Throw_TaskCanceledException(Type exception)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var html = string.Empty;
        var document = new ContentDocument();
        var plugin = new PluginManifest();
        var site = new SiteManifest();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        Func<Task> func = async () => await pg.PostHtmlAsync(html, document, plugin, site, cancellationTokenSource.Token);

        // Assert
        func.ShouldThrowAsync(exception);
    }

    [Theory]
    [InlineData("<html><head><plugin:open-graph></plugin:open-graph></head><body>Test</body></html>")]
    public async Task Given_NullPluginOptions_When_PostHtmlAsync_Invoked_Then_It_Should_Render_OpenGraph_Without_TwitterSite_And_Creator(string html)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = new PluginManifest { Options = null };
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldNotContain("<plugin:open-graph></plugin:open-graph>");
        result.ShouldContain("property=\"og:title\"");
        result.ShouldContain("property=\"og:description\"");
        result.ShouldContain("property=\"og:url\"");
        result.ShouldContain("property=\"og:image\"");
        result.ShouldContain("name=\"twitter:card\"");
        result.ShouldNotContain("name=\"twitter:site\"");
        result.ShouldNotContain("name=\"twitter:creator\"");
    }

    [Theory]
    [InlineData("<html><head><plugin:open-graph></plugin:open-graph></head><body>Test</body></html>")]
    public async Task Given_ValidInputs_When_PostHtmlAsync_Invoked_Then_It_Should_Replace_Placeholder(string html)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", description: "Post description", heroImage: "/images/hero.png");
        var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
        var site = CreateSiteManifest(siteUrl: "https://example.com", baseUrl: "", title: "My Blog", description: "Site description", locale: "en-US", heroImage: "/images/site-hero.png");

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldNotContain("<plugin:open-graph></plugin:open-graph>");
        result.ShouldContain("property=\"og:title\"");
        result.ShouldContain("property=\"og:description\"");
        result.ShouldContain("property=\"og:locale\"");
        result.ShouldContain("property=\"og:url\"");
        result.ShouldContain("property=\"og:image\"");
        result.ShouldContain("property=\"og:site_name\"");
        result.ShouldContain("name=\"twitter:card\"");
        result.ShouldContain("name=\"twitter:site\" content=\"@site\"");
        result.ShouldContain("name=\"twitter:creator\" content=\"@creator\"");
        result.ShouldNotContain("{{CONTENT_TITLE}}");
        result.ShouldNotContain("{{CONTENT_DESCRIPTION}}");
        result.ShouldNotContain("{{CONTENT_LOCALE}}");
        result.ShouldNotContain("{{CONTENT_URL}}");
        result.ShouldNotContain("{{CONTENT_HERO_IMAGE_URL}}");
        result.ShouldNotContain("{{SITE_NAME}}");
    }

    [Theory]
    [InlineData("<html><head><plugin:open-graph></plugin:open-graph></head><body>Test</body></html>")]
    public async Task Given_NoTwitterSiteId_When_PostHtmlAsync_Invoked_Then_It_Should_Not_Render_TwitterSiteTag(string html)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(twitterSiteId: null, twitterCreatorId: "@creator");
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldNotContain("<plugin:open-graph></plugin:open-graph>");
        result.ShouldNotContain("name=\"twitter:site\"");
    }

    [Theory]
    [InlineData("<html><head><plugin:open-graph></plugin:open-graph></head><body>Test</body></html>")]
    public async Task Given_DocumentIsNotPost_When_PostHtmlAsync_Invoked_Then_It_Should_Not_Render_TwitterCreatorTag(string html)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var document = CreateDocument(kind: ContentKind.Page, title: "About", slug: "/about", twitterHandle: "@handle");
        var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldContain("name=\"twitter:site\" content=\"@site\"");
        result.ShouldNotContain("name=\"twitter:creator\"");
    }

    [Theory]
    [InlineData("<html><head><plugin:open-graph></plugin:open-graph></head><body>Test</body></html>")]
    public async Task Given_TwitterHandleInMetadata_When_PostHtmlAsync_Invoked_Then_It_Should_Override_TwitterCreatorIdOption(string html)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", twitterHandle: "@from-metadata");
        var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@from-options");
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldContain("name=\"twitter:creator\" content=\"@from-metadata\"");
        result.ShouldNotContain("name=\"twitter:creator\" content=\"@from-options\"");
    }

    [Theory]
    [InlineData("<html><head><plugin:open-graph></plugin:open-graph></head><body>Test</body></html>")]
    public async Task Given_NoDocumentDescription_When_PostHtmlAsync_Invoked_Then_It_Should_FallbackTo_SiteDescription(string html)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", description: null);
        var plugin = CreatePluginManifest(twitterSiteId: null, twitterCreatorId: null);
        var site = CreateSiteManifest(description: "Site description");

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);
        // Assert
        result.ShouldContain("property=\"og:description\" content=\"Site description\"");
        result.ShouldContain("name=\"twitter:description\" content=\"Site description\"");
    }

    [Theory]
    [InlineData("<html><head><plugin:open-graph></plugin:open-graph></head><body><plugin:open-graph></plugin:open-graph></body></html>", 2)]
    public async Task Given_MultiplePlaceholders_When_PostHtmlAsync_Invoked_Then_It_Should_Replace_AllOccurrences(string html, int expected)
    {
        // Arrange
        var pg = new OpenGraphPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(twitterSiteId: null, twitterCreatorId: null);
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);
        var count = OpenGraphTitleRegex.Count(result);

        // Assert
        result.ShouldNotContain("<plugin:open-graph></plugin:open-graph>");
        count.ShouldBe(expected);
    }

    private static ContentDocument CreateDocument(
        ContentKind kind,
        string title,
        string slug,
        string? description = "Document description",
        string? heroImage = "/images/doc-hero.png",
        string? twitterHandle = null)
    {
        return new ContentDocument
        {
            Kind = kind,
            Metadata = new ContentMetadata
            {
                Title = title,
                Slug = slug,
                Description = description,
                HeroImage = heroImage,
                TwitterHandle = twitterHandle,
            }
        };
    }

    private static PluginManifest CreatePluginManifest(string? twitterSiteId = null, string? twitterCreatorId = null)
    {
        return new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "TwitterSiteId", twitterSiteId },
                { "TwitterCreatorId", twitterCreatorId },
            }
        };
    }

    private static SiteManifest CreateSiteManifest(
        string siteUrl = "https://example.com",
        string baseUrl = "",
        string title = "Site title",
        string description = "Site description",
        string locale = "en-US",
        string heroImage = "/images/site-hero.png")
    {
        return new SiteManifest
        {
            SiteUrl = siteUrl,
            BaseUrl = baseUrl,
            Title = title,
            Description = description,
            Locale = locale,
            HeroImage = heroImage,
        };
    }
}
