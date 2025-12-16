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

1. Add the placeholder, `<plugin:google-analytics />`, to `MainLayout.razor`.

    ```html
        <!-- Add placeholder below -->
        <plugin:google-analytics />
    </body>
    </html>
    ```
