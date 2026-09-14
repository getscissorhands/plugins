using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics.Tests;

public class GoogleAnalyticsComponentTests
{
    [Theory]
    [MemberData(nameof(GoogleAnalyticsTestData.InvalidConfigurationCases), MemberType = typeof(GoogleAnalyticsTestData))]
    public void Given_InvalidConfiguration_When_OnParametersSet_Then_It_Should_Throw_Contextual_NonLeaking_Error(string scenario)
    {
        // Arrange
        using var context = new BunitContext();
        var manifest = new PluginManifest
        {
            Id = "google-analytics",
            Options = GoogleAnalyticsTestData.CreateInvalidOptions(scenario),
        };

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => context.Render<AnalyticsHost>(
            parameters => parameters.Add(p => p.Plugins, new[] { manifest })));

        // Assert
        exception.Message.ShouldBe(GoogleAnalyticsTestData.ConfigurationError);
        exception.InnerException.ShouldBeNull();
    }

    [Theory]
    [InlineData("G-EXAMPLE")]
    [InlineData("G-A")]
    [InlineData("G-0")]
    [InlineData("G-A1B2C3")]
    [InlineData("G-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public void Given_ValidSyntheticMeasurementId_When_OnParametersSet_Then_It_Should_Emit_Intact_Safe_Tag(string measurementId)
    {
        // Arrange
        using var context = new BunitContext();
        var manifest = GoogleAnalyticsTestData.CreateManifest(measurementId);

        // Act
        var cut = context.Render<AnalyticsHost>(parameters => parameters.Add(p => p.Plugins, new[] { manifest }));

        // Assert
        GoogleAnalyticsTestData.AssertTag(cut.FindAll("script"), measurementId);
        cut.Markup.ShouldNotContain("{{");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Shared display name")]
    public void Given_OptionalOrRepeatedDisplayNames_When_OnParametersSet_Then_It_Should_Select_By_Exact_Id(string? name)
    {
        // Arrange
        using var context = new BunitContext();
        var manifest = GoogleAnalyticsTestData.CreateManifest("G-SELECTED", name: name);
        var other = GoogleAnalyticsTestData.CreateManifest("G-OTHER", "other-analytics", name);

        // Act
        var cut = context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, new[] { other, manifest })
            .Add(p => p.Name, "A different component display name"));

        // Assert
        cut.Markup.ShouldContain("gtag('config', 'G-SELECTED');");
        cut.Markup.ShouldNotContain("G-OTHER");
    }

    [Theory]
    [InlineData("missing")]
    public void Given_NonmatchingSelector_When_OnParametersSet_Then_It_Should_Render_Nothing_Without_Validating_Other_Manifests(string id)
    {
        // Arrange
        using var context = new BunitContext();
        var manifest = new PluginManifest { Id = "google-analytics", Name = id, Options = null };

        // Act
        var cut = context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, new[] { manifest })
            .Add(p => p.Id, id)
            .Add(p => p.Name, id));

        // Assert
        cut.Markup.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("GOOGLE-ANALYTICS")]
    [InlineData("Google Analytics")]
    [InlineData(" google-analytics")]
    [InlineData("google-analytics ")]
    public void Given_InvalidSelector_When_OnParametersSet_Then_It_Should_Propagate_Upstream_Identity_Validation_Without_Normalization(string id)
    {
        // Arrange
        using var context = new BunitContext();
        var manifest = GoogleAnalyticsTestData.CreateManifest("G-EXAMPLE", name: id);

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, new[] { manifest })
            .Add(p => p.Id, id)
            .Add(p => p.Name, id)));

        // Assert
        exception.Message.ShouldContain("Plugin component");
        exception.Message.ShouldContain("invalid plugin ID");
        exception.Message.ShouldNotBe(GoogleAnalyticsTestData.ConfigurationError);
    }

    [Fact]
    public void Given_NoManifestCascade_When_OnParametersSet_Then_It_Should_Render_Nothing()
    {
        // Arrange
        using var context = new BunitContext();

        // Act
        var cut = context.Render<GoogleAnalyticsComponent>(parameters => parameters.Add(p => p.Id, "google-analytics"));

        // Assert
        cut.Markup.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_AbsentManifests_When_OnParametersSet_Then_It_Should_Render_Nothing(bool nullCollection)
    {
        // Arrange
        using var context = new BunitContext();

        // Act
        var cut = context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, nullCollection ? null : Array.Empty<PluginManifest>()));

        // Assert
        cut.Markup.ShouldBeEmpty();
    }

    [Fact]
    public void Given_UpdatedManifestAndContext_When_OnParametersSet_Then_It_Should_Recompute_Configuration()
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, new[] { GoogleAnalyticsTestData.CreateManifest("G-INITIAL") }));

        // Act
        cut.Render(parameters => parameters
            .Add(p => p.Plugins, new[] { GoogleAnalyticsTestData.CreateManifest("G-UPDATED") })
            .Add(p => p.Site, new SiteManifest { IsPreview = true, BaseUrl = "/updated", Locale = "ko-KR" })
            .Add(p => p.Document, new ContentDocument { Metadata = new ContentMetadata { Title = "Updated" } }));

        // Assert
        GoogleAnalyticsTestData.AssertTag(cut.FindAll("script"), "G-UPDATED");
        cut.Markup.ShouldNotContain("G-INITIAL");
    }

    [Fact]
    public void Given_SelectionChanges_When_OnParametersSet_Then_It_Should_Reselect_Clear_And_Readd_Output()
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<AnalyticsHost>(parameters => parameters.Add(p => p.Plugins, new[]
        {
            GoogleAnalyticsTestData.CreateManifest("G-INITIAL", name: "Shared"),
            GoogleAnalyticsTestData.CreateManifest("G-OTHER", "other-analytics", "Shared"),
        }));

        // Act
        cut.Render(parameters => parameters.Add(p => p.Id, "other-analytics"));
        var reselected = cut.Markup;
        cut.Render(parameters => parameters.Add(p => p.Id, "absent"));
        var removed = cut.Markup;
        cut.Render(parameters => parameters.Add(p => p.Id, "google-analytics"));

        // Assert
        reselected.ShouldContain("gtag('config', 'G-OTHER');");
        reselected.ShouldNotContain("G-INITIAL");
        removed.ShouldBeEmpty();
        cut.Markup.ShouldContain("gtag('config', 'G-INITIAL');");
        cut.Markup.ShouldNotContain("G-OTHER");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_ManifestRemovedAndReadded_When_OnParametersSet_Then_It_Should_Clear_And_Refresh_Output(bool nullCollection)
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, new[] { GoogleAnalyticsTestData.CreateManifest("G-INITIAL") }));

        // Act
        cut.Render(parameters => parameters.Add(p => p.Plugins, nullCollection ? null : Array.Empty<PluginManifest>()));
        var removed = cut.Markup;
        cut.Render(parameters => parameters.Add(p => p.Plugins, new[] { GoogleAnalyticsTestData.CreateManifest("G-READDED") }));

        // Assert
        removed.ShouldBeEmpty();
        GoogleAnalyticsTestData.AssertTag(cut.FindAll("script"), "G-READDED");
        cut.Markup.ShouldNotContain("G-INITIAL");
    }

    [Fact]
    public void Given_ValidManifestBecomesInvalid_When_OnParametersSet_Then_It_Should_Fail_Instead_Of_Reusing_The_Previous_Id()
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, new[] { GoogleAnalyticsTestData.CreateManifest("G-INITIAL") }));

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => cut.Render(parameters =>
            parameters.Add(p => p.Plugins, new[] { new PluginManifest { Id = "google-analytics", Options = null } })));

        // Assert
        exception.Message.ShouldBe(GoogleAnalyticsTestData.ConfigurationError);
    }

    [Theory]
    [InlineData("absent")]
    [InlineData("invalid-analytics")]
    public void Given_PreviousMeasurementId_When_OnParametersSet_Then_It_Should_Clear_Derived_State_Before_Absence_Or_Validation_Failure(string id)
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<InspectableGoogleAnalyticsComponent>(parameters => parameters
            .Add(p => p.Id, "google-analytics")
            .AddCascadingValue<IEnumerable<PluginManifest>>(new[]
            {
                GoogleAnalyticsTestData.CreateManifest("G-INITIAL"),
                new PluginManifest { Id = "invalid-analytics", Options = null },
            }));
        var previous = cut.Instance.CurrentMeasurementId;

        // Act
        if (id == "invalid-analytics")
        {
            Should.Throw<InvalidOperationException>(() => cut.Render(parameters => parameters.Add(p => p.Id, id)));
        }
        else
        {
            cut.Render(parameters => parameters.Add(p => p.Id, id));
        }

        // Assert
        previous.ShouldBe("G-INITIAL");
        cut.Instance.CurrentMeasurementId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "/blog")]
    [InlineData(true, "/blog")]
    public void Given_PublicationContext_When_OnParametersSet_Then_It_Should_Keep_Configured_External_Tag(bool isPreview, string baseUrl)
    {
        // Arrange
        using var context = new BunitContext();
        var site = new SiteManifest { IsPreview = isPreview, BaseUrl = baseUrl, SiteUrl = "https://example.test", Locale = "ko-KR" };

        // Act
        var cut = context.Render<AnalyticsHost>(parameters => parameters
            .Add(p => p.Plugins, new[] { GoogleAnalyticsTestData.CreateManifest("G-EXAMPLE") })
            .Add(p => p.Site, site));

        // Assert
        GoogleAnalyticsTestData.AssertTag(cut.FindAll("script"), "G-EXAMPLE");
        cut.Markup.ShouldNotContain("example.test");
        cut.Markup.ShouldNotContain("/blog");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_ReadOnlyOptionsWithNestedValues_When_OnParametersSet_Then_It_Should_Preserve_Caller_Ownership(bool invalid)
    {
        // Arrange
        using var context = new BunitContext();
        var snapshot = new GoogleAnalyticsTestData.OptionsSnapshot(invalid);

        // Act
        if (invalid)
        {
            Should.Throw<InvalidOperationException>(() => context.Render<AnalyticsHost>(
                parameters => parameters.Add(p => p.Plugins, new[] { snapshot.Manifest })));
        }
        else
        {
            context.Render<AnalyticsHost>(parameters => parameters.Add(p => p.Plugins, new[] { snapshot.Manifest }));
        }

        // Assert
        snapshot.AssertUnchanged();
    }

    [Fact]
    public void Given_SourceOptionsChangedAfterManifestCreation_When_OnParametersSet_Then_It_Should_Use_Upstream_Snapshot()
    {
        // Arrange
        using var context = new BunitContext();
        var source = new Dictionary<string, object?> { ["MeasurementId"] = "G-ORIGINAL" };
        var manifest = new PluginManifest { Id = "google-analytics", Options = source };
        source["MeasurementId"] = "G-MUTATED";

        // Act
        var cut = context.Render<AnalyticsHost>(parameters => parameters.Add(p => p.Plugins, new[] { manifest }));

        // Assert
        cut.Markup.ShouldContain("gtag('config', 'G-ORIGINAL');");
        cut.Markup.ShouldNotContain("G-MUTATED");
        source["MeasurementId"].ShouldBe("G-MUTATED");
    }

    public sealed class InspectableGoogleAnalyticsComponent : GoogleAnalyticsComponent
    {
        public string? CurrentMeasurementId => MeasurementId;
    }

    public sealed class AnalyticsHost : ComponentBase
    {
        [Parameter]
        public IEnumerable<PluginManifest>? Plugins { get; set; }

        [Parameter]
        public SiteManifest Site { get; set; } = new();

        [Parameter]
        public ContentDocument Document { get; set; } = new();

        [Parameter]
        public string Id { get; set; } = "google-analytics";

        [Parameter]
        public string? Name { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<SiteManifest>>(0);
            builder.AddComponentParameter(1, "Value", Site);
            builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(siteBuilder =>
            {
                siteBuilder.OpenComponent<CascadingValue<ContentDocument>>(0);
                siteBuilder.AddComponentParameter(1, "Value", Document);
                siteBuilder.AddComponentParameter(2, "ChildContent", (RenderFragment)(documentBuilder =>
                {
                    documentBuilder.OpenComponent<CascadingValue<IEnumerable<PluginManifest>>>(0);
                    documentBuilder.AddComponentParameter(1, "Value", Plugins);
                    documentBuilder.AddComponentParameter(2, "ChildContent", (RenderFragment)(pluginBuilder =>
                    {
                        pluginBuilder.OpenComponent<GoogleAnalyticsComponent>(0);
                        pluginBuilder.AddComponentParameter(1, "Id", Id);
                        pluginBuilder.AddComponentParameter(2, "Name", Name);
                        pluginBuilder.CloseComponent();
                    }));
                    documentBuilder.CloseComponent();
                }));
                siteBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        }
    }
}
