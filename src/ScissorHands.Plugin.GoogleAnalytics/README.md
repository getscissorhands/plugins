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

The hook replaces every paired marker case-insensitively and leaves unmarked HTML unchanged when configuration is valid. It does not deduplicate existing tags. The component refreshes its configuration when cascading context or selection changes and renders nothing when its selected manifest is absent.

## Configuration and breaking migration

An enabled manifest requires a string `MeasurementId`: `G-` followed by one or more uppercase ASCII letters or digits, without whitespace. `G-EXAMPLE` is a supported synthetic value; syntax validation does not verify a Google property.

Both paths throw `InvalidOperationException` for missing or invalid identifiers, including null/non-string values and whitespace-padded input. The error identifies the plugin and option without exposing the supplied value. The hook validates even when no paired marker is present.

**Breaking change:** earlier versions accepted arbitrary strings and emitted an empty ID for missing or non-string values. Before upgrading, supply a supported identifier or remove the `google-analytics` manifest. Clearing the value or setting an unrelated `Enabled` option does not disable this plugin.

## Preview and privacy

Configured output is retained in preview and production. Generation performs no provider request, but browsing generated pages can contact Google even with the fake ID. The plugin does not manage consent, suppress preview tracking, or provide provider retention/deletion controls. Remove its entry from `Plugins` to disable output.

See the [technical requirements and evidence](https://github.com/getscissorhands/plugins/blob/main/src/ScissorHands.Plugin.GoogleAnalytics/TRD.md) for validation, output handling and verification details.

## Support

See the shared [support and recovery policy](https://github.com/getscissorhands/plugins#support-and-recovery) for best-effort issue support and preview-release recovery.
