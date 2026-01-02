using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics.Tests;

public class GoogleAnalyticsComponentTests
{
	[Fact]
	public void Given_PluginIsNull_When_Rendered_Then_It_Should_Render_GoogleTag_With_EmptyMeasurementId()
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest();
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");

		// Act
		var cut = ctx.Render<GoogleAnalyticsComponent>(parameters => parameters
			.Add(p => p.Name, "Google Analytics")
			.AddCascadingValue(site)
			.AddCascadingValue(document));

		// Assert
		cut.Markup.ShouldContain("gtag/js?id=");
		cut.Markup.ShouldContain("gtag('config', '');");
	}

	[Fact]
	public void Given_PluginOptionsIsNull_When_Rendered_Then_It_Should_Render_GoogleTag_With_EmptyMeasurementId()
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest();
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var plugin = new PluginManifest { Name = "Google Analytics", Options = null };

		// Act
		var cut = ctx.Render<GoogleAnalyticsComponent>(parameters => parameters
			.Add(p => p.Name, "Google Analytics")
			.AddCascadingValue(site)
			.AddCascadingValue(document)
			.AddCascadingValue<IEnumerable<PluginManifest>>(new[] { plugin }));

		// Assert
		cut.Markup.ShouldContain("gtag/js?id=");
		cut.Markup.ShouldContain("gtag('config', '');");
	}

	[Theory]
	[InlineData("G-XXXXXXXXXX")]
	public void Given_MeasurementId_When_Rendered_Then_It_Should_Render_GoogleTag_With_MeasurementId(string measurementId)
	{
		// Arrange
		using var ctx = new BunitContext();
		var site = CreateSiteManifest();
		var document = CreateDocument(kind: ContentKind.Post, title: "Hello", slug: "/hello-world");
		var plugin = new PluginManifest
		{
			Name = "Google Analytics",
			Options = new Dictionary<string, object?>
			{
				{ "MeasurementId", measurementId },
			}
		};

		// Act
		var cut = ctx.Render<GoogleAnalyticsComponent>(parameters => parameters
			.Add(p => p.Name, "Google Analytics")
			.AddCascadingValue(site)
			.AddCascadingValue(document)
			.AddCascadingValue<IEnumerable<PluginManifest>>(new[] { plugin }));

		// Assert
		cut.Markup.ShouldContain($"gtag/js?id={measurementId}");
		cut.Markup.ShouldContain($"gtag('config', '{measurementId}');");
	}

	private static ContentDocument CreateDocument(ContentKind kind, string title, string slug)
	{
		return new ContentDocument
		{
			Kind = kind,
			SourcePath = "/posts/hello-world.md",
			Metadata = new ContentMetadata
			{
				Title = title,
				Slug = slug,
				Description = "Document description",
				HeroImage = "/images/doc-hero.png",
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

