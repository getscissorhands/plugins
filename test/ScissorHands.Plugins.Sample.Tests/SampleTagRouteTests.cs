using System.IO.Abstractions;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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
using ScissorHands.Web.Services;

namespace ScissorHands.Plugins.Sample.Tests;

public class SampleTagRouteTests
{
    [Theory]
    [InlineData("/", false, false)]
    [InlineData("/", true, false)]
    [InlineData("/blog/", false, false)]
    [InlineData("/blog/", true, false)]
    [InlineData("/", false, true)]
    [InlineData("/", true, true)]
    [InlineData("/blog/", false, true)]
    [InlineData("/blog/", true, true)]
    public async Task Given_ReleasedLoaderAndGenerator_When_BuildingSample_Then_It_Should_PreserveLocalePublicationAndPluginContracts(
        string baseUrl, bool preview, bool localized)
    {
        // Arrange
        var workspace = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"sample-integration-{Guid.NewGuid():N}"));
        try
        {
            var contents = Path.Combine(workspace.FullName, "contents");
            foreach (var source in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "SampleContents"), "*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(contents, Path.GetRelativePath(Path.Combine(AppContext.BaseDirectory, "SampleContents"), source));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(source, target);
            }
            foreach (var (relativePath, markdown) in AdditionalFixtures)
            {
                var target = Path.Combine(contents, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await File.WriteAllTextAsync(target, markdown, Xunit.TestContext.Current.CancellationToken);
            }
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
                .Build();
            var originalSite = configuration.GetSection("Site").Get<SiteManifest>()!;
            var site = new SiteManifest
            {
                Title = originalSite.Title,
                Description = originalSite.Description,
                Locales = localized ? originalSite.Locales : [],
                SiteUrl = "https://example.com",
                BaseUrl = baseUrl,
                Theme = "default",
                UseDateInPostUrl = false,
                TimeZone = "UTC",
            };
            var paths = Substitute.For<IAppPaths>();
            paths.BasePath.Returns(workspace.FullName);
            paths.GetContentsRoot().Returns(contents);
            paths.GetThemesRoot().Returns(Path.Combine(workspace.FullName, "themes"));
            var fileSystem = new FileSystem();
            var loader = new ContentLoader(paths, fileSystem, site, NullLogger<ContentLoader>.Instance);
            var configuredTheme = configuration.GetSection("Theme").Get<ThemeSettings>()!;
            var applicationTheme = new ThemeSettings { Localization = configuredTheme.Localization };
            var themeService = new ThemeService(paths, fileSystem, NullLogger<ThemeService>.Instance);
            var theme = await themeService.LoadManifestAsync("default", Xunit.TestContext.Current.CancellationToken);
            var manifests = configuration.GetSection("Plugins").Get<PluginManifest[]>()!
                .Append(new PluginManifest { Id = "sample-probe" }).ToArray();
            var primaryRoutes = new List<string>
            {
                "", "welcome", "fallbacks", "about", "escaped", "tags", "tags/plugins", "tags/preview",
                "tags/c%23%20%2F%20%3Ctools%3E", "tags/100%25", "tags/literal%252f", "tags/%ED%95%9C%EA%B8%80",
            };
            if (preview)
            {
                primaryRoutes.AddRange(["draft-post", "scheduled-post", "draft-page", "tags/publication-examples"]);
            }
            var expectedRoutes = primaryRoutes.Concat(localized
                    ? primaryRoutes.Select(route => $"ko-kr/{route}".TrimEnd('/'))
                    : ["ko-kr/about"])
                .Append("404.html").ToArray();
            var componentMetadata = new Dictionary<string, Dictionary<string, string?>>();

            foreach (var usePlaceholders in new[] { false, true })
            {
                var destination = Path.Combine(workspace.FullName, usePlaceholders ? "hooks" : "components");
                var observations = new CollectionObservations();
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddSingleton<IThemeService>(themeService);
                services.AddSingleton(applicationTheme);
                services.AddSingleton(observations);
                services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Sample:UsePlaceholders"] = usePlaceholders.ToString(),
                    }).Build());
                using var provider = services.BuildServiceProvider();
                var renderer = new ComponentRenderer(provider.GetRequiredService<IServiceScopeFactory>(),
                    provider.GetRequiredService<ILoggerFactory>());
                var probe = new CollectionProbePlugin();
                var runner = new PluginRunner(manifests,
                    new IContentPlugin[] { new OpenGraphPlugin(applicationTheme), new GoogleAnalyticsPlugin(), probe }, site);
                var generator = new StaticSiteGenerator(loader, new MarkdownService(), runner, themeService,
                    renderer, paths, fileSystem, site, NullLogger<StaticSiteGenerator>.Instance, new SampleClock(), applicationTheme);

                // Act
                await generator.BuildAsync<SampleLayout, ProbeIndexView, PostView, PageView, NotFoundView, ProbeTagListView, ProbeTagView>(
                    destination, preview, Xunit.TestContext.Current.CancellationToken);

                // Assert
                site.IsPreview.ShouldBe(preview);
                site.Locales.ShouldBe(localized ? originalSite.Locales : []);
                Directory.GetFiles(destination, "*.html", SearchOption.AllDirectories).Length.ShouldBe(expectedRoutes.Length);
                foreach (var route in expectedRoutes)
                {
                    var path = route == "404.html" ? Path.Combine(destination, route) : Path.Combine(destination, route, "index.html");
                    var html = await File.ReadAllTextAsync(path, Xunit.TestContext.Current.CancellationToken);
                    using var parsed = new HtmlParser().ParseDocument(html);
                    var metadata = parsed.QuerySelectorAll("meta[property^='og:'], meta[name^='twitter:']")
                        .ToDictionary(element => element.GetAttribute("property") ?? element.GetAttribute("name")!,
                            element => element.GetAttribute("content"));
                    var secondary = localized && route.StartsWith("ko-kr", StringComparison.Ordinal);
                    var relative = secondary ? route["ko-kr".Length..].TrimStart('/') : route;
                    var collection = relative.Length == 0 || relative == "tags" || relative.StartsWith("tags/", StringComparison.Ordinal);
                    var fallback = secondary && !collection && relative != "about";
                    var canonicalRoute = fallback ? relative : route;
                    metadata["og:url"].ShouldBe(Absolute(baseUrl, route));
                    if (localized)
                    {
                        metadata["og:locale"].ShouldBe(secondary && !fallback ? "ko-kr" : "en-us");
                    }
                    else
                    {
                        metadata.ContainsKey("og:locale").ShouldBeFalse();
                    }
                    parsed.DocumentElement.GetAttribute("lang").ShouldBe(localized ? secondary ? "ko-kr" : "en-us" : null);
                    if (!collection && route != "404.html")
                    {
                        parsed.QuerySelector("article")!.GetAttribute("lang")
                            .ShouldBe(localized ? secondary && !fallback ? "ko-kr" : "en-us" : null);
                    }
                    var canonical = parsed.QuerySelectorAll("link[rel='canonical']");
                    canonical.Length.ShouldBe(localized && !collection && route != "404.html" ? 1 : 0);
                    if (canonical.Length == 1)
                    {
                        canonical[0].GetAttribute("href").ShouldBe(Absolute(baseUrl, canonicalRoute) + "/");
                        var alternatives = parsed.QuerySelectorAll("link[rel='alternate'][hreflang]");
                        alternatives.Length.ShouldBe(relative == "about" ? 2 : 1);
                        alternatives.Single(element => element.GetAttribute("hreflang") == "en-us")
                            .GetAttribute("href").ShouldBe(Absolute(baseUrl, relative) + "/");
                        if (relative == "about")
                        {
                            alternatives.Single(element => element.GetAttribute("hreflang") == "ko-kr")
                                .GetAttribute("href").ShouldBe(Absolute(baseUrl, "ko-kr/about") + "/");
                        }
                    }
                    else
                    {
                        parsed.QuerySelectorAll("link[rel='alternate'][hreflang]").ShouldBeEmpty();
                    }
                    var notices = parsed.QuerySelectorAll("[data-localization-fallback]");
                    notices.Length.ShouldBe(fallback ? 1 : 0);
                    if (fallback)
                    {
                        notices[0].TextContent.ShouldBe(applicationTheme.Localization["ko-kr"]!.TranslationUnavailable);
                        notices[0].GetAttribute("lang").ShouldBe("ko-kr");
                    }
                    AssertPublication(parsed, relative, secondary, preview);
                    if (collection)
                    {
                        metadata["og:title"].ShouldBe(site.Title);
                        metadata["og:description"].ShouldBe(site.Description);
                        parsed.QuerySelectorAll("tools").ShouldBeEmpty();
                    }
                    if (relative == "about")
                    {
                        metadata["og:title"].ShouldBe($"{(secondary ? "플러그인 샘플 소개" : "About the plugin sample")} | {site.Title}");
                    }
                    metadata.ContainsKey("twitter:creator").ShouldBe(!collection && relative is "welcome" or "fallbacks" or "escaped" or "draft-post" or "scheduled-post");
                    if (metadata.TryGetValue("twitter:creator", out var creator))
                    {
                        creator.ShouldBe(relative == "welcome" ? "@post-author" : "@sample-author");
                    }
                    metadata.ContainsKey("og:image").ShouldBe(relative == "welcome");
                    parsed.QuerySelectorAll("script[src='https://www.googletagmanager.com/gtag/js?id=G-EXAMPLE']").Length.ShouldBe(1);
                    parsed.QuerySelector(".site-title")!.GetAttribute("href").ShouldBe(secondary ? "ko-kr/" : ".");
                    parsed.QuerySelector("base")!.GetAttribute("href").ShouldBe(baseUrl);
                    parsed.QuerySelector("link[rel='stylesheet']")!.GetAttribute("href")
                        .ShouldBe($"themes/default/{theme.Stylesheets.Single().TrimStart('/')}");
                    parsed.QuerySelector("script[src^='themes/default/']")!.GetAttribute("src")
                        .ShouldBe($"themes/default/{theme.Scripts.Single().TrimStart('/')}");
                    html.ShouldNotContain("<plugin:");
                    html.ShouldNotContain("en-us/");
                    html.ShouldNotContain("ko-kr/ko-kr/");
                    if (usePlaceholders)
                    {
                        metadata.ShouldBe(componentMetadata[route]);
                    }
                    else
                    {
                        componentMetadata[route] = metadata;
                    }
                }
                var observed = observations.Documents.Select(document => document.Metadata.Slug).ToHashSet();
                observed.ShouldContain("welcome");
                foreach (var route in new[] { "draft-post", "scheduled-post", "draft-page" })
                {
                    observed.Contains(route).ShouldBe(preview);
                    probe.Routes.Contains(route).ShouldBe(preview);
                    if (localized)
                    {
                        observed.Contains($"ko-kr/{route}").ShouldBe(preview);
                        probe.Routes.Contains($"ko-kr/{route}").ShouldBe(preview);
                    }
                }
                observations.Kinds.ShouldContain("home");
                observations.Kinds.ShouldContain("tags");
                observations.Kinds.ShouldContain("tag");
                foreach (var document in observations.Documents)
                {
                    document.PublicationStatus.IsDraft.ShouldBe(preview && document.Metadata.Slug.Contains("draft-", StringComparison.Ordinal));
                    document.PublicationStatus.IsScheduled.ShouldBe(preview && document.Metadata.Slug.Contains("scheduled-post", StringComparison.Ordinal));
                }
                foreach (var asset in theme.Stylesheets.Concat(theme.Scripts)
                    .Select(path => path.TrimStart('/'))
                    .Append(Path.Combine("assets", "THIRD-PARTY-NOTICES.md")))
                {
                    File.ReadAllBytes(Path.Combine(destination, "themes", "default", asset))
                        .ShouldBe(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "themes", "default", asset)));
                }
                var bundledIcons = Path.Combine(AppContext.BaseDirectory, "themes", "default", "assets", "images", "icons");
                foreach (var icon in Directory.GetFiles(bundledIcons, "*.svg"))
                {
                    File.ReadAllBytes(Path.Combine(destination, "themes", "default", "assets", "images", "icons", Path.GetFileName(icon)))
                        .ShouldBe(File.ReadAllBytes(icon));
                }
                File.Exists(Path.Combine(destination, "images", "sample.svg")).ShouldBeTrue();
            }
        }
        finally
        {
            workspace.Delete(recursive: true);
        }
    }

    private static string Absolute(string baseUrl, string route) =>
        route.Length == 0 ? $"https://example.com{baseUrl.TrimEnd('/')}" : $"https://example.com{baseUrl}{route}";

    private static readonly IReadOnlyDictionary<string, string> AdditionalFixtures = new Dictionary<string, string>
    {
        ["pages/ko-kr/about.md"] = """
            ---
            title: 플러그인 샘플 소개
            slug: ko-kr/about
            description: 번역된 페이지에도 작성자 메타데이터는 표시하지 않습니다.
            show_in_navigation: true
            tags: [preview]
            ---
            # 플러그인 샘플 소개
            [첫 글](welcome)은 영어 원문과 한국어 번역 안내를 표시합니다.
            """,
        ["posts/draft-post.md"] = """
            ---
            title: Draft plugin example
            slug: draft-post
            published: 2026-09-14
            draft: true
            tags: [publication-examples]
            ---
            # Draft plugin example
            Preview includes this post and its listing badges.
            """,
        ["posts/scheduled-post.md"] = """
            ---
            title: Scheduled plugin example
            slug: scheduled-post
            published: 2099-12-31
            tags: [publication-examples]
            ---
            # Scheduled plugin example
            Production withholds this post until its UTC publication instant.
            """,
        ["pages/draft-page.md"] = """
            ---
            title: Draft plugin page
            slug: draft-page
            draft: true
            show_in_navigation: true
            tags: [publication-examples]
            ---
            # Draft plugin page
            Pages have draft badges, never scheduled badges.
            """,
        ["posts/escaped.md"] = """
            ---
            title: Escaped tags
            slug: escaped
            published: 2026-09-14
            tags: ["C# / <Tools>", "100%", "literal%2F", "한글"]
            ---
            # Escaped tags
            """,
    };

    private static void AssertPublication(IDocument parsed, string route, bool secondary, bool preview)
    {
        var badges = parsed.QuerySelectorAll("[data-publication-badge]");
        if (!preview)
        {
            badges.ShouldBeEmpty();
            parsed.QuerySelectorAll("a[href*='draft-post'], a[href*='scheduled-post'], a[href*='draft-page']").ShouldBeEmpty();
            return;
        }
        var detailKind = route is "draft-post" or "draft-page" ? "draft" : route == "scheduled-post" ? "scheduled" : null;
        var listing = route.Length == 0 || route == "tags/publication-examples";
        badges.Length.ShouldBe(detailKind is not null ? 1 : listing ? route.Length == 0 ? 2 : 3 : 0);
        foreach (var badge in badges)
        {
            var scheduled = badge.GetAttribute("data-publication-badge") == "scheduled";
            badge.TextContent.ShouldBe(scheduled
                ? secondary ? "2099-12-31 공개 예정" : "Scheduled on 2099-12-31"
                : secondary ? "초안" : "Draft");
            badge.GetAttribute("data-publication-placement").ShouldBe(detailKind is null ? "listing" : "detail");
            badge.Closest("main").ShouldNotBeNull();
        }
    }

    private sealed class SampleClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);
    }

    public sealed class CollectionObservations
    {
        public List<ContentDocument> Documents { get; } = [];
        public HashSet<string> Kinds { get; } = [];
    }

    public sealed class CollectionProbeComponent : PluginComponentBase
    {
        [Inject]
        public CollectionObservations Observations { get; set; } = null!;

        [CascadingParameter(Name = "TaggedDocuments")]
        public IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>? TaggedDocuments { get; set; }

        [CascadingParameter(Name = "TaggedPosts")]
        public IEnumerable<ContentDocument>? TaggedPosts { get; set; }

        [CascadingParameter(Name = "TaggedPages")]
        public IEnumerable<ContentDocument>? TaggedPages { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Plugin.ShouldNotBeNull();
            if (Documents is not null)
            {
                Observations.Kinds.Add("home");
                Observations.Documents.AddRange(Documents);
            }
            if (TaggedDocuments is not null)
            {
                Observations.Kinds.Add("tags");
                Observations.Documents.AddRange(TaggedDocuments.Values.SelectMany(group => group.Posts.Concat(group.Pages)));
            }
            if (TaggedPosts is not null || TaggedPages is not null)
            {
                Observations.Kinds.Add("tag");
                Observations.Documents.AddRange((TaggedPosts ?? []).Concat(TaggedPages ?? []));
            }
        }
    }

    public sealed class ProbeIndexView : IndexView
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            RenderProbe(builder);
        }
    }

    public sealed class ProbeTagListView : TagListView
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            RenderProbe(builder);
        }
    }

    public sealed class ProbeTagView : TagView
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            RenderProbe(builder);
        }
    }

    private static void RenderProbe(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CollectionProbeComponent>(1000);
        builder.AddAttribute(1001, nameof(CollectionProbeComponent.Id), "sample-probe");
        builder.CloseComponent();
    }

    private sealed class CollectionProbePlugin : ContentPlugin
    {
        public override string Id => "sample-probe";
        public override string Name => "Sample integration observer";
        public HashSet<string> Routes { get; } = [];

        public override Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest plugin,
            SiteManifest site, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Routes.Add(document.Metadata.Slug);
            return Task.FromResult(document);
        }
    }
}
