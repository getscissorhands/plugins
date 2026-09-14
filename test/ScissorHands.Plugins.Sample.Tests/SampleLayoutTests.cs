using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Plugin;
using ScissorHands.Plugin.GoogleAnalytics;
using ScissorHands.Plugin.OpenGraph;
using ScissorHands.Web.Runners;

namespace ScissorHands.Plugins.Sample.Tests;

public class SampleLayoutTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Given_RenderingMode_When_Rendered_Then_It_Should_Emit_OnlyConfiguredPluginOutput(
        bool usePlaceholders,
        bool enableAnalytics)
    {
        // Arrange
        using var context = CreateContext(usePlaceholders);
        var site = new SiteManifest
        {
            Title = "Plugin preview",
            Description = "Site description",
            SiteUrl = "https://example.com",
            BaseUrl = "/blog/",
            HeroImage = "/images/sample.svg",
        };
        var document = new ContentDocument
        {
            Kind = ContentKind.Post,
            SourcePath = "contents/posts/sample.md",
            Metadata = new ContentMetadata { Title = "Sample post", Slug = "sample" },
        };
        var manifests = new List<PluginManifest>
        {
            new() { Id = "open-graph" },
        };
        if (enableAnalytics)
        {
            manifests.Add(new PluginManifest
            {
                Id = "google-analytics",
                Options = new Dictionary<string, object?> { ["MeasurementId"] = "G-EXAMPLE" },
            });
        }
        var runner = new PluginRunner(manifests,
            new IContentPlugin[] { new OpenGraphPlugin(), new GoogleAnalyticsPlugin() }, site);

        // Act
        var rendered = context.Render<SampleLayout>(parameters => parameters
            .Add(component => component.Site, site)
            .Add(component => component.Document, document)
            .Add(component => component.Plugins, manifests)
            .Add(component => component.Body, builder => builder.AddMarkupContent(0, "<p>Sample content</p>")));
        var html = await runner.RunPostHtmlAsync(rendered.Markup, document, Xunit.TestContext.Current.CancellationToken);

        // Assert
        html.ShouldContain("<p>Sample content</p>");
        html.ShouldContain("content=\"Sample post | Plugin preview\"");
        html.ShouldContain("content=\"https://example.com/blog/sample\"");
        html.ShouldNotContain("<plugin:");
        html.Split("property=\"og:title\"", StringSplitOptions.None).Length.ShouldBe(2);
        html.IndexOf("property=\"og:title\"", StringComparison.Ordinal)
            .ShouldBeLessThan(html.IndexOf("</head>", StringComparison.Ordinal));
        if (enableAnalytics)
        {
            html.Split("gtag/js?id=G-EXAMPLE", StringSplitOptions.None).Length.ShouldBe(2);
            html.IndexOf("gtag/js?id=G-EXAMPLE", StringComparison.Ordinal)
                .ShouldBeLessThan(html.IndexOf("</head>", StringComparison.Ordinal));
        }
        else
        {
            html.ShouldNotContain("googletagmanager.com");
            html.ShouldNotContain("gtag('config'");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_NoManifests_When_Rendered_Then_It_Should_Omit_PluginMarkersAndOutput(bool usePlaceholders)
    {
        // Arrange
        using var context = CreateContext(usePlaceholders);

        // Act
        var rendered = context.Render<SampleLayout>(parameters => parameters
            .Add(component => component.Site, new SiteManifest())
            .Add(component => component.Plugins, Array.Empty<PluginManifest>()));

        // Assert
        rendered.Markup.ShouldNotContain("<plugin:");
        rendered.Markup.ShouldNotContain("property=\"og:");
        rendered.Markup.ShouldNotContain("googletagmanager.com");
    }

    private static BunitContext CreateContext(bool usePlaceholders)
    {
        var context = new BunitContext();
        context.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Sample:UsePlaceholders"] = usePlaceholders.ToString(),
            })
            .Build());
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        return context;
    }
}
