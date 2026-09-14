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
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_DefaultSampleConfiguration_When_Rendered_Then_It_Should_Include_FakeAnalytics(
        bool usePlaceholders)
    {
        // Arrange
        using var context = CreateContext(usePlaceholders, loadSampleConfiguration: true);
        var configuration = context.Services.GetRequiredService<IConfiguration>();
        var manifests = configuration.GetSection("Plugins").Get<PluginManifest[]>()
            ?? throw new InvalidOperationException("The sample must configure its plugins.");
        var site = configuration.GetSection("Site").Get<SiteManifest>()
            ?? throw new InvalidOperationException("The sample must configure its site.");
        var document = new ContentDocument();
        var runner = new PluginRunner(manifests,
            new IContentPlugin[] { new OpenGraphPlugin(), new GoogleAnalyticsPlugin() }, site);

        // Act
        var rendered = context.Render<SampleLayout>(parameters => parameters
            .Add(component => component.Site, site)
            .Add(component => component.Theme, CreateTheme())
            .Add(component => component.Document, document)
            .Add(component => component.Plugins, manifests));
        var html = await runner.RunPostHtmlAsync(rendered.Markup, document, Xunit.TestContext.Current.CancellationToken);

        // Assert
        manifests.Single(plugin => plugin.Id == "google-analytics").Options!["MeasurementId"]
            .ShouldBe("G-EXAMPLE");
        html.Split("gtag/js?id=G-EXAMPLE", StringSplitOptions.None).Length.ShouldBe(2);
        html.Split("gtag('config', 'G-EXAMPLE');", StringSplitOptions.None).Length.ShouldBe(2);
        html.Split("property=\"og:title\"", StringSplitOptions.None).Length.ShouldBe(2);
        html.ShouldNotContain("<plugin:");
        html.ShouldContain("may contact Google, even with a fake measurement ID");
    }

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
            .Add(component => component.Theme, CreateTheme())
            .Add(component => component.Document, document)
            .Add(component => component.Plugins, manifests)
            .Add(component => component.Body, builder => builder.AddMarkupContent(0, "<p>Sample content</p>")));
        var html = await runner.RunPostHtmlAsync(rendered.Markup, document, Xunit.TestContext.Current.CancellationToken);

        // Assert
        html.ShouldContain("<p>Sample content</p>");
        html.ShouldContain("content=\"Sample post | Plugin preview\"");
        html.ShouldContain("content=\"https://example.com/blog/sample\"");
        html.ShouldNotContain("<plugin:");
        rendered.Find("head link[rel='stylesheet']").GetAttribute("href")
            .ShouldBe("themes/default/assets/theme.css");
        rendered.Find("head link[rel='icon']").GetAttribute("href")
            .ShouldBe("themes/default/favicon.ico");
        rendered.Find("body script[src]").GetAttribute("src")
            .ShouldBe("themes/default/assets/theme.js");
        rendered.Find("base").GetAttribute("href").ShouldBe("/blog/");
        rendered.Find(".site-header .navigation-list").ShouldNotBeNull();
        rendered.Find("#theme-toggle").GetAttribute("aria-label").ShouldBe("Switch color theme");
        rendered.Find(".site-footer").ShouldNotBeNull();
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
            .Add(component => component.Theme, CreateTheme())
            .Add(component => component.Plugins, Array.Empty<PluginManifest>()));

        // Assert
        rendered.Markup.ShouldNotContain("<plugin:");
        rendered.Markup.ShouldNotContain("property=\"og:");
        rendered.Markup.ShouldNotContain("googletagmanager.com");
        rendered.Find("link[rel='stylesheet']").GetAttribute("href")
            .ShouldBe("themes/default/assets/theme.css");
    }

    [Fact]
    public void Given_ThemeManifestChanges_When_Rerendered_Then_It_Should_Refresh_AssetReferences()
    {
        // Arrange
        using var context = CreateContext(false);
        var rendered = context.Render<SampleLayout>(parameters => parameters
            .Add(component => component.Site, new SiteManifest())
            .Add(component => component.Theme, CreateTheme()));

        // Act
        rendered.Render(parameters => parameters.Add(component => component.Theme, new ThemeManifest
        {
            Slug = "alternate",
            Stylesheets = ["/assets/replacement.css"],
            Scripts = ["/assets/replacement.js"],
        }));

        // Assert
        rendered.Find("link[rel='stylesheet']").GetAttribute("href")
            .ShouldBe("themes/alternate/assets/replacement.css");
        rendered.Find("script[src]").GetAttribute("src")
            .ShouldBe("themes/alternate/assets/replacement.js");
        rendered.Markup.ShouldNotContain("themes/default/");
    }

    [Fact]
    public void Given_EmptyAssetLists_When_Rendered_Then_It_Should_Not_Invent_AssetReferences()
    {
        // Arrange
        using var context = CreateContext(false);

        // Act
        var rendered = context.Render<SampleLayout>(parameters => parameters
            .Add(component => component.Site, new SiteManifest())
            .Add(component => component.Theme, new ThemeManifest { Slug = "default" }));

        // Assert
        rendered.FindAll("link[rel='stylesheet']").ShouldBeEmpty();
        rendered.FindAll("script[src]").ShouldBeEmpty();
    }

    private static ThemeManifest CreateTheme()
    {
        return new ThemeManifest
        {
            Slug = "default",
            Stylesheets = ["/assets/theme.css"],
            Scripts = ["/assets/theme.js"],
        };
    }

    private static BunitContext CreateContext(bool usePlaceholders, bool loadSampleConfiguration = false)
    {
        var context = new BunitContext();
        var configuration = new ConfigurationBuilder();
        if (loadSampleConfiguration)
        {
            configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
        }
        context.Services.AddSingleton<IConfiguration>(configuration
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Sample:UsePlaceholders"] = usePlaceholders.ToString(),
            })
            .Build());
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        return context;
    }
}
