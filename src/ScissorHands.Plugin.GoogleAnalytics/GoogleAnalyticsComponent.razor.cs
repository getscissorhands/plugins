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

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (Plugin is null)
        {
            return;
        }

        if (Plugin.Options?.ContainsKey("MeasurementId") != true)
        {
            return;
        }

        MeasurementId = Plugin.Options?["MeasurementId"] is string measurementIdValue
                            ? measurementIdValue
                            : default;
    }
}