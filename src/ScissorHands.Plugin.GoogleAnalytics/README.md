# ScissorHands.NET: Google Analytics Plugin

Adds [Google Analytics](https://analytics.google.com) tag markup to a compatible ScissorHands.NET site.

## Getting Started

Install the preview package in your ScissorHands.NET host:

```bash
dotnet add package ScissorHands.Plugin.GoogleAnalytics --prerelease
```

Add this entry to the `Plugins` array in `appsettings.json`. Replace `G-EXAMPLE` with your Google Analytics measurement ID for real tracking:

```json
{
  "Plugins": [
    {
      "Id": "google-analytics",
      "Options": { "MeasurementId": "G-EXAMPLE" }
    }
  ]
}
```

In a layout supplied with the engine's cascading context, place the component just after the opening `<head>` tag:

```razor
<GoogleAnalyticsComponent Id="google-analytics" />
```

Alternatively, use the paired placeholder for the post-HTML hook:

```html
<plugin:google-analytics></plugin:google-analytics>
```

Choose one path per insertion to avoid duplicates. Hook placeholders must be paired, not self-closing. The `Id` is exact and case-sensitive; an optional manifest `Name` is only a display label.

**Privacy:** browsing generated pages can contact Google, including during preview or with the fake ID. The plugin does not manage consent or suppress preview tracking. Remove its entry from `Plugins` to disable output; clearing `MeasurementId` is not a disable switch.

## Support

See the shared [support and recovery policy](https://github.com/getscissorhands/plugins#support-and-recovery) for best-effort issue support and preview-release recovery.
