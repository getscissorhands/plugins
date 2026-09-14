# ScissorHands.NET: Google Analytics Plugin

This plugin renders [Google Analytics](https://analytics.google.com) script.

## Getting Started

1. Assuming that you've got a running [ScissorHands.NET](https://github.com/getscissorhands/Scissorhands.NET) app.
1. Update the plugin section of `appsettings.json` to add options. `MeasurementId` can be obtained from [Google Analytics](https://analytics.google.com) website.

    ```jsonc
    {
      ...
      "Plugins": [
        {
          "Id": "google-analytics",
          "Name": "Google Analytics",
          "Options": {
            "MeasurementId": "G-XXXXXXXX"
          }
        }
      ]
    }
    ```

1. Add a NuGet package.

    ```bash
    dotnet add package ScissorHands.Plugin.GoogleAnalytics --prerelease
    ```

1. Add a UI component, `<GoogleAnalyticsComponent />` with parameters, to `MainLayout.razor`. **It's strongly advised to place right after the opening `<head>` tag.**

    ```razor
    <GoogleAnalyticsComponent Id="google-analytics" />
    ```

   > **NOTE**: The configured plugin manifest is resolved by its exact, case-sensitive `Id`. `Name` is optional display metadata. The current documents, document, theme and site are received from the inherited cascading values.

1. Alternatively, use the paired placeholder, `<plugin:google-analytics></plugin:google-analytics>` instead of the `<GoogleAnalyticsComponent />` component. **It's strongly advised to place right after the opening `<head>` tag**.

    ```html
    <html>
    <head>
        <plugin:google-analytics></plugin:google-analytics>
        ...
    ```

   Use paired markers for the hook path; self-closing hook markers are not part of the supported contract. Choose one insertion path per intended output to avoid duplicates.

## Support

See the shared [support and recovery policy](https://github.com/getscissorhands/plugins#support-and-recovery)
for best-effort issue support and preview-release recovery.
