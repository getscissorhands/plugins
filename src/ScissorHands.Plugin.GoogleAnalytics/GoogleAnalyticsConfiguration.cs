using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace ScissorHands.Plugin.GoogleAnalytics;

internal static partial class GoogleAnalyticsConfiguration
{
    internal static string GetMeasurementId(IReadOnlyDictionary<string, object?>? options)
    {
        if (options is null ||
            !options.TryGetValue("MeasurementId", out var value) ||
            value is not string measurementId ||
            !MeasurementIdPattern().IsMatch(measurementId))
        {
            throw new InvalidOperationException(
                "Plugin 'google-analytics' requires option 'MeasurementId' to be a string containing " +
                "'G-' followed by one or more uppercase ASCII letters or digits, with no whitespace or other characters.");
        }

        return measurementId;
    }

    internal static string GetLoaderUrl(string measurementId) =>
        $"https://www.googletagmanager.com/gtag/js?id={Uri.EscapeDataString(measurementId)}";

    // JavaScript string content is not HTML text. The default encoder also escapes HTML-sensitive characters.
    internal static string GetJavaScriptMeasurementId(string measurementId) =>
        JavaScriptEncoder.Default.Encode(measurementId);

    [GeneratedRegex(@"\AG-[A-Z0-9]+\z", RegexOptions.CultureInvariant)]
    private static partial Regex MeasurementIdPattern();
}
