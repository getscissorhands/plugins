# ScissorHands Plugins

Collection of the official plugins for ScissorHands.NET

## List of Plugins

| Name                                                                    | Description                    |
|-------------------------------------------------------------------------|--------------------------------|
| [Google Analytics](./src/ScissorHands.Plugin.GoogleAnalytics/README.md) | Google Analytics script plugin |

## Build Your Plugin

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

1. Create a class library.

    ```bash
    dotnet new classlib -n MyAwesomeScissorHandsPlugin
    ```

1. Add a NuGet package.

    ```bash
    dotnet add package ScissorHands.Plugin --prerelease
    ```

1. Create a plugin class inheriting the `ContentPlugin` class.

    ```csharp
    public class MyAwesomeScissorHandsPlugin : ContentPlugin
    {
        public override string Name => "My Awesome ScissorHands Plugin";
    
        public override async Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest manifest, CancellationToken cancellationToken = default)
        {
            // ADD LOGIC HERE
        }
    
        public override async Task<ContentDocument> PostMarkdownAsync(ContentDocument document, PluginManifest manifest, CancellationToken cancellationToken = default)
        {
            // ADD LOGIC HERE
        }
    
        public override async Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest manifest, CancellationToken cancellationToken = default)
        {
            // ADD LOGIC HERE
        }
    }
    ```

## Issues?

If you find any issues, please [report them](../../issues).
