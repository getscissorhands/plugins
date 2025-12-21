using System.Text.RegularExpressions;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics.Tests;

public class GoogleAnalyticsPluginTests
{
    private static readonly Regex GoogleTagRegex = new("<!-- Google tag \\(gtag\\.js\\) -->", RegexOptions.Compiled);

    [Theory]
    [InlineData("Google Analytics")]
    public void When_Instantiated_Then_Name_Should_Be(string name)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();

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
        var pg = new GoogleAnalyticsPlugin();
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
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>")]
    public async Task Given_NullPluginOptions_When_PostHtmlAsync_Invoked_Then_It_Should_Render_GoogleTag_With_EmptyMeasurementId(string html)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var plugin = new PluginManifest { Options = null };
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldNotContain("<plugin:google-analytics></plugin:google-analytics>");
        result.ShouldContain("<!-- Google tag (gtag.js) -->");
        result.ShouldContain("gtag/js?id=");
        result.ShouldContain("gtag('config', '');");
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>")]
    public async Task Given_InvalidOption_When_PostHtmlAsync_Invoked_Then_It_Should_Render_GoogleTag_With_EmptyMeasurementId(string html)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var plugin = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "SomeOtherKey", "SomeValue" }
            }
        };
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldNotContain("<plugin:google-analytics></plugin:google-analytics>");
        result.ShouldContain("<!-- Google tag (gtag.js) -->");
        result.ShouldContain("gtag/js?id=");
        result.ShouldContain("gtag('config', '');");
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>")]
    public async Task Given_NoMeasurementId_When_PostHtmlAsync_Invoked_Then_It_Should_Render_GoogleTag_With_EmptyMeasurementId(string html)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(measurementId: null);
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldNotContain("<plugin:google-analytics></plugin:google-analytics>");
        result.ShouldContain("<!-- Google tag (gtag.js) -->");
        result.ShouldContain("gtag/js?id=");
        result.ShouldContain("gtag('config', '');");
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>", "G-XXXXXXXXXX")]
    public async Task Given_MeasurementId_When_PostHtmlAsync_Invoked_Then_It_Should_Replace_Placeholder(string html, string measurementId)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(measurementId: measurementId);
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldContain($"https://www.googletagmanager.com/gtag/js?id={measurementId}");
        result.ShouldContain($"gtag('config', '{measurementId}');");
        result.ShouldNotContain("<plugin:google-analytics></plugin:google-analytics>");
        result.ShouldNotContain("{{MEASUREMENT_ID}}");
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>", "G-XXXXXXXXXX")]
    public async Task Given_MeasurementId_When_PostHtmlAsync_Invoked_Then_It_Should_Insert_GoogleTagScript(string html, string measurementId)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(measurementId: measurementId);
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldContain("<!-- Google tag (gtag.js) -->");
        result.ShouldContain("<script async src=");
        result.ShouldContain("window.dataLayer = window.dataLayer || [];");
        result.ShouldContain("function gtag(){dataLayer.push(arguments);}");
        result.ShouldContain("gtag('js', new Date());");
    }

    [Theory]
    [InlineData("<html><head></head><body>Test</body></html>", "G-XXXXXXXXXX")]
    public async Task Given_HTML_When_PostHtmlAsync_Invoked_Then_It_Should_Return_OriginalHtml(string html, string measurementId)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(measurementId: measurementId);
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldBe(html);
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body><plugin:google-analytics></plugin:google-analytics></body></html>", "G-XXXXXXXXXX", 2)]
    public async Task Given_Multiple_Placeholders_When_PostHtmlAsync_Invoked_Then_It_Should_Replace_AllOccurrences(string html, string measurementId, int expected)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(measurementId: measurementId);
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);
        var count = GoogleTagRegex.Count(result);

        // Assert
        result.ShouldNotContain("<plugin:google-analytics></plugin:google-analytics>");
        count.ShouldBe(expected);
    }

    [Theory]
    [InlineData("G-XXXXXXXXXX")]
    public async Task Given_EmptyHTML_When_PostHtmlAsync_Invoked_Then_It_Should_Return_EmptyString(string measurementId)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var html = string.Empty;
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = CreatePluginManifest(measurementId: measurementId);
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>", 12345)]
    public async Task Given_NonStringMeasurementId_When_PostHtmlAsync_Invoked_Then_It_Should_Render_GoogleTag_With_EmptyMeasurementId(string html, object measurementId)
    {
        // Arrange
        var pg = new GoogleAnalyticsPlugin();
        var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
        var plugin = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId }
            }
        };
        var site = CreateSiteManifest();

        // Act
        var result = await pg.PostHtmlAsync(html, document, plugin, site);

        // Assert
        result.ShouldNotContain("<plugin:google-analytics></plugin:google-analytics>");
        result.ShouldContain("<!-- Google tag (gtag.js) -->");
        result.ShouldContain("gtag/js?id=");
        result.ShouldContain("gtag('config', '');");
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

    private static PluginManifest CreatePluginManifest(string? measurementId = null)
    {
        return new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId },
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
