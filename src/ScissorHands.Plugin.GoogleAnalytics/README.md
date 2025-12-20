# ScissorHands.NET: Google Analytics Plugin

This plugin renders [Google Analytics](https://analytics.google.com) script.

## GitHub Nuget Package Registry

1. Set environment variables for GitHub NuGet Package Registry.

    ```bash
    # zsh/bash
    export GH_PACKAGE_USERNAME="<GITHUB_USERNAME>"
    export GH_PACKAGE_TOKEN="<GITHUB_TOKEN>"
    ```

    ```powershell
    # PowerShell
    $env:GH_PACKAGE_USERNAME = "<GITHUB_USERNAME>"
    $env:GH_PACKAGE_TOKEN = "<GITHUB_TOKEN>"
    ```

## Getting Started

1. Assuming that you've got a running [ScissorHands.NET](https://github.com/getscissorhands/Scissorhands.NET) app.
1. Update the plugin section of `appsettings.json` to add options. `MeasurementId` can be obtained from [Google Analytics](https://analytics.google.com) website.

    ```jsonc
    {
      ...
      "Plugins": [
        {
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
    <GoogleAnalyticsComponent Documents="@Documents" Document="@Document" Plugin="@GoogleAnalyticsPlugin" Theme="@Theme" Site="@Site" />

    @code {
        protected PluginManifest? GoogleAnalyticsPlugin { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
    
            GoogleAnalyticsPlugin = Plugins?.SingleOrDefault(p => p.Name!.Equals("Google Analytics", StringComparison.OrdinalIgnoreCase));
        }
    }    
    ```

   > **NOTE**: Those `@Documents`, `@Document`, `@Theme` and `@Site` values are inherited, and the `@GoogleAnalyticsPlugin` value is calculated from the `OnInitializedAsync()` method.

1. Alternatively, instead of the `<GoogleAnalyticsComponent />` component, add the placeholder, `<plugin:google-analytics />`, to `MainLayout.razor`. **It's strongly advised to place right after the opening `<head>` tag**.

    ```html
    <html>
    <head>
        <plugin:google-analytics />
        ...
    ```
