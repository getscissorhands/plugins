using AngleSharp.Html.Parser;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics.Tests;

public class GoogleAnalyticsPluginTests
{
    [Fact]
    public void Given_Plugin_When_Instantiated_Then_It_Should_Preserve_Identity_And_Declare_No_Dependencies()
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();

        // Act
        var id = plugin.Id;
        var name = plugin.Name;
        var dependencies = plugin.DependsOn;

        // Assert
        id.ShouldBe("google-analytics");
        name.ShouldBe("Google Analytics");
        dependencies.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(GoogleAnalyticsTestData.InvalidConfigurationCases), MemberType = typeof(GoogleAnalyticsTestData))]
    public async Task Given_InvalidConfiguration_When_PostHtmlAsync_Then_It_Should_Throw_Contextual_NonLeaking_Error(string scenario)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var manifest = new PluginManifest
        {
            Id = "google-analytics",
            Options = GoogleAnalyticsTestData.CreateInvalidOptions(scenario),
        };

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => plugin.PostHtmlAsync(
            GoogleAnalyticsTestData.Marker, new ContentDocument(), manifest, new SiteManifest(),
            Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldBe(GoogleAnalyticsTestData.ConfigurationError);
        exception.InnerException.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("<html><head></head><body>Unmarked</body></html>")]
    [InlineData("<plugin:google-analytics />")]
    public async Task Given_InvalidConfigurationWithoutPairedMarkers_When_PostHtmlAsync_Then_It_Should_Still_Fail(string html)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var manifest = new PluginManifest { Id = "google-analytics", Options = null };

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => plugin.PostHtmlAsync(
            html, new ContentDocument(), manifest, new SiteManifest(), Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldBe(GoogleAnalyticsTestData.ConfigurationError);
    }

    [Theory]
    [InlineData("G-EXAMPLE")]
    [InlineData("G-A")]
    [InlineData("G-0")]
    [InlineData("G-A1B2C3")]
    [InlineData("G-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public async Task Given_ValidSyntheticMeasurementId_When_PostHtmlAsync_Then_It_Should_Emit_Intact_Safe_Tag(string measurementId)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var manifest = GoogleAnalyticsTestData.CreateManifest(measurementId);

        // Act
        var result = await plugin.PostHtmlAsync(
            $"<html><head>{GoogleAnalyticsTestData.Marker}</head><body>Preserved</body></html>",
            new ContentDocument(), manifest, new SiteManifest(), Xunit.TestContext.Current.CancellationToken);
        var document = new HtmlParser().ParseDocument(result);

        // Assert
        GoogleAnalyticsTestData.AssertTag(document.QuerySelectorAll("script"), measurementId);
        result.ShouldContain("<!-- Google tag (gtag.js) -->");
        result.ShouldContain("<body>Preserved</body>");
        result.ShouldNotContain("<plugin:");
        result.ShouldNotContain("{{");
    }

    [Theory]
    [InlineData("")]
    [InlineData("<html><head></head><body>Unmarked</body></html>")]
    [InlineData("<plugin:google-analytics />")]
    [InlineData("<plugin:google-analytics/>")]
    [InlineData("<plugin:google-analytics>content</plugin:google-analytics>")]
    [InlineData("<plugin:google-analytics> </plugin:google-analytics>")]
    [InlineData("<plugin:google-analytics>")]
    [InlineData("</plugin:google-analytics>")]
    public async Task Given_NoExactPairedMarker_When_PostHtmlAsync_Then_It_Should_Preserve_Original_Html(string html)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var manifest = GoogleAnalyticsTestData.CreateManifest("G-EXAMPLE");

        // Act
        var result = await plugin.PostHtmlAsync(
            html, new ContentDocument(), manifest, new SiteManifest(), Xunit.TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(html);
    }

    [Theory]
    [InlineData("<plugin:google-analytics></plugin:google-analytics>")]
    [InlineData("<PLUGIN:GOOGLE-ANALYTICS></PLUGIN:GOOGLE-ANALYTICS>")]
    [InlineData("<Plugin:Google-Analytics></pLuGiN:gOoGlE-aNaLyTiCs>")]
    public async Task Given_RepeatedCaseInsensitiveMarkers_When_PostHtmlAsync_Then_It_Should_Replace_Every_Pair_Without_Deduplication(string marker)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var manifest = GoogleAnalyticsTestData.CreateManifest("G-EXAMPLE");
        const string existing = "<script>gtag('config', 'G-EXAMPLE');</script>";
        var html = $"<html><head>{existing}{marker}{marker}</head><body>Preserved</body></html>";

        // Act
        var result = await plugin.PostHtmlAsync(
            html, new ContentDocument(), manifest, new SiteManifest(), Xunit.TestContext.Current.CancellationToken);
        var scripts = new HtmlParser().ParseDocument(result).QuerySelectorAll("script");

        // Assert
        scripts.Length.ShouldBe(5);
        scripts.Count(script => script.HasAttribute("src")).ShouldBe(2);
        result.ShouldContain(existing);
        result.ShouldContain("<body>Preserved</body>");
        result.ShouldNotContain(marker);
    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "/blog")]
    [InlineData(true, "/blog")]
    public async Task Given_PublicationContext_When_PostHtmlAsync_Then_It_Should_Keep_Configured_External_Tag(bool isPreview, string baseUrl)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var manifest = GoogleAnalyticsTestData.CreateManifest("G-EXAMPLE");
        var site = new SiteManifest { IsPreview = isPreview, BaseUrl = baseUrl, SiteUrl = "https://example.test", Locale = "ko-KR" };

        // Act
        var result = await plugin.PostHtmlAsync(
            GoogleAnalyticsTestData.Marker, new ContentDocument(), manifest, site, Xunit.TestContext.Current.CancellationToken);
        var document = new HtmlParser().ParseDocument(result);

        // Assert
        GoogleAnalyticsTestData.AssertTag(document.QuerySelectorAll("script"), "G-EXAMPLE");
        result.ShouldNotContain("example.test");
        result.ShouldNotContain("/blog");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_CancelledToken_When_PostHtmlAsync_Then_It_Should_Cancel_At_Entry_Before_Configuration_Access(bool nullArguments)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var manifest = nullArguments ? null : new PluginManifest { Id = "google-analytics", Options = null };

        // Act
        var exception = await Should.ThrowAsync<OperationCanceledException>(() => plugin.PostHtmlAsync(
            nullArguments ? null! : GoogleAnalyticsTestData.Marker, null!, manifest!, null!, cancellation.Token));

        // Assert
        exception.CancellationToken.ShouldBe(cancellation.Token);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_ReadOnlyOptionsWithNestedValues_When_PostHtmlAsync_Then_It_Should_Preserve_Caller_Ownership(bool invalid)
    {
        // Arrange
        var plugin = new GoogleAnalyticsPlugin();
        var snapshot = new GoogleAnalyticsTestData.OptionsSnapshot(invalid);

        // Act
        if (invalid)
        {
            await Should.ThrowAsync<InvalidOperationException>(() => plugin.PostHtmlAsync(
                GoogleAnalyticsTestData.Marker, new ContentDocument(), snapshot.Manifest, new SiteManifest(),
                Xunit.TestContext.Current.CancellationToken));
        }
        else
        {
            await plugin.PostHtmlAsync(
                GoogleAnalyticsTestData.Marker, new ContentDocument(), snapshot.Manifest, new SiteManifest(),
                Xunit.TestContext.Current.CancellationToken);
        }

        // Assert
        snapshot.AssertUnchanged();
    }

    [Fact]
    public async Task Given_SourceOptionsChangedAfterManifestCreation_When_PostHtmlAsync_Then_It_Should_Use_Upstream_Snapshot()
    {
        // Arrange
        var source = new Dictionary<string, object?> { ["MeasurementId"] = "G-ORIGINAL" };
        var manifest = new PluginManifest { Id = "google-analytics", Options = source };
        source["MeasurementId"] = "G-MUTATED";
        var plugin = new GoogleAnalyticsPlugin();

        // Act
        var result = await plugin.PostHtmlAsync(
            GoogleAnalyticsTestData.Marker, new ContentDocument(), manifest, new SiteManifest(),
            Xunit.TestContext.Current.CancellationToken);

        // Assert
        result.ShouldContain("gtag('config', 'G-ORIGINAL');");
        result.ShouldNotContain("G-MUTATED");
        source["MeasurementId"].ShouldBe("G-MUTATED");
    }
}
