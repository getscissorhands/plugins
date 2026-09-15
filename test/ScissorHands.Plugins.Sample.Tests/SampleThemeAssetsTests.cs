using System.IO.Abstractions;

using Microsoft.Extensions.Logging.Abstractions;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Services;

namespace ScissorHands.Plugins.Sample.Tests;

public class SampleThemeAssetsTests
{
    [Fact]
    public async Task Given_BundledTheme_When_Loaded_Then_It_Should_Contain_ManifestAndAssets()
    {
        // Arrange
        var paths = Substitute.For<IAppPaths>();
        paths.GetThemesRoot().Returns(Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N")));
        var service = new ThemeService(paths, new FileSystem(), new SiteManifest(), NullLogger<ThemeService>.Instance);
        var themeRoot = Path.Combine(AppContext.BaseDirectory, "themes", "default");

        // Act
        var theme = await service.LoadManifestAsync("default", Xunit.TestContext.Current.CancellationToken);

        // Assert
        theme.Slug.ShouldBe("default");
        theme.Stylesheets.ShouldContain("/assets/theme.css");
        theme.Scripts.ShouldContain("/assets/theme.js");
        foreach (var path in new[]
        {
            "theme.json",
            "favicon.ico",
            Path.Combine("assets", "theme.css"),
            Path.Combine("assets", "theme.js"),
            Path.Combine("assets", "THIRD-PARTY-NOTICES.md"),
        })
        {
            var asset = new FileInfo(Path.Combine(themeRoot, path));
            asset.Exists.ShouldBeTrue($"The bundled theme must contain {path}.");
            asset.Length.ShouldBeGreaterThan(0L);
        }
    }
}
