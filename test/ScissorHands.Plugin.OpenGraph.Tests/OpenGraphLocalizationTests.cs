using AngleSharp.Html.Parser;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

using MetadataHost = ScissorHands.Plugin.OpenGraph.Tests.OpenGraphContractTests.MetadataHost;

namespace ScissorHands.Plugin.OpenGraph.Tests;

public class OpenGraphLocalizationTests
{
    private const string Marker = "<plugin:open-graph></plugin:open-graph>";
    private static readonly PluginManifest Plugin = new()
    {
        Id = "open-graph",
        Options = new Dictionary<string, object?> { ["TwitterCreatorId"] = "@author" },
    };

    [Theory]
    [InlineData("", false, false)]
    [InlineData("/blog/", false, false)]
    [InlineData("", true, false)]
    [InlineData("/blog/", true, false)]
    [InlineData("", true, true)]
    [InlineData("/blog/", true, true)]
    public async Task Given_LocalizedDocument_When_Rendered_Then_It_Should_Use_ActualLanguageAndCurrentRouteWithoutChangingThemeSeo(
        string baseUrl, bool additional, bool fallback)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(baseUrl);
        var primaryUrl = $"https://example.com{baseUrl.TrimEnd('/')}/guide%20%25%20%ED%95%9C%EA%B8%80";
        var route = (additional ? "ko-kr/" : "") + "guide % 한글";
        var expectedUrl = additional ? primaryUrl.Replace("/guide", "/ko-kr/guide", StringComparison.Ordinal) : primaryUrl;
        var language = additional && !fallback ? "ko-kr" : "en-us";
        var canonical = fallback ? primaryUrl : expectedUrl;
        var document = Document(route, language);
        var locale = new LocaleContext
        {
            Locale = additional ? "ko-kr" : "en-us",
            ContentLocale = language,
            IsFallback = fallback,
            Route = route,
            CanonicalUrl = canonical,
            AlternateLanguageUrls = new Dictionary<string, string> { ["en-us"] = primaryUrl },
        };
        var seo = $"<link rel=\"canonical\" href=\"{canonical}\" />"
            + $"<link rel=\"alternate\" hreflang=\"en-us\" href=\"{primaryUrl}\" />";

