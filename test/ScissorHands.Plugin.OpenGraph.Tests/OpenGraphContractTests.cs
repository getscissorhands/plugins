using System.Collections.ObjectModel;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph.Tests;

public class OpenGraphContractTests
{
    private const string Marker = "<plugin:open-graph></plugin:open-graph>";

    [Theory]
    [InlineData(ContentKind.Post, "/post.md", null, true, true)]
    [InlineData(ContentKind.Post, "", null, false, false)]
    [InlineData(ContentKind.Post, " \t ", null, false, false)]
    [InlineData(ContentKind.Page, "/about.md", null, true, false)]
    [InlineData(ContentKind.Post, "", "collection", false, false)]
    public async Task Given_EquivalentContexts_When_Rendered_Then_It_Should_Have_MetadataParity(
        ContentKind kind, string sourcePath, string? collection, bool contentMetadata, bool creator)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site();
        var document = Document(kind: kind, sourcePath: sourcePath, handle: "@document");
        var plugin = Manifest();
        IEnumerable<ContentDocument>? documents = collection is null ? null : [];

        // Act
        var hook = await Hook(site, document, plugin);
        var component = Render(context, site, document, plugin, documents);
        var metadata = Parse(hook);

        // Assert
        AssertParity(metadata, Parse(component.Markup));
        metadata["og:title"].ShouldBe(contentMetadata ? "Document | Site" : "Site");
        metadata["og:description"].ShouldBe(contentMetadata ? "Document description" : "Site description");
        metadata.ContainsKey("twitter:creator").ShouldBe(creator);
        if (creator)
        {
            metadata["twitter:creator"].ShouldBe("@document");
        }
    }

    [Fact]
    public async Task Given_NoDocument_When_Rendered_Then_It_Should_Use_SiteContextInBothPaths()
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site();
        var plugin = Manifest();

        // Act
        var hook = Parse(await Hook(site, null, plugin));
        var component = Render(context, site, null, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:title"].ShouldBe("Site");
        hook["og:url"].ShouldBe("https://example.com/blog");
        hook.ContainsKey("twitter:creator").ShouldBeFalse();
    }

    [Theory]
    [InlineData(null, "Site description")]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("Document description", "Document description")]
    public async Task Given_Description_When_Rendered_Then_It_Should_Preserve_NullOnlyFallback(string? description, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        var document = Document(description: description);
        var site = Site();
        var plugin = Manifest();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:description"].ShouldBe(expected);
        hook["twitter:description"].ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", "\t")]
    [InlineData(123, false)]
    [InlineData("@site", "@creator")]
    public async Task Given_ReadOnlyOptions_When_Rendered_Then_It_Should_Respect_OptionalValuesWithoutMutation(object? siteId, object? creatorId)
    {
        // Arrange
        using var context = new BunitContext();
        var nested = new Dictionary<string, object?> { ["unchanged"] = "value" };
        var options = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>
        {
            ["TwitterSiteId"] = siteId,
            ["TwitterCreatorId"] = creatorId,
            ["Other"] = nested,
        });
        var plugin = new PluginManifest { Id = "open-graph", Options = options };
        var snapshot = plugin.Options!.ToArray();
        var site = Site();
        var document = Document();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook.ContainsKey("twitter:site").ShouldBe(siteId is string text && !string.IsNullOrWhiteSpace(text));
        hook.ContainsKey("twitter:creator").ShouldBe(creatorId is string handle && !string.IsNullOrWhiteSpace(handle));
        plugin.Options.ToArray().ShouldBe(snapshot);
        options.Count.ShouldBe(3);
        nested.Count.ShouldBe(1);
        nested["unchanged"].ShouldBe("value");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_NullOrMissingOptions_When_Rendered_Then_It_Should_Omit_OptionalTwitterIds(bool nullOptions)
    {
        // Arrange
        using var context = new BunitContext();
        var plugin = new PluginManifest
        {
            Id = "open-graph",
            Options = nullOptions ? null : new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>()),
        };
        var site = Site();
        var document = Document();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook.ContainsKey("twitter:site").ShouldBeFalse();
        hook.ContainsKey("twitter:creator").ShouldBeFalse();
        hook["twitter:card"].ShouldBe("summary_large_image");
    }

    [Fact]
    public async Task Given_SourceOptionsChangedAfterManifestCreation_When_Rendered_Then_It_Should_Use_TheReadOnlySnapshot()
    {
        // Arrange
        using var context = new BunitContext();
        var options = new Dictionary<string, object?> { ["TwitterSiteId"] = "@original", ["TwitterCreatorId"] = "@original-author" };
        var plugin = new PluginManifest { Id = "open-graph", Options = options };
        options["TwitterSiteId"] = "@changed";
        options.Clear();
        var site = Site();
        var document = Document();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["twitter:site"].ShouldBe("@original");
        hook["twitter:creator"].ShouldBe("@original-author");
        plugin.Options!.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public async Task Given_AbsentImages_When_Rendered_Then_It_Should_Omit_BothImageTagsOnly(string? image)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(image: image);
        var document = Document(image: image);
        var plugin = Manifest();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);
        var helper = OpenGraphPluginHelper.GetHeroImageUrl(document, site);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        helper.ShouldBeEmpty();
        hook.ContainsKey("og:image").ShouldBeFalse();
        hook.ContainsKey("twitter:image").ShouldBeFalse();
        hook["twitter:card"].ShouldBe("summary_large_image");
        hook["twitter:creator"].ShouldBe("@creator");
        hook.Count.ShouldBe(11);
    }

    [Theory]
    [InlineData("/images/picture.png", "https://example.com/blog/images/picture.png")]
    [InlineData(@" images\picture.png ", "https://example.com/blog/images/picture.png")]
    [InlineData("/images/", "https://example.com/blog/images/")]
    [InlineData("/", "https://example.com/blog/")]
    [InlineData("/images/p%20a%2Fb.png?v=a%2fb&size=2#preview", "https://example.com/blog/images/p%20a%2Fb.png?v=a%2fb&size=2#preview")]
    [InlineData("/images/a b.png", "https://example.com/blog/images/a b.png")]
    [InlineData("/images/한글.png?title=日本語#français", "https://example.com/blog/images/한글.png?title=日本語#français")]
    [InlineData("/images/a.png?q=%22%3Cscript%3E&template={{CONTENT_TITLE}}#preview", "https://example.com/blog/images/a.png?q=%22%3Cscript%3E&template={{CONTENT_TITLE}}#preview")]
    [InlineData("images/?x=1&y=%26#part", "https://example.com/blog/images/?x=1&y=%26#part")]
    [InlineData("https://cdn.example.com:8443/p%20a%2fb/?v=%2F&size=2#preview", "https://cdn.example.com:8443/p%20a%2fb/?v=%2F&size=2#preview")]
    [InlineData("http://cdn.example.com/image.png", "http://cdn.example.com/image.png")]
    [InlineData("HTTPS://cdn.example.com/image.png", "HTTPS://cdn.example.com/image.png")]
    public async Task Given_SupportedImage_When_Rendered_Then_It_Should_Preserve_ReferenceMeaning(string image, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site();
        var document = Document(image: image);
        var plugin = Manifest();

        // Act
        var helper = OpenGraphPluginHelper.GetHeroImageUrl(document, site);
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        helper.ShouldBe(expected);
        AssertParity(hook, Parse(component.Markup));
        hook["og:image"].ShouldBe(expected);
        hook["twitter:image"].ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("example.com")]
    [InlineData("/relative")]
    [InlineData("//example.com")]
    [InlineData("https://")]
    [InlineData("https:///example.com")]
    [InlineData("http:example.com")]
    [InlineData(@"https:\\example.com")]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:payload")]
    [InlineData("data:text/html,payload")]
    [InlineData("https://exa mple.com")]
    [InlineData("https://example.com/\nprivate")]
    [InlineData("https://example.com/%invalid")]
    [InlineData("https://example.com/?query=invalid-for-publication-root")]
    [InlineData("https://example.com/#fragment")]
    public async Task Given_InvalidSiteUrl_When_Rendered_Then_It_Should_Fail_ExplicitlyAcrossSurfaces(string? siteUrl)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(siteUrl: siteUrl);
        var document = Document();
        var plugin = Manifest();

        // Act
        Action content = () => OpenGraphPluginHelper.GetContentUrl(document, site);
        Action image = () => OpenGraphPluginHelper.GetHeroImageUrl(document, site);
        Func<Task> hook = () => Hook(site, document, plugin);
        Action component = () => Render(context, site, document, plugin);

        // Assert
        content.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.SiteUrl");
        image.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.SiteUrl");
        (await hook.ShouldThrowAsync<ArgumentException>()).Message.ShouldContain("Site.SiteUrl");
        component.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.SiteUrl");
    }

    [Theory]
    [InlineData("//private.example")]
    [InlineData(@"\\private.example")]
    [InlineData("https://private.example")]
    [InlineData("/blog?query=1")]
    [InlineData("/blog#fragment")]
    [InlineData("/blog%invalid")]
    [InlineData("\t/blog/")]
    public async Task Given_InvalidBaseUrl_When_Rendered_Then_It_Should_Reject_CorruptedPublicationContext(string baseUrl)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(baseUrl: baseUrl);
        var document = Document();
        var plugin = Manifest();

        // Act
        Action content = () => OpenGraphPluginHelper.GetContentUrl(document, site);
        Action image = () => OpenGraphPluginHelper.GetHeroImageUrl(document, site);
        Func<Task> hook = () => Hook(site, document, plugin);
        Action component = () => Render(context, site, document, plugin);

        // Assert
        content.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.BaseUrl");
        image.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.BaseUrl");
        (await hook.ShouldThrowAsync<ArgumentException>()).Message.ShouldContain("Site.BaseUrl");
        component.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.BaseUrl");
    }

    [Fact]
    public async Task Given_MissingSite_When_Rendered_Then_It_Should_Fail_OnlyWhenConfigured()
    {
        // Arrange
        using var context = new BunitContext();
        var document = Document();
        var plugin = Manifest();

        // Act
        var absent = context.Render<MetadataHost>();
        Func<Task> hook = () => Hook(null, document, plugin);
        Action configured = () => Render(context, null, document, plugin);

        // Assert
        absent.Markup.ShouldBeEmpty();
        (await hook.ShouldThrowAsync<ArgumentException>()).Message.ShouldContain("Site");
        configured.ShouldThrow<ArgumentException>().Message.ShouldContain("Site");
    }

    [Theory]
    [InlineData("javascript:payload")]
    [InlineData("JaVaScRiPt:payload")]
    [InlineData("data:image/png;base64,payload")]
    [InlineData("file:///private/image.png")]
    [InlineData("ftp://example.com/image.png")]
    [InlineData("mailto:private@example.com")]
    [InlineData("urn:private:image")]
    [InlineData("//private.example/image.png")]
    [InlineData(@"\\private.example\image.png")]
    [InlineData(@"/\private.example/image.png")]
    [InlineData(@"\/private.example/image.png")]
    [InlineData(" //private.example/image.png ")]
    [InlineData("https:/private.example/image.png")]
    [InlineData("https:private.example/image.png")]
    [InlineData("https:///private.example/image.png")]
    [InlineData("https://")]
    [InlineData(@"https:\\private.example\image.png")]
    [InlineData("https://private.example\\image.png")]
    [InlineData("http://[invalid/image.png")]
    [InlineData("javascript\t:payload")]
    [InlineData("\nhttps://private.example/image.png")]
    [InlineData("/images/line\nbreak.png")]
    [InlineData("/images/a%2.png")]
    [InlineData("/images/a%GG.png")]
    [InlineData("/images/a%.png")]
    [InlineData("https://private.example/a%GG.png")]
    [InlineData("bad scheme:payload")]
    public async Task Given_UnsupportedImage_When_Rendered_Then_It_Should_Reject_WithoutLeakingInput(string image)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site();
        var document = Document(image: image);
        var plugin = Manifest();

        // Act
        Action helper = () => OpenGraphPluginHelper.GetHeroImageUrl(document, site);
        Func<Task> hook = () => Hook(site, document, plugin);
        Action component = () => Render(context, site, document, plugin);

        // Assert
        var exception = helper.ShouldThrow<ArgumentException>();
        exception.Message.ShouldContain("Document.Metadata.HeroImage");
        exception.Message.ShouldNotContain(image);
        (await hook.ShouldThrowAsync<ArgumentException>()).Message.ShouldBe(exception.Message);
        component.ShouldThrow<ArgumentException>().Message.ShouldBe(exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public async Task Given_AbsentDocumentImage_When_Rendered_Then_It_Should_Use_SiteImage(string? image)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(image: "https://cdn.example.com/site.png?x=%2f#preview");
        var document = Document(image: image);
        var plugin = Manifest();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:image"].ShouldBe(site.HeroImage);
        OpenGraphPluginHelper.GetHeroImageUrl(document, site).ShouldBe(site.HeroImage);
    }

    [Fact]
    public async Task Given_ValidDocumentImageAndInvalidUnusedFallback_When_Rendered_Then_It_Should_Use_OnlySelectedImage()
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(image: "javascript:unused");
        var document = Document();
        var plugin = Manifest();

        // Act
        var helper = OpenGraphPluginHelper.GetHeroImageUrl(document, site);
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:image"].ShouldBe("https://example.com/blog/images/post.png");
        helper.ShouldBe(hook["og:image"]);
    }

    [Fact]
    public async Task Given_InvalidFallbackImage_When_Rendered_Then_It_Should_Identify_SiteImageContext()
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(image: "//private.example/site.png");
        var document = Document(image: null);
        var plugin = Manifest();

        // Act
        Action helper = () => OpenGraphPluginHelper.GetHeroImageUrl(document, site);
        Func<Task> hook = () => Hook(site, document, plugin);
        Action component = () => Render(context, site, document, plugin);

        // Assert
        helper.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.HeroImage");
        (await hook.ShouldThrowAsync<ArgumentException>()).Message.ShouldContain("Site.HeroImage");
        component.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.HeroImage");
    }

    [Theory]
    [InlineData("https://example.com", "", "/", "https://example.com")]
    [InlineData("http://example.com/", "", "", "http://example.com")]
    [InlineData("http://example.com/", "/blog/", "/", "http://example.com/blog")]
    [InlineData("https://example.com/prefix/", "/blog/", "/", "https://example.com/prefix/blog")]
    [InlineData("https://example.com", "/blog/", @" guides\about & team/ ", "https://example.com/blog/guides/about%20%26%20team")]
    [InlineData("https://example.com", "/blog/", "/p%20a/?q#f", "https://example.com/blog/p%2520a/%3Fq%23f")]
    public async Task Given_PublicationContext_When_Rendered_Then_It_Should_Preserve_ContentHelperSemantics(
        string origin, string baseUrl, string slug, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(siteUrl: origin, baseUrl: baseUrl);
        var document = Document(slug: slug);
        var plugin = Manifest();

        // Act
        var helper = OpenGraphPluginHelper.GetContentUrl(document, site);
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        helper.ShouldBe(expected);
        AssertParity(hook, Parse(component.Markup));
        hook["og:url"].ShouldBe(expected);
    }

    [Theory]
    [InlineData("https://example.com", "", "/images/", "https://example.com/images/")]
    [InlineData("http://example.com/", "", "/images/", "http://example.com/images/")]
    [InlineData("https://example.com/prefix/", "/blog/", "/images/", "https://example.com/prefix/blog/images/")]
    [InlineData("https://example.com", @"\blog\sub\", "/images/", "https://example.com/blog/sub/images/")]
    public async Task Given_ImageInRootOrSubpathSite_When_Rendered_Then_It_Should_Retain_ThePublicationPrefix(
        string origin, string baseUrl, string image, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(siteUrl: origin, baseUrl: baseUrl);
        var document = Document(image: image);
        var plugin = Manifest();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:image"].ShouldBe(expected);
        OpenGraphPluginHelper.GetHeroImageUrl(document, site).ShouldBe(expected);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("folder/../private")]
    [InlineData(@"folder\.\private")]
    public async Task Given_DotTraversalSlug_When_Rendered_Then_It_Should_Propagate_ExplicitFailure(string slug)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site();
        var document = Document(slug: slug);
        var plugin = Manifest();

        // Act
        Func<Task> hook = () => Hook(site, document, plugin);
        Action component = () => Render(context, site, document, plugin);

        // Assert
        await hook.ShouldThrowAsync<ArgumentException>();
        component.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("\" ' & <script>alert('synthetic')</script> >", true)]
    [InlineData("{{CONTENT_TITLE}} {{CONTENT_DESCRIPTION}} {{SITE_NAME}} {{TWITTER_CARD_CREATOR}}", true)]
    [InlineData("<plugin:open-graph></plugin:open-graph> &amp; &#34;", true)]
    [InlineData("\" ' & <script>alert('synthetic')</script> >", false)]
    [InlineData("{{CONTENT_TITLE}} {{CONTENT_DESCRIPTION}} {{SITE_NAME}} {{TWITTER_CARD_CREATOR}}", false)]
    [InlineData("<plugin:open-graph></plugin:open-graph> &amp; &#34;", false)]
    public async Task Given_MetadataShapedLikeHtmlOrTemplates_When_Rendered_Then_It_Should_Remain_EncodedData(string text, bool documentAuthor)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(title: text, description: text, locale: text);
        var document = Document(title: text, description: text, handle: documentAuthor ? text : null);
        var plugin = Manifest(siteId: text, creatorId: documentAuthor ? "unused" : text);

        // Act
        var html = await Hook(site, document, plugin);
        var component = Render(context, site, document, plugin);
        var metadata = Parse(html);
        var parsed = new HtmlParser().ParseDocument(html);

        // Assert
        AssertParity(metadata, Parse(component.Markup));
        metadata["og:title"].ShouldBe($"{text} | {text}");
        metadata["og:description"].ShouldBe(text);
        metadata["og:site_name"].ShouldBe(text);
        metadata["og:locale"].ShouldBe(text);
        metadata["twitter:site"].ShouldBe(text);
        metadata["twitter:creator"].ShouldBe(text);
        parsed.QuerySelectorAll("script, plugin\\:open-graph").ShouldBeEmpty();
        parsed.QuerySelectorAll("meta").ShouldAllBe(meta => meta.Attributes.Length == 2);
        component.FindAll("script").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("<plugin:open-graph />")]
    [InlineData("<plugin:open-graph></plugin:open-graph >")]
    [InlineData("<p title=\"unsafe & raw\">{{CONTENT_TITLE}}</p><script>unchanged()</script>")]
    public async Task Given_NoPairedMarker_When_PostHtmlAsync_Then_It_Should_Leave_AllOriginalHtmlUnchanged(string html)
    {
        // Arrange
        var plugin = new OpenGraphPlugin();

        // Act
        var result = await plugin.PostHtmlAsync(html, Document(), Manifest(), Site(), Xunit.TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(html);
    }

    [Fact]
    public async Task Given_InvalidContextWithoutMarkers_When_PostHtmlAsync_Then_It_Should_Not_HideConfigurationFailure()
    {
        // Arrange
        var site = Site(siteUrl: "");

        // Act
        Func<Task> act = () => new OpenGraphPlugin().PostHtmlAsync(
            "<p>unchanged for valid inputs</p>", Document(), Manifest(), site, Xunit.TestContext.Current.CancellationToken);

        // Assert
        (await act.ShouldThrowAsync<ArgumentException>()).Message.ShouldContain("Site.SiteUrl");
    }
    [Fact]
    public async Task Given_MixedCaseRepeatedMarkersAndExistingMetadata_When_PostHtmlAsync_Then_It_Should_Replace_AllWithoutDeduplication()
    {
        // Arrange
        var html = "<meta property=\"og:title\" content=\"existing\">" + Marker
            + "<PLUGIN:OPEN-GRAPH></PLUGIN:OPEN-GRAPH>" + "<p>preserve & raw</p>";

        // Act
        var result = await new OpenGraphPlugin().PostHtmlAsync(html, Document(), Manifest(), Site(), Xunit.TestContext.Current.CancellationToken);

        // Assert
        var parsed = new HtmlParser().ParseDocument(result);
        parsed.QuerySelectorAll("meta[property='og:title']").Length.ShouldBe(3);
        result.ShouldContain("<p>preserve & raw</p>");
        result.ShouldNotContain("<plugin:", Case.Insensitive);
    }

    [Theory]
    [InlineData("OPEN-GRAPH")]
    [InlineData(" open-graph")]
    [InlineData("open-graph ")]
    [InlineData("Open Graph")]
    public void Given_InvalidSelector_When_Rendered_Then_It_Should_Preserve_UpstreamIdValidation(string id)
    {
        // Arrange
        using var context = new BunitContext();

        // Act
        Action act = () => context.Render<MetadataHost>(parameters => parameters
            .Add(p => p.Id, id)
            .Add(p => p.Plugins, new[] { Manifest() }));

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldContain("plugin ID");
    }

    [Fact]
    public void Given_AbsentManifestWithInvalidSiteAndImage_When_Rendered_Then_It_Should_Not_ValidateUnusedContext()
    {
        // Arrange
        using var context = new BunitContext();

        // Act
        var cut = context.Render<MetadataHost>(parameters => parameters
            .Add(p => p.Site, Site(siteUrl: "invalid"))
            .Add(p => p.Document, Document(image: "javascript:unused"))
            .Add(p => p.Plugins, new[] { Manifest(id: "other") }));

        // Assert
        cut.Markup.ShouldBeEmpty();
    }

    [Fact]
    public void Given_InvalidContextWhileDisabled_When_ManifestIsAdded_Then_It_Should_Validate_RequiredContext()
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<MetadataHost>(parameters => parameters.Add(p => p.Site, Site(siteUrl: "invalid")));

        // Act
        Action act = () => cut.Render(parameters => parameters.Add(p => p.Plugins, new[] { Manifest() }));

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.SiteUrl");
    }

    [Fact]
    public void Given_ConfiguredComponent_When_SiteBecomesInvalid_Then_It_Should_Fail_InsteadOfKeepingOldMetadata()
    {
        // Arrange
        using var context = new BunitContext();
        var cut = Render(context, Site(), Document(), Manifest());

        // Act
        Action act = () => cut.Render(parameters => parameters.Add(p => p.Site, Site(siteUrl: "invalid")));

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.SiteUrl");
    }

    [Fact]
    public void Given_ContextAndManifestTransitions_When_OnParametersSet_Then_It_Should_Clear_AndRecomputeMetadata()
    {
        // Arrange
        using var context = new BunitContext();
        var initialSite = Site();
        var initialDocument = Document(handle: "@author");
        var cut = Render(context, initialSite, initialDocument, Manifest());

        // Act
        cut.Render(parameters => parameters
            .Add(p => p.Site, Site(siteUrl: "http://other.example", baseUrl: "/new", image: null, title: "New site"))
            .Add(p => p.Document, Document(kind: ContentKind.Page, title: "About", slug: "/about", image: null))
            .Add(p => p.Plugins, new[] { Manifest(siteId: "@updated", creatorId: "@new-creator") }));
        var page = Parse(cut.Markup);
        cut.Render(parameters => parameters.Add(p => p.Documents, Array.Empty<ContentDocument>()));
        var collection = Parse(cut.Markup);
        cut.Render(parameters => parameters
            .Add(p => p.Documents, (IEnumerable<ContentDocument>?)null)
            .Add(p => p.Document, Document(sourcePath: "", image: null)));
        var sourceLess = Parse(cut.Markup);
        cut.Render(parameters => parameters
            .Add(p => p.Documents, (IEnumerable<ContentDocument>?)null)
            .Add(p => p.Document, (ContentDocument?)null));
        var siteOnly = Parse(cut.Markup);
        cut.Render(parameters => parameters.Add(p => p.Plugins, Array.Empty<PluginManifest>()));
        cut.Render(parameters => parameters.Add(p => p.Site, (SiteManifest?)null));
        var removed = cut.Markup;
        cut.Render(parameters => parameters.Add(p => p.Site, initialSite));
        cut.Render(parameters => parameters
            .Add(p => p.Document, initialDocument)
            .Add(p => p.Plugins, new[] { Manifest(siteId: "@restored", creatorId: "@default") }));
        var restored = Parse(cut.Markup);

        // Assert
        page["og:title"].ShouldBe("About | New site");
        page["og:url"].ShouldBe("http://other.example/new/about");
        page["twitter:site"].ShouldBe("@updated");
        page.ContainsKey("twitter:creator").ShouldBeFalse();
        page.ContainsKey("og:image").ShouldBeFalse();
        collection["og:title"].ShouldBe("New site");
        collection.ContainsKey("twitter:creator").ShouldBeFalse();
        sourceLess["og:title"].ShouldBe("New site");
        sourceLess.ContainsKey("twitter:creator").ShouldBeFalse();
        siteOnly["og:title"].ShouldBe("New site");
        siteOnly["og:url"].ShouldBe("http://other.example/new");
        removed.ShouldBeEmpty();
        restored["twitter:site"].ShouldBe("@restored");
        restored["twitter:creator"].ShouldBe("@author");
        restored["og:image"].ShouldBe("https://example.com/blog/images/post.png");
    }

    [Fact]
    public void Given_IdSelectionChangesWithDuplicateDisplayLabels_When_OnParametersSet_Then_It_Should_Select_ByExactId()
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<MetadataHost>(parameters => parameters
            .Add(p => p.Site, Site())
            .Add(p => p.Document, Document())
            .Add(p => p.Plugins, new[] { Manifest(siteId: "@first"), Manifest(id: "other", siteId: "@second") }));

        // Act
        cut.Render(parameters => parameters.Add(p => p.Id, "other"));
        var selected = Parse(cut.Markup);
        cut.Render(parameters => parameters.Add(p => p.Id, "missing"));
        var removed = cut.Markup;
        cut.Render(parameters => parameters.Add(p => p.Id, "open-graph"));

        // Assert
        selected["twitter:site"].ShouldBe("@second");
        removed.ShouldBeEmpty();
        Parse(cut.Markup)["twitter:site"].ShouldBe("@first");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_PreviewOrProduction_When_Rendered_Then_It_Should_Apply_TheSameMetadataRules(bool isPreview)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(isPreview: isPreview);
        var document = Document(image: "https://cdn.example.com/post.png");
        var plugin = Manifest();

        // Act
        var hook = Parse(await Hook(site, document, plugin));
        var component = Render(context, site, document, plugin);

        // Assert
        AssertParity(hook, Parse(component.Markup));
        hook["og:title"].ShouldBe("Document | Site");
        hook["og:image"].ShouldBe("https://cdn.example.com/post.png");
        hook["twitter:creator"].ShouldBe("@creator");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_PreviewOrProductionWithoutValidOrigin_When_Rendered_Then_It_Should_Apply_TheSameValidation(bool isPreview)
    {
        // Arrange
        using var context = new BunitContext();
        var site = Site(siteUrl: "", isPreview: isPreview);
        var document = Document();
        var plugin = Manifest();

        // Act
        Func<Task> hook = () => Hook(site, document, plugin);
        Action component = () => Render(context, site, document, plugin);

        // Assert
        (await hook.ShouldThrowAsync<ArgumentException>()).Message.ShouldContain("Site.SiteUrl");
        component.ShouldThrow<ArgumentException>().Message.ShouldContain("Site.SiteUrl");
    }

    private static IRenderedComponent<MetadataHost> Render(
        BunitContext context, SiteManifest? site, ContentDocument? document, PluginManifest plugin,
        IEnumerable<ContentDocument>? documents = null)
    {
        return context.Render<MetadataHost>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Document, document)
            .Add(p => p.Documents, documents)
            .Add(p => p.Plugins, new[] { plugin }));
    }

    private static Task<string> Hook(SiteManifest? site, ContentDocument? document, PluginManifest plugin)
        => new OpenGraphPlugin().PostHtmlAsync(Marker, document!, plugin, site!, Xunit.TestContext.Current.CancellationToken);

    private static Dictionary<string, string> Parse(string html)
        => new HtmlParser().ParseDocument(html).QuerySelectorAll("meta")
            .ToDictionary(meta => meta.GetAttribute("property") ?? meta.GetAttribute("name")!,
                meta => meta.GetAttribute("content") ?? string.Empty, StringComparer.Ordinal);

    private static void AssertParity(Dictionary<string, string> hook, Dictionary<string, string> component)
    {
        component.Keys.Order().ShouldBe(hook.Keys.Order());
        foreach (var (key, value) in hook)
        {
            component[key].ShouldBe(value, key);
        }
    }

    private static SiteManifest Site(
        string? siteUrl = "https://example.com", string baseUrl = "/blog/", string? image = "/images/site.png",
        string title = "Site", string description = "Site description", string locale = "en-US", bool isPreview = false)
        => new()
        {
            SiteUrl = siteUrl!,
            BaseUrl = baseUrl,
            HeroImage = image,
            Title = title,
            Description = description,
            Locale = locale,
            IsPreview = isPreview,
        };

    private static ContentDocument Document(
        ContentKind kind = ContentKind.Post, string sourcePath = "/post.md", string title = "Document",
        string slug = "/post", string? description = "Document description", string? image = "/images/post.png",
        string? handle = null)
        => new()
        {
            Kind = kind,
            SourcePath = sourcePath,
            Metadata = new ContentMetadata
            {
                Title = title,
                Slug = slug,
                Description = description,
                HeroImage = image,
                TwitterHandle = handle,
            },
        };

    private static PluginManifest Manifest(string id = "open-graph", string? siteId = "@site", string? creatorId = "@creator")
        => new()
        {
            Id = id,
            Name = "Shared label",
            Options = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>
            {
                ["TwitterSiteId"] = siteId,
                ["TwitterCreatorId"] = creatorId,
            }),
        };

    public sealed class MetadataHost : ComponentBase
    {
        [Parameter] public SiteManifest? Site { get; set; }
        [Parameter] public ContentDocument? Document { get; set; }
        [Parameter] public IEnumerable<ContentDocument>? Documents { get; set; }
        [Parameter] public IEnumerable<PluginManifest>? Plugins { get; set; }
        [Parameter] public string Id { get; set; } = "open-graph";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, Cascade(Site, Cascade(Document, Cascade(Documents, Cascade(Plugins, child =>
            {
                child.OpenComponent<OpenGraphComponent>(0);
                child.AddAttribute(1, nameof(OpenGraphComponent.Id), Id);
                child.AddAttribute(2, nameof(OpenGraphComponent.Name), "Unrelated component label");
                child.CloseComponent();
            })))));
        }

        private static RenderFragment Cascade<T>(T value, RenderFragment child) => builder =>
        {
            builder.OpenComponent<CascadingValue<T>>(0);
            builder.AddAttribute(1, nameof(CascadingValue<T>.Value), value);
            builder.AddAttribute(2, nameof(CascadingValue<T>.ChildContent), child);
            builder.CloseComponent();
        };
    }
}
