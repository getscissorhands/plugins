using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.OpenGraph.Tests;

public class OpenGraphPluginHelperTests
{
	[Fact]
	public void Given_DocumentsNotNull_When_UseContentMetadata_Invoked_Then_It_Should_Return_False()
	{
		// Arrange
		IEnumerable<ContentDocument> documents = new[] { CreateDocument("/hello-world", sourcePath: "/posts/hello-world.md") };
		var document = CreateDocument("/hello-world", sourcePath: "/posts/hello-world.md");

		// Act
		var result = OpenGraphPluginHelper.UseContentMetadata(documents, document);

		// Assert
		result.ShouldBeFalse();
	}

	[Fact]
	public void Given_NullDocument_When_UseContentMetadata_Invoked_Then_It_Should_Return_False()
	{
		// Arrange
		IEnumerable<ContentDocument>? documents = null;
		ContentDocument? document = null;

		// Act
		var result = OpenGraphPluginHelper.UseContentMetadata(documents, document);

		// Assert
		result.ShouldBeFalse();
	}

	[Fact]
	public void Given_NullDocumentsAndNonNullDocument_When_UseContentMetadata_Invoked_Then_It_Should_Return_True()
	{
		// Arrange
		IEnumerable<ContentDocument>? documents = null;
		var document = CreateDocument("/hello-world", sourcePath: "/posts/hello-world.md");

		// Act
		var result = OpenGraphPluginHelper.UseContentMetadata(documents, document);

		// Assert
		result.ShouldBeTrue();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("\t")]
	public void Given_NullDocumentsAndDocumentWithEmptySourcePath_When_UseContentMetadata_Invoked_Then_It_Should_Return_False(string? sourcePath)
	{
		// Arrange
		IEnumerable<ContentDocument>? documents = null;
		var document = CreateDocument("/hello-world", sourcePath: sourcePath);

		// Act
		var result = OpenGraphPluginHelper.UseContentMetadata(documents, document);

		// Assert
		result.ShouldBeFalse();
	}

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

	[Fact]
	public void Given_NullPlugin_When_GetOptionValue_Invoked_Then_It_Should_Return_Default()
	{
		// Arrange
		PluginManifest? plugin = null;

		// Act
		var result = OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterSiteId");

		// Assert
		result.ShouldBeNull();
	}

	[Fact]
	public void Given_NullPluginOptions_When_GetOptionValue_Invoked_Then_It_Should_Return_Default()
	{
		// Arrange
		var plugin = new PluginManifest { Options = null };

		// Act
		var result = OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterSiteId");

		// Assert
		result.ShouldBeNull();
	}

	[Fact]
	public void Given_MissingKey_When_GetOptionValue_Invoked_Then_It_Should_Return_Default()
	{
		// Arrange
		var plugin = new PluginManifest
		{
			Options = new Dictionary<string, object?>
			{
				{ "SomeOtherKey", "SomeValue" },
			}
		};

		// Act
		var result = OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterSiteId");

		// Assert
		result.ShouldBeNull();
	}

	[Fact]
	public void Given_WrongTypeValue_When_GetOptionValue_Invoked_Then_It_Should_Return_Default()
	{
		// Arrange
		var plugin = new PluginManifest
		{
			Options = new Dictionary<string, object?>
			{
				{ "TwitterSiteId", 12345 },
			}
		};

		// Act
		var result = OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterSiteId");

		// Assert
		result.ShouldBeNull();
	}

	[Fact]
	public void Given_ValidTypedValue_When_GetOptionValue_Invoked_Then_It_Should_Return_Value()
	{
		// Arrange
		var plugin = new PluginManifest
		{
			Options = new Dictionary<string, object?>
			{
				{ "TwitterSiteId", "@site" },
			}
		};

		// Act
		var result = OpenGraphPluginHelper.GetOptionValue<string>(plugin, "TwitterSiteId");

		// Assert
		result.ShouldBe("@site");
	}

	[Theory]
	[InlineData("https://example.com", "")]
	[InlineData("https://example.com/", "blog")]
	public void Given_NullSlug_When_GetContentUrl_Invoked_Then_It_Should_Throw_NullReferenceException(
		string siteUrl,
		string baseUrl)
	{
		// Arrange
		var document = new ContentDocument
		{
			Kind = ContentKind.Post,
			SourcePath = "/posts/hello-world.md",
			Metadata = new ContentMetadata
			{
				Title = "Title",
				Slug = null!,
				Description = "Description",
			}
		};
		var site = CreateSite(siteUrl, baseUrl);

		// Act
		Func<string> func = () => OpenGraphPluginHelper.GetContentUrl(document, site);

		// Assert
		func.ShouldThrow<NullReferenceException>();
	}

	private static ContentDocument CreateDocument(string slug, string? heroImage = null, string? sourcePath = "/posts/hello-world.md")
	{
		return new ContentDocument
		{
			Kind = ContentKind.Post,
			SourcePath = sourcePath ?? string.Empty,
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