        // Act
        var component = Render(context, site, document, locale);
        var html = await Hook(site, document, seo + Marker);
        var hook = Parse(html);
        var parsed = new HtmlParser().ParseDocument(html);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:locale"].ShouldBe(language);
        hook["og:url"].ShouldBe(expectedUrl);
        hook["og:title"].ShouldBe("Article | Site");
        hook["twitter:creator"].ShouldBe("@author");
        hook.ContainsKey("og:image").ShouldBeFalse();
        component.FindAll("link").ShouldBeEmpty();
        parsed.QuerySelectorAll("link[rel='canonical']").Length.ShouldBe(1);
        parsed.QuerySelector("link[rel='canonical']")!.GetAttribute("href").ShouldBe(canonical);
        parsed.QuerySelectorAll("link[hreflang]").Length.ShouldBe(1);
        html.ShouldContain(seo);
        site.Locales.ShouldBe(["EN_US", "ko-KR"]);
        document.Metadata.Slug.ShouldBe(route);
        document.Metadata.Locale.ShouldBe(language);
    }

    [Theory]
    [InlineData("", "", "en-us", "")]
    [InlineData("/blog/", "", "en-us", "")]
    [InlineData("", "ko-kr", "ko-kr", "/ko-kr")]
    [InlineData("/blog/", "ko-kr", "ko-kr", "/ko-kr")]
    [InlineData("", "tags", "en-us", "/tags")]
    [InlineData("/blog/", "ko-kr/tags", "ko-kr", "/ko-kr/tags")]
    [InlineData("", "tags/c%23", "en-us", "/tags/c%23")]
    [InlineData("/blog/", "ko-kr/tags/c%23", "ko-kr", "/ko-kr/tags/c%23")]
    [InlineData("", "ko-kr/tags/literal%252F", "ko-kr", "/ko-kr/tags/literal%252F")]
    [InlineData("/blog/", "ko-kr/tags/%ED%95%9C%EA%B8%80", "ko-kr", "/ko-kr/tags/%ED%95%9C%EA%B8%80")]
    [InlineData("", "404.html", "en-us", "/404.html")]
    [InlineData("/blog/", "404.html", "en-us", "/404.html")]
    public async Task Given_GeneratedLocalizedContext_When_Rendered_Then_It_Should_Preserve_RoutesAndSiteMetadataWithoutDocumentSeo(
        string baseUrl, string route, string language, string suffix)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(baseUrl);
        var renderingDocument = Document(route, language, generated: true);
        var hookDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = renderingDocument.Metadata with { Title = "Synthetic hook title" },
        };
        var locale = new LocaleContext { Locale = language, ContentLocale = language, Route = route };

        // Act
        var component = Render(context, site, renderingDocument, locale);
        var html = await Hook(site, hookDocument);
        var hook = Parse(html);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:url"].ShouldBe($"https://example.com{baseUrl.TrimEnd('/')}{suffix}");
        hook["og:locale"].ShouldBe(language);
        hook["og:title"].ShouldBe("Site");
        hook["og:description"].ShouldBe("Site description");
        hook.ContainsKey("twitter:creator").ShouldBeFalse();
        hook.ContainsKey("og:image").ShouldBeFalse();
        component.FindAll("link").ShouldBeEmpty();
        new HtmlParser().ParseDocument(html).QuerySelectorAll("link").ShouldBeEmpty();
        locale.CanonicalUrl.ShouldBeNull();
        locale.AlternateLanguageUrls.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Given_DisabledLocaleInventory_When_Rendered_Then_It_Should_Omit_LanguageWithoutTreatingLocaleLookingFoldersAsTranslations(int configuration)
    {
        // Arrange
        using var context = new BunitContext();
        var site = configuration switch
        {
            0 => new SiteManifest { SiteUrl = "https://example.com", HeroImage = "" },
            1 => new SiteManifest { SiteUrl = "https://example.com", HeroImage = "", Locales = null! },
            _ => new SiteManifest { SiteUrl = "https://example.com", HeroImage = "", Locales = [] },
        };
        var document = Document("ko-kr/about", null);

        // Act
        var component = Render(context, site, document, null);
        var hook = Parse(await Hook(site, document));

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook.ContainsKey("og:locale").ShouldBeFalse();
        hook["og:url"].ShouldBe("https://example.com/ko-kr/about");
        site.IsLocalizationEnabled.ShouldBeFalse();
    }

    [Theory]
    [InlineData("", "", "")]
    [InlineData("/blog/", "", "")]
    [InlineData("", "tags/c%23", "/tags/c%23")]
    [InlineData("/blog/", "tags/literal%252F", "/tags/literal%252F")]
    [InlineData("", "404.html", "/404.html")]
    [InlineData("/blog/", "404.html", "/404.html")]
    public async Task Given_GeneratedPagesWithoutLocalization_When_Rendered_Then_It_Should_Preserve_UrlsWithoutInventingLanguage(
        string baseUrl, string route, string suffix)
    {
        // Arrange
        using var context = new BunitContext();
        var site = new SiteManifest { SiteUrl = "https://example.com", BaseUrl = baseUrl, HeroImage = "" };
        var document = Document(route, null, generated: true);

        // Act
        var component = Render(context, site, route.Length == 0 ? null : document, null);
        var hook = Parse(await Hook(site, document));

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook.ContainsKey("og:locale").ShouldBeFalse();
        hook.ContainsKey("twitter:creator").ShouldBeFalse();
        hook["og:title"].ShouldBe(site.Title);
        hook["og:url"].ShouldBe($"https://example.com{baseUrl.TrimEnd('/')}{suffix}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Given_NoActualLanguage_When_Rendered_Then_It_Should_Use_NormalizedFirstDeclaredLocaleNotRequestedLocale(string? language)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site("");
        var document = Document("ko-kr/about", language);
        var locale = new LocaleContext { Locale = "ko-kr", Route = "ko-kr/about", ContentLocale = language };

        // Act
        var component = Render(context, site, document, locale);
        var hook = Parse(await Hook(site, document));

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:locale"].ShouldBe("en-us");
        hook["og:url"].ShouldBe("https://example.com/ko-kr/about");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Given_ThemeNoticesAndPublicationBadges_When_PostHtmlAsync_Then_It_Should_Preserve_OriginalHtmlOutsideMarkers(
        bool preview, bool localized)
    {
        // Arrange
        var site = new SiteManifest
        {
            SiteUrl = "https://example.com",
            Locales = localized ? ["en-us", "ko-kr"] : [],
            IsPreview = preview,
            HeroImage = "",
        };
        var notice = localized
            ? "<section data-localization-fallback lang=\"ko-kr\" role=\"note\">번역 &amp; unavailable</section>"
            : "";
        var badges = preview
            ? "<span data-publication-badge=\"draft\" data-publication-route=\"ko-kr/about\" data-publication-placement=\"detail\">Draft &amp; review</span>"
                + "<span data-publication-badge=\"scheduled\" data-publication-route=\"ko-kr/about\" data-publication-placement=\"detail\" data-publication-date=\"2099-09-01\">Scheduled on 2099-09-01</span>"
            : "";
        var body = "<body><main>" + notice + "<article lang=\"en-us\" data-publication-content=\"ko-kr/about\">"
            + badges + "<p>Original content</p></article></main></body>";
        var html = "<html><head>" + Marker + "</head>" + body + "</html>";

        // Act
        var result = await Hook(site, Document("ko-kr/about", "en-us"), html);

        // Assert
        result.ShouldContain(body);
        result.ShouldNotContain(Marker);
        Parse(result)["og:url"].ShouldBe("https://example.com/ko-kr/about");
        Parse(result).ContainsKey("og:locale").ShouldBe(localized);
        site.IsPreview.ShouldBe(preview);
    }

    [Fact]
    public void Given_ResolvedContextChanges_When_OnParametersSet_Then_It_Should_Refresh_LanguageRouteAndEnablementWithoutStaleValues()
    {
        // Arrange
        using var context = new BunitContext();
        var document = Document("post-hook-route", "post-hook-language");
        var cut = Render(context, Site("/blog/"), document,
            new LocaleContext { Locale = "ko-kr", ContentLocale = "ko-kr", Route = "ko-kr/article %" });

        // Act
        var translated = Parse(cut.Markup);
        cut.Render(p => p.Add(x => x.LocaleContext,
            new LocaleContext { Locale = "ko-kr", ContentLocale = "en-us", IsFallback = true, Route = "ko-kr/article %" }));
        var fallback = Parse(cut.Markup);
        cut.Render(p => p.Add(x => x.Site, Site("/new/")));
        var moved = Parse(cut.Markup);
        cut.Render(p => p.Add(x => x.Plugins, Array.Empty<PluginManifest>()));
        var disabled = cut.Markup;
        cut.Render(p => p.Add(x => x.LocaleContext, (LocaleContext?)null)
            .Add(x => x.Document, (ContentDocument?)null)
            .Add(x => x.Site, new SiteManifest { SiteUrl = "https://other.example", HeroImage = "" }));
        cut.Render(p => p.Add(x => x.Plugins, new[] { Plugin }));
        var restored = Parse(cut.Markup);

        // Assert
        translated["og:locale"].ShouldBe("ko-kr");
        translated["og:url"].ShouldBe("https://example.com/blog/ko-kr/article%20%25");
        fallback["og:locale"].ShouldBe("en-us");
        fallback["og:url"].ShouldBe(translated["og:url"]);
        moved["og:url"].ShouldBe("https://example.com/new/ko-kr/article%20%25");
        disabled.ShouldBeEmpty();
        restored.ContainsKey("og:locale").ShouldBeFalse();
        restored["og:url"].ShouldBe("https://other.example");
        restored.ContainsKey("twitter:creator").ShouldBeFalse();
        cut.Markup.ShouldNotContain("ko-kr");
    }

    [Fact]
    public void Given_LocalizationDisabledWithStaleContext_When_OnParametersSet_Then_It_Should_Clear_LanguageMetadata()
    {
        // Arrange
        using var context = new BunitContext();
        var cut = Render(context, Site(""), Document("about", "ko-kr"),
            new LocaleContext { Locale = "ko-kr", ContentLocale = "ko-kr", Route = "about" });

        // Act
        cut.Render(p => p.Add(x => x.Site, new SiteManifest { SiteUrl = "https://example.com", Locales = [], HeroImage = "" }));

        // Assert
        Parse(cut.Markup).ContainsKey("og:locale").ShouldBeFalse();
    }

    private static SiteManifest Site(string baseUrl) => new()
    {
        SiteUrl = "https://example.com",
        BaseUrl = baseUrl,
        Locales = ["EN_US", "ko-KR"],
        Title = "Site",
        Description = "Site description",
        HeroImage = "",
    };

    private static ContentDocument Document(string route, string? language, bool generated = false) => new()
    {
        Kind = generated ? ContentKind.Page : ContentKind.Post,
        SourcePath = generated ? "" : "/contents/posts/article.md",
        Metadata = new ContentMetadata { Slug = route, Locale = language, Title = "Article" },
    };

    private static IRenderedComponent<MetadataHost> Render(
        BunitContext context, SiteManifest site, ContentDocument? document, LocaleContext? locale)
        => context.Render<MetadataHost>(p => p.Add(x => x.Site, site)
            .Add(x => x.Document, document).Add(x => x.LocaleContext, locale)
            .Add(x => x.Plugins, new[] { Plugin }));

    private static Task<string> Hook(SiteManifest site, ContentDocument document, string html = Marker)
        => new OpenGraphPlugin().PostHtmlAsync(html, document, Plugin, site, Xunit.TestContext.Current.CancellationToken);

    private static Dictionary<string, string> Parse(string html)
        => new HtmlParser().ParseDocument(html).QuerySelectorAll("meta")
            .ToDictionary(meta => meta.GetAttribute("property") ?? meta.GetAttribute("name")!,
                meta => meta.GetAttribute("content") ?? string.Empty, StringComparer.Ordinal);

    private static void AssertParity(Dictionary<string, string> expected, Dictionary<string, string> actual)
    {
        actual.Keys.Order().ShouldBe(expected.Keys.Order());
        foreach (var (key, value) in expected)
        {
            actual[key].ShouldBe(value, key);
        }
    }
}
