# ScissorHands Plugins

Collection of the official plugins for ScissorHands.NET

## List of Plugins

| Name                                                                    | Description             |
|-------------------------------------------------------------------------|-------------------------|
| [Google Analytics](./src/ScissorHands.Plugin.GoogleAnalytics/README.md) | Google Analytics plugin |
| [Open Graph](./src/ScissorHands.Plugin.OpenGraph/README.md)             | Open Graph plugin       |

## Build Your Plugin

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
        public override string Id => "my-awesome-plugin";

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

   Plugin IDs are stable, lowercase kebab-case identifiers used by configuration, dependencies, and Razor components. Display names are optional labels and are not used for matching.

## Issues?

If you find any issues, please [report them](../../issues).
