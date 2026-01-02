using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph.Tests;

public class OpenGraphComponentTests
{
	[Fact]
	public void Given_ValidPostWithPluginOptions_When_Rendered_Then_It_Should_Render_OpenGraph_And_TwitterTags()
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest(siteUrl: "https://example.com", baseUrl: "", title: "My Blog", description: "Site description", locale: "en-US", heroImage: "/images/site-hero.png");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", description: "Post description", heroImage: "/images/hero.png");
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");

		// Act
		var cut = ctx.Render<OpenGraphComponent>(parameters => parameters
			.Add(p => p.Name, "Open Graph")
			.AddCascadingValue(site)
			.AddCascadingValue(document)
			.AddCascadingValue<IEnumerable<PluginManifest>>(new[] { plugin }));

		// Assert
		cut.Markup.ShouldContain("property=\"og:title\"");
		cut.Markup.ShouldContain("property=\"og:title\" content=\"Hello | My Blog\"");
		cut.Markup.ShouldContain("property=\"og:description\" content=\"Post description\"");
		cut.Markup.ShouldContain("property=\"og:locale\" content=\"en-US\"");
		cut.Markup.ShouldContain("property=\"og:url\" content=\"https://example.com/hello-world\"");
		cut.Markup.ShouldContain("property=\"og:image\" content=\"https://example.com/images/hero.png\"");
		cut.Markup.ShouldContain("property=\"og:site_name\" content=\"My Blog\"");

		cut.Markup.ShouldContain("name=\"twitter:card\"");
		cut.Markup.ShouldContain("name=\"twitter:site\" content=\"@site\"");
		cut.Markup.ShouldContain("name=\"twitter:creator\" content=\"@creator\"");
		cut.Markup.ShouldContain("name=\"twitter:title\" content=\"Hello | My Blog\"");
		cut.Markup.ShouldContain("name=\"twitter:description\" content=\"Post description\"");
		cut.Markup.ShouldContain("name=\"twitter:image\" content=\"https://example.com/images/hero.png\"");
	}

	[Fact]
	public void Given_EmptyTwitterSiteId_When_Rendered_Then_It_Should_Not_Render_TwitterSiteTag()
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest();
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var plugin = CreatePluginManifest(twitterSiteId: null, twitterCreatorId: "@creator");

		// Act
		var cut = ctx.Render<OpenGraphComponent>(parameters => parameters
			.Add(p => p.Name, "Open Graph")
			.AddCascadingValue(site)
			.AddCascadingValue(document)
			.AddCascadingValue<IEnumerable<PluginManifest>>(new[] { plugin }));

		// Assert
		cut.Markup.ShouldNotContain("name=\"twitter:site\"");
		cut.Markup.ShouldContain("name=\"twitter:card\"");
	}

	[Fact]
	public void Given_PageDocument_When_Rendered_Then_It_Should_Not_Render_TwitterCreatorTag()
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest();
		var document = CreateDocument(kind: ContentKind.Page, title: "About", slug: "/about", twitterHandle: "@handle");
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");

		// Act
		var cut = ctx.Render<OpenGraphComponent>(parameters => parameters
			.Add(p => p.Name, "Open Graph")
			.AddCascadingValue(site)
			.AddCascadingValue(document)
			.AddCascadingValue<IEnumerable<PluginManifest>>(new[] { plugin }));

		// Assert
		cut.Markup.ShouldContain("name=\"twitter:site\" content=\"@site\"");
		cut.Markup.ShouldNotContain("name=\"twitter:creator\"");
	}

	[Fact]
	public void Given_TwitterHandleInMetadata_When_Rendered_Then_It_Should_Override_TwitterCreatorIdOption()
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest();
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", twitterHandle: "@from-metadata");
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@from-options");

		// Act
		var cut = ctx.Render<OpenGraphComponent>(parameters => parameters
			.Add(p => p.Name, "Open Graph")
			.AddCascadingValue(site)
			.AddCascadingValue(document)
			.AddCascadingValue<IEnumerable<PluginManifest>>(new[] { plugin }));

		// Assert
		cut.Markup.ShouldContain("name=\"twitter:creator\" content=\"@from-metadata\"");
		cut.Markup.ShouldNotContain("name=\"twitter:creator\" content=\"@from-options\"");
	}

	[Fact]
	public void Given_DocumentsNotNull_When_Rendered_Then_It_Should_Use_SiteMetadata_And_Not_Render_TwitterCreatorTag()
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest(title: "Site title", description: "Site description");
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world", description: "Post description", twitterHandle: "@handle");
		var plugin = CreatePluginManifest(twitterSiteId: "@site", twitterCreatorId: "@creator");
		IEnumerable<ContentDocument> documents = new[] { document };

		// Act
		var cut = ctx.Render<OpenGraphComponent>(parameters => parameters
			.Add(p => p.Name, "Open Graph")
			.AddCascadingValue(site)
			.AddCascadingValue(document)
			.AddCascadingValue(documents)
			.AddCascadingValue<IEnumerable<PluginManifest>>(new[] { plugin }));

		// Assert
		cut.Markup.ShouldContain("property=\"og:title\" content=\"Site title\"");
		cut.Markup.ShouldContain("property=\"og:description\" content=\"Site description\"");
		cut.Markup.ShouldNotContain("name=\"twitter:creator\"");
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

	private static PluginManifest CreatePluginManifest(string? twitterSiteId = null, string? twitterCreatorId = null)
	{
		return new PluginManifest
		{
			Name = "Open Graph",
			Options = new Dictionary<string, object?>
			{
				{ "TwitterSiteId", twitterSiteId },
				{ "TwitterCreatorId", twitterCreatorId },
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

