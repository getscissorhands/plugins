# ScissorHands Plugins

Collection of the official plugins for ScissorHands.NET

## List of Plugins

| Name                                                                    | Description             |
|-------------------------------------------------------------------------|-------------------------|
| [Google Analytics](./src/ScissorHands.Plugin.GoogleAnalytics/README.md) | Google Analytics plugin |
| [Open Graph](./src/ScissorHands.Plugin.OpenGraph/README.md)             | Open Graph plugin       |

## Build Your Plugin

1. Set environment variables for GitHub NuGet Package Registry.

    ```bash
    # zsh/bash
    source ./scripts/setup-gh-auth.sh --username "<GITHUB_USERNAME>" --token "<GITHUB_TOKEN>"
    ```

    ```powershell
    # PowerShell
    . ./scripts/setup-gh-auth.ps1 -Username "<GITHUB_USERNAME>" -Token "<GITHUB_TOKEN>"
    ```

   > **NOTE**: Make sure to **sourcing** the script instead of executing it.

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
    
        public override async Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            // ADD LOGIC HERE
        }
    
        public override async Task<ContentDocument> PostMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            // ADD LOGIC HERE
        }
    
        public override async Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            // ADD LOGIC HERE
        }
    }
    ```

## Issues?

If you find any issues, please [report them](../../issues).
