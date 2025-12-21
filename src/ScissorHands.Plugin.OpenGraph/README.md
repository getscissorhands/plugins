# ScissorHands.NET: Open Graph Plugin

This plugin renders the [Open Graph](https://ogp.me/) tags.

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
1. Update the plugin section of `appsettings.json` to add options. `TwitterSiteId` is the Twitter handle for the website, and `TwitterCreatorId` is the default Twitter handle for the content authors.

    ```jsonc
    {
      ...
      "Plugins": [
        {
          "Name": "Open Graph",
          "Options": {
            "TwitterSiteId": "@your_twitter_handle_site",
            "TwitterCreatorId": "@your_twitter_handle_creator"
          }
        }
      ]
    }
    ```

   > **NOTE**: If you don't have any of both, you can omit the property. For example, you can omit both properties like `"Options": {}`.

1. Add a NuGet package.

    ```bash
    dotnet add package ScissorHands.Plugin.OpenGraph --prerelease
    ```

1. Add a UI component, `<OpenGraphComponent />` with parameters, to `MainLayout.razor`.

    ```razor
    <OpenGraphComponent Documents="@Documents" Document="@Document" Plugin="@OpenGraphPlugin" Theme="@Theme" Site="@Site" />

    @code {
        protected PluginManifest? OpenGraphPlugin { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
    
            OpenGraphPlugin = Plugins?.SingleOrDefault(p => p.Name!.Equals("Open Graph", StringComparison.OrdinalIgnoreCase));
        }
    }    
    ```

   > **NOTE**: Those `@Documents`, `@Document`, `@Theme` and `@Site` values are inherited, and the `@OpenGraphPlugin` value is calculated from the `OnInitializedAsync()` method.

1. Alternatively, use the placeholder, `<plugin:open-graph />` instead of the `<OpenGraphComponent />` component.

    ```html
    <html>
    <head>
        ...
        <plugin:open-graph />
        ...
    </head>
    ```
