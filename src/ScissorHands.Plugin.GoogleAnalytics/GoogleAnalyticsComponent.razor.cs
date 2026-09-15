namespace ScissorHands.Plugin.GoogleAnalytics;

/// <summary>
/// This represents the UI component entity for Google Analytics.
/// </summary>
public partial class GoogleAnalyticsComponent : PluginComponentBase
{
    /// <summary>
    /// Gets or sets the measurement ID.
    /// </summary>
    protected string? MeasurementId { get; set; }

    private string? LoaderUrl { get; set; }

    private string? JavaScriptMeasurementId { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        MeasurementId = default;
        LoaderUrl = default;
        JavaScriptMeasurementId = default;

        if (Plugin is null)
        {
            return;
        }

        MeasurementId = GoogleAnalyticsConfiguration.GetMeasurementId(Plugin.Options);
        LoaderUrl = GoogleAnalyticsConfiguration.GetLoaderUrl(MeasurementId);
        JavaScriptMeasurementId = GoogleAnalyticsConfiguration.GetJavaScriptMeasurementId(MeasurementId);
    }
}