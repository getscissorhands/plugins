using Bunit;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph.Tests;

public class OpenGraphComponentTests
{
	[Fact]
	public void Given_NullPlugin_When_Rendered_Then_It_Should_Not_Throw()
	{
		// Arrange
		using var ctx = new BunitContext();
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var site = CreateSiteManifest();

		// Act
		var render = () => ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, (PluginManifest?)null)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		render.ShouldNotThrow();
	}

	[Fact]
	public void Given_NullPluginOptions_When_Rendered_Then_It_Should_Not_Render_Twitter_MetaTags()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = new PluginManifest { Options = null };
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var site = CreateSiteManifest();

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:site']").Count.ShouldBe(0);
			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(0);
		});
	}

	[Fact]
	public void Given_TwitterOptionsAndPost_When_Rendered_Then_It_Should_Render_TwitterSite_And_Creator_MetaTags()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var site = CreateSiteManifest();

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:site']").Count.ShouldBe(1);
			cut.Find("meta[name='twitter:site']").GetAttribute("content").ShouldBe("@site");

			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(1);
			cut.Find("meta[name='twitter:creator']").GetAttribute("content").ShouldBe("@creator");

			cut.Find("meta[property='og:title']").GetAttribute("content").ShouldBe("Hello | Site title");
			cut.Find("meta[name='twitter:title']").GetAttribute("content").ShouldBe("Hello | Site title");

			cut.Find("meta[property='og:description']").GetAttribute("content").ShouldBe("Document description");
			cut.Find("meta[name='twitter:description']").GetAttribute("content").ShouldBe("Document description");
		});
	}

	[Fact]
	public void Given_NoTwitterSiteId_When_Rendered_Then_It_Should_Not_Render_TwitterSite_MetaTag()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(twitterSiteId: null, twitterCreatorId: "@creator");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var site = CreateSiteManifest();

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:site']").Count.ShouldBe(0);
			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(1);
		});
	}

	[Fact]
	public void Given_PageDocument_When_Rendered_Then_It_Should_Not_Render_TwitterCreator_MetaTag()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
		var document = CreateDocument(kind: ContentKind.Page, title: "About", slug: "/about", twitterHandle: "@handle");
		var site = CreateSiteManifest();

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:site']").Count.ShouldBe(1);
			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(0);
		});
	}

	[Fact]
	public void Given_TwitterHandleInMetadata_When_Rendered_Then_It_Should_Override_TwitterCreatorIdOption()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@from-options");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", twitterHandle: "@from-metadata");
		var site = CreateSiteManifest();

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(1);
			cut.Find("meta[name='twitter:creator']").GetAttribute("content").ShouldBe("@from-metadata");
		});
	}

	[Fact]
	public void Given_DocumentsCollection_When_Rendered_Then_It_Should_Use_SiteTitleAndDescription_And_Clear_TwitterCreator()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", description: "Post description");
		var site = CreateSiteManifest(title: "Site title", description: "Site description");
		IEnumerable<ContentDocument> documents = new[] { document };

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Documents, documents)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(0);

			cut.Find("meta[property='og:title']").GetAttribute("content").ShouldBe("Site title");
			cut.Find("meta[property='og:description']").GetAttribute("content").ShouldBe("Site description");
		});
	}

	[Fact]
	public void Given_EmptySourcePath_When_Rendered_Then_It_Should_Use_SiteTitleAndDescription_And_Clear_TwitterCreator()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", description: "Post description", sourcePath: "");
		var site = CreateSiteManifest(title: "Site title", description: "Site description");

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(0);
			cut.Find("meta[property='og:title']").GetAttribute("content").ShouldBe("Site title");
			cut.Find("meta[property='og:description']").GetAttribute("content").ShouldBe("Site description");
		});
	}

	[Fact]
	public void Given_NullDocumentDescription_When_Rendered_Then_It_Should_Not_Render_Description_Content_Attributes()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", description: null);
		var site = CreateSiteManifest(description: "Site description");

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.Find("meta[property='og:description']").GetAttribute("content").ShouldBeNull();
			cut.Find("meta[name='twitter:description']").GetAttribute("content").ShouldBeNull();
		});
	}

	[Fact]
	public void Given_InvalidOptionTypes_When_Rendered_Then_It_Should_Not_Render_TwitterSite_Or_Creator_MetaTags()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = new PluginManifest
		{
			Options = new Dictionary<string, object?>
			{
				{ "TwitterSiteId", 12345 },
				{ "TwitterCreatorId", 67890 },
			}
		};
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var site = CreateSiteManifest();

		// Act
		var cut = ctx.Render<OpenGraphComponent>(ps => ps
			.Add(p => p.Plugin, plugin)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("meta[name='twitter:site']").Count.ShouldBe(0);
			cut.FindAll("meta[name='twitter:creator']").Count.ShouldBe(0);
		});
	}

	private static PluginManifest CreatePluginManifest(string? twitterSiteId, string? twitterCreatorId)
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

	private static ContentDocument CreateDocument(
		ContentKind kind,
		string title,
		string slug,
		string? description = "Document description",
		string? heroImage = "/images/doc-hero.png",
		string? twitterHandle = null,
		string? sourcePath = "/posts/hello-world.md")
	{
		return new ContentDocument
		{
			Kind = kind,
			SourcePath = sourcePath ?? string.Empty,
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
