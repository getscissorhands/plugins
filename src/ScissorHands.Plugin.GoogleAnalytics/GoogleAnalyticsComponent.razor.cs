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

        if (Plugin.Options is null)
        {
            return;
        }

        MeasurementId = Plugin.Options.TryGetValue("MeasurementId", out var measurementIdValue) && measurementIdValue is string measurementIdString
                            ? measurementIdString
                            : default;
    }
}