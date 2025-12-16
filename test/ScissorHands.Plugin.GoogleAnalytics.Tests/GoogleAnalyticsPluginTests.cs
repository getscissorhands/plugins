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
        var plugin = new GoogleAnalyticsPlugin();

        // Act
        var result = plugin.Name;

        // Assert
        result.ShouldBe(name);
    }

    [Theory]
    [InlineData(typeof(TaskCanceledException))]
    public void Given_CancellationToken_When_PostHtmlAsync_Invoked_Then_It_Should_Throw_TaskCanceledException(Type exception)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var html = string.Empty;
        var document = new ContentDocument();
        var manifest = new PluginManifest();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        Func<Task> func = async () => await plugin.PostHtmlAsync(html, document, manifest, cancellationTokenSource.Token);

        // Assert
        func.ShouldThrow(exception);
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>")]
    public async Task Given_InvalidOption_When_PostHtmlAsync_Invoked_Then_It_Should_Return_OriginalHtml(string html)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "SomeOtherKey", "SomeValue" }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);

        // Assert
        result.ShouldBe(html);
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>")]
    public async Task Given_NoMeasurementId_When_PostHtmlAsync_Invoked_Then_It_Should_Return_OriginalHtml(string html)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", null! }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);

        // Assert
        result.ShouldBe(html);
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>", "G-XXXXXXXXXX")]
    public async Task Given_MeasurementId_When_PostHtmlAsync_Invoked_Then_It_Should_Replace_Placeholder(string html, string measurementId)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);

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
        var plugin = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);

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
        var plugin = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);

        // Assert
        result.ShouldBe(html);
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body><plugin:google-analytics></plugin:google-analytics></body></html>", "G-XXXXXXXXXX", 2)]
    public async Task Given_Multiple_Placeholders_When_PostHtmlAsync_Invoked_Then_It_Should_Replace_AllOccurrences(string html, string measurementId, int expected)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);
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
        var plugin = new GoogleAnalyticsPlugin();
        var html = string.Empty;
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("<html><head><plugin:google-analytics></plugin:google-analytics></head><body>Test</body></html>", 12345)]
    public async Task Given_NonStringMeasurementId_When_PostHtmlAsync_Invoked_Then_It_Should_Return_OriginalHtml(string html, object measurementId)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var document = new ContentDocument();
        var manifest = new PluginManifest
        {
            Options = new Dictionary<string, object?>
            {
                { "MeasurementId", measurementId }
            }
        };

        // Act
        var result = await plugin.PostHtmlAsync(html, document, manifest);

        // Assert
        result.ShouldBe(html);
    }
}
