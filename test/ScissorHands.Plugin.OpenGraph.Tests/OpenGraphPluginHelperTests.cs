using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph.Tests;

public class OpenGraphPluginHelperTests
{
	[Theory]
	[InlineData("https://example.com", "", "/hello-world", "https://example.com/hello-world")]
	[InlineData("https://example.com/", "", "/hello-world", "https://example.com/hello-world")]
	[InlineData("https://example.com", "/blog/", "/hello-world", "https://example.com/blog/hello-world")]
	[InlineData("https://example.com/", "blog", "hello-world", "https://example.com/blog/hello-world")]
	public void Given_DocumentAndSite_When_GetContentUrl_Invoked_Then_It_Should_Compose_Url(
		string siteUrl,
		string baseUrl,
		string slug,
		string expected)
	{
		// Arrange
		var document = CreateDocument(slug);
		var site = CreateSite(siteUrl, baseUrl);

		// Act
		var result = OpenGraphPluginHelper.GetContentUrl(document, site);

		// Assert
		result.ShouldBe(expected);
	}

	[Theory]
	[InlineData("https://example.com", "/blog/", "/", "https://example.com/blog")]
	[InlineData("https://example.com/", "blog", "/", "https://example.com/blog")]
	public void Given_SlugIsRoot_When_GetContentUrl_Invoked_Then_It_Should_Return_SiteRootWithoutTrailingSlash(
		string siteUrl,
		string baseUrl,
		string slug,
		string expected)
	{
		// Arrange
		var document = CreateDocument(slug);
		var site = CreateSite(siteUrl, baseUrl);

		// Act
		var result = OpenGraphPluginHelper.GetContentUrl(document, site);

		// Assert
		result.ShouldBe(expected);
	}

	[Theory]
	[InlineData("https://example.com", "", "/images/site.png", null, "https://example.com/images/site.png")]
	[InlineData("https://example.com", "/blog/", "/images/site.png", null, "https://example.com/blog/images/site.png")]
	[InlineData("https://example.com", "/blog/", "/images/site.png", "/images/post.png", "https://example.com/blog/images/post.png")]
	[InlineData("https://example.com/", "blog", "/images/site.png", "images/post.png", "https://example.com/blog/images/post.png")]
	public void Given_DocumentAndSite_When_GetHeroImageUrl_Invoked_Then_It_Should_Return_Expected(
		string siteUrl,
		string baseUrl,
		string siteHeroImage,
		string? contentHeroImage,
		string expected)
	{
		// Arrange
		var document = CreateDocument("/hello-world", heroImage: contentHeroImage);
		var site = CreateSite(siteUrl, baseUrl, heroImage: siteHeroImage);

		// Act
		var result = OpenGraphPluginHelper.GetHeroImageUrl(document, site);

		// Assert
		result.ShouldBe(expected);
	}

	[Fact]
	public void Given_NullArguments_When_GetContentUrl_Invoked_Then_It_Should_Return_EmptyString()
	{
		// Arrange
		ContentDocument? document = null;
		SiteManifest? site = null;

		// Act
		var result = OpenGraphPluginHelper.GetContentUrl(document, site);

		// Assert
		result.ShouldBe(string.Empty);
	}

	[Fact]
	public void Given_NullArguments_When_GetHeroImageUrl_Invoked_Then_It_Should_Return_Slash()
	{
		// Arrange
		ContentDocument? document = null;
		SiteManifest? site = null;

		// Act
		var result = OpenGraphPluginHelper.GetHeroImageUrl(document, site);

		// Assert
		result.ShouldBe("/");
	}

	private static ContentDocument CreateDocument(string slug, string? heroImage = null)
	{
		return new ContentDocument
		{
			Kind = ContentKind.Post,
			Metadata = new ContentMetadata
			{
				Title = "Title",
				Slug = slug,
				Description = "Description",
				HeroImage = heroImage,
			}
		};
	}

	private static SiteManifest CreateSite(string siteUrl, string baseUrl, string heroImage = "/images/site.png")
	{
		return new SiteManifest
		{
			SiteUrl = siteUrl,
			BaseUrl = baseUrl,
			HeroImage = heroImage,
			Title = "Site Title",
			Description = "Site Description",
			Locale = "en-US",
		};
	}
}
