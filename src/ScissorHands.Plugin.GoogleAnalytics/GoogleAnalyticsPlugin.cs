using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.GoogleAnalytics;

/// <summary>
/// This represents the plugin entity for Google Analytics.
/// </summary>
public class GoogleAnalyticsPlugin : ContentPlugin
{
    private const string GOOGLE_ANALYTICS_SCRIPT = """
    <!-- Google tag (gtag.js) -->
    <script async src="https://www.googletagmanager.com/gtag/js?id={{MEASUREMENT_ID}}"></script>
    <script>
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    gtag('config', '{{MEASUREMENT_ID}}');
    </script>
    """;

    private const string PLACEHOLDER = "<plugin:google-analytics></plugin:google-analytics>";

    /// <inheritdoc />
    public override string Name => "Google Analytics";

    /// <inheritdoc />
    public override async Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest manifest, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var measurementId = manifest.Options!.TryGetValue("MeasurementId", out var id)
                                ? id as string
                                : default;
        if (measurementId is null)
        {
            return html;
        }

        var script = GOOGLE_ANALYTICS_SCRIPT.Replace("{{MEASUREMENT_ID}}", measurementId);

        html = html.Replace(PLACEHOLDER, $"\n{script}\n", StringComparison.OrdinalIgnoreCase);

        return await Task.FromResult(html);
    }
}
