using System.IO.Abstractions;

using AngleSharp.Html.Parser;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Plugin;
using ScissorHands.Plugin.GoogleAnalytics;
using ScissorHands.Plugin.OpenGraph;
using ScissorHands.Web;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;

namespace ScissorHands.Plugins.Sample.Tests;

public class SampleTagRouteTests
{
    [Theory]
    [InlineData("/", false)]
    [InlineData("/", true)]
    [InlineData("/blog/", false)]
    [InlineData("/blog/", true)]
    public async Task Given_ReleasedGenerator_When_BuildingTagPages_Then_It_Should_PreserveCanonicalUrlsInBothModes(
        string baseUrl, bool preview)
    {
        // Arrange
        var temporary = Directory.CreateTempSubdirectory("ScissorHands-tag-routes-");
        try
        {
            var paths = Substitute.For<IAppPaths>();
            paths.GetContentsRoot().Returns(Path.Combine(temporary.FullName, "contents"));
            var site = new SiteManifest
            {
                Title = "Site title",
                Description = "Site description",
                Locale = "ko-KR",
                SiteUrl = "https://example.com",
                BaseUrl = baseUrl,
                HeroImage = null,
                UseLocaleInUrl = true,
            };
            var documents = new[]
            {
                new ContentDocument
                {
                    Kind = ContentKind.Post,
                    SourcePath = "contents/posts/post.md",
                    Markdown = "Post body",
                    Metadata = new ContentMetadata
                    {
                        Title = "Post title",
                        Slug = "ko-kr/post",
                        TwitterHandle = "@post-author",
                        Tags = ["Plugins", "  C# / <Tools>  ", "100%", "literal%2F", "\uD55C\uAE00"],
                    },
                },
                new ContentDocument
                {
                    Kind = ContentKind.Page,
                    SourcePath = "contents/pages/about.md",
                    Markdown = "Page body",
                    Metadata = new ContentMetadata { Title = "About", Slug = "ko-kr/about", Tags = ["Plugins"] },
                },
            };
            var manifests = new[]
            {
                new PluginManifest
                {
                    Id = "open-graph",
                    Options = new Dictionary<string, object?> { ["TwitterCreatorId"] = "@site-author" },
                },
                new PluginManifest
                {
                    Id = "google-analytics",
                    Options = new Dictionary<string, object?> { ["MeasurementId"] = "G-EXAMPLE" },
                },
            };
            var loader = Substitute.For<IContentLoader>();
            loader.LoadAsync(Arg.Any<CancellationToken>()).Returns(documents);
            var markdown = Substitute.For<IMarkdownService>();
            markdown.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
                .Returns(call => $"<p>{call.ArgAt<string>(0)}</p>");
            var themeService = Substitute.For<IThemeService>();
            themeService.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new ThemeManifest { Slug = "default" });
            var expectedRoutes = new[]
            {
                "", "404.html", "ko-kr/post", "ko-kr/about", "tags", "tags/plugins",
                "tags/c%23%20%2F%20%3Ctools%3E", "tags/100%25", "tags/literal%252f", "tags/%ED%95%9C%EA%B8%80",
            };
            var components = new Dictionary<string, Dictionary<string, string?>>();

            foreach (var usePlaceholders in new[] { false, true })
            {
                var destination = Path.Combine(temporary.FullName, usePlaceholders ? "hooks" : "components");
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddSingleton(themeService);
                services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Sample:UsePlaceholders"] = usePlaceholders.ToString(),
                    })
                    .Build());
                using var provider = services.BuildServiceProvider();
                var renderer = new ComponentRenderer(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    provider.GetRequiredService<ILoggerFactory>());
                var runner = new PluginRunner(manifests,
                    new IContentPlugin[] { new OpenGraphPlugin(), new GoogleAnalyticsPlugin() }, site);
                var generator = new StaticSiteGenerator(loader, markdown, runner, themeService, renderer,
                    paths, new FileSystem(), site, NullLogger<StaticSiteGenerator>.Instance);

                // Act
                await generator.BuildAsync<SampleLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                    destination, preview, Xunit.TestContext.Current.CancellationToken);

                // Assert
                site.IsPreview.ShouldBe(preview);
                Directory.GetFiles(destination, "*.html", SearchOption.AllDirectories).Length.ShouldBe(expectedRoutes.Length);
                foreach (var route in expectedRoutes)
                {
                    var path = route == "404.html"
                        ? Path.Combine(destination, route)
                        : Path.Combine(destination, route, "index.html");
                    var html = await File.ReadAllTextAsync(path, Xunit.TestContext.Current.CancellationToken);
                    using var parsed = new HtmlParser().ParseDocument(html);
                    var metadata = parsed.QuerySelectorAll("meta[property^='og:'], meta[name^='twitter:']")
                        .ToDictionary(
                            element => element.GetAttribute("property") ?? element.GetAttribute("name")
                                ?? throw new InvalidOperationException("Metadata must have a name."),
                            element => element.GetAttribute("content"));
                    metadata["og:url"].ShouldBe(route.Length == 0
                        ? $"https://example.com{baseUrl.TrimEnd('/')}"
                        : $"https://example.com{baseUrl}{route}");
                    if (usePlaceholders)
                    {
                        metadata.ShouldBe(components[route]);
                    }
                    else
                    {
                        components[route] = metadata;
                    }
                    if (route.StartsWith("tags", StringComparison.Ordinal))
                    {
                        metadata["og:title"].ShouldBe(site.Title);
                        metadata["og:description"].ShouldBe(site.Description);
                        metadata["og:locale"].ShouldBe(site.Locale);
                        parsed.Title.ShouldBe(site.Title);
                        parsed.DocumentElement.GetAttribute("lang").ShouldBe("ko-kr");
                        parsed.QuerySelector("meta[name='description']")!.GetAttribute("content").ShouldBe(site.Description);
                        parsed.QuerySelectorAll("tools").ShouldBeEmpty();
                    }
                    metadata.ContainsKey("twitter:creator").ShouldBe(route == "ko-kr/post");
                    metadata.ContainsKey("og:image").ShouldBeFalse();
                    parsed.QuerySelectorAll("script[src='https://www.googletagmanager.com/gtag/js?id=G-EXAMPLE']").Length.ShouldBe(1);
                    html.ShouldNotContain("<plugin:");
                }
            }
        }
        finally
        {
            temporary.Delete(recursive: true);
        }
    }
}
