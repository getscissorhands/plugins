using Bunit;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics.Tests;

public class GoogleAnalyticsComponentTests
{
	[Fact]
	public void Given_NullPlugin_When_Rendered_Then_It_Should_Not_Throw()
	{
		// Arrange
		using var ctx = new BunitContext();
		var document = new ContentDocument();
		var site = new SiteManifest();

		// Act
		var render = () => ctx.Render<GoogleAnalyticsComponent>(ps => ps
			.Add(p => p.Plugin, (PluginManifest?)null)
			.Add(p => p.Document, document)
			.Add(p => p.Site, site));

		// Assert
		render.ShouldNotThrow();
	}

	[Theory]
	[InlineData("G-XXXXXXXXXX")]
	public void Given_MeasurementId_When_Rendered_Then_It_Should_Render_GtagScript_With_Id(string measurementId)
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = CreatePluginManifest(measurementId);

		// Act
		var cut = ctx.Render<GoogleAnalyticsComponent>(ps => ps
			.Add(p => p.Plugin, plugin));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.Markup.ShouldContain($"https://www.googletagmanager.com/gtag/js?id={measurementId}");
			cut.Markup.ShouldContain($"gtag('config', '{measurementId}');");
		});
	}

	[Fact]
	public void Given_NoMeasurementId_When_Rendered_Then_It_Should_Not_Throw_And_Should_Not_Contain_Any_Id()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = new PluginManifest
		{
			Options = new Dictionary<string, object?>
			{
				{ "MeasurementId", null }
			}
		};

		// Act
		var cut = ctx.Render<GoogleAnalyticsComponent>(ps => ps
			.Add(p => p.Plugin, plugin));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.Markup.ShouldNotContain("gtag/js?id=G-");
			cut.Markup.ShouldNotContain("gtag('config', 'G-");
		});
	}

	[Fact]
	public void Given_MeasurementIdKeyMissing_When_Rendered_Then_It_Should_Not_Throw_And_Should_Not_Contain_Any_Id()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = new PluginManifest
		{
			Options = new Dictionary<string, object?>()
		};

		// Act
		var cut = ctx.Render<GoogleAnalyticsComponent>(ps => ps
			.Add(p => p.Plugin, plugin));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.Markup.ShouldNotContain("gtag/js?id=G-");
			cut.Markup.ShouldNotContain("gtag('config', 'G-");
		});
	}

	[Fact]
	public void Given_NonStringMeasurementId_When_Rendered_Then_It_Should_Not_Throw_And_Should_Not_Contain_Any_Id()
	{
		// Arrange
		using var ctx = new BunitContext();
		var plugin = new PluginManifest
		{
			Options = new Dictionary<string, object?>
			{
				{ "MeasurementId", 12345 }
			}
		};

		// Act
		var cut = ctx.Render<GoogleAnalyticsComponent>(ps => ps
			.Add(p => p.Plugin, plugin));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.Markup.ShouldNotContain("gtag/js?id=G-");
			cut.Markup.ShouldNotContain("gtag('config', 'G-");
		});
	}

	private static PluginManifest CreatePluginManifest(string measurementId)
	{
		return new PluginManifest
		{
			Options = new Dictionary<string, object?>
			{
				{ "MeasurementId", measurementId }
			}
		};
	}
}
