# ScissorHands Plugins

Collection of the official plugins for ScissorHands.NET

## List of Plugins

| Name                                                                    | Description             |
|-------------------------------------------------------------------------|-------------------------|
| [Google Analytics](./src/ScissorHands.Plugin.GoogleAnalytics/README.md) | Google Analytics plugin |
| [Open Graph](./src/ScissorHands.Plugin.OpenGraph/README.md)             | Open Graph plugin       |

## Local preview

The [sample application](sample/README.md) previews the plugins from local
project references using the NuGet.org engine and its packaged default-theme
CSS/JavaScript. From the repository root:

```powershell
dotnet build .\ScissorHandsPlugins.sln -c Release
Set-Location sample
dotnet run -c Release --no-build -- --preview
```

Open `http://localhost:5000` and inspect page source. Open Graph is enabled by
default; Google Analytics is opt-in. The sample guide includes the alternate
post-HTML hook mode (`--Sample:UsePlaceholders=true`), static generation and
configuration details. No package publishing or theme symlink is needed.

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

## Publishing packages

The [release workflow](.github/workflows/main.yaml) builds and tests on Windows,
macOS and Linux. A `v*` tag packages the plugins on Linux; the release job then
publishes to NuGet.org and GitHub Packages and creates a GitHub release. Branch
and pull-request builds do not publish. The tag supplies the package version,
so a prerelease dependency requires an appropriate prerelease tag/version.

NuGet.org uses [trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
with `NuGet/login@v1`, not a stored long-lived API key. Before the first release:

1. Configure the GitHub environment `nuget-release` with appropriate protection
   rules and permission to deploy the intended release tags.
2. Set the `NUGET_USER` secret in that environment or the repository to the
   NuGet.org profile username (not an email address).
3. In the publishing account's NuGet.org trusted-publishing settings, authorize
   repository owner `getscissorhands`, repository `plugins`, workflow filename
   **`main.yaml`** (not its full path), and environment `nuget-release`. Scope
   the policy to the plugin package IDs and allow new packages as well as new
   versions if needed for the first publication. The selected NuGet owner must
   have permission to publish those package IDs.

Only the release job requests OIDC tokens. It uses the short-lived key for
packages and their adjacent `.snupkg` symbols on NuGet.org. GitHub Packages uses
`GITHUB_TOKEN` with symbol upload disabled. Both destinations skip existing
versions so partial runs can be retried; the two registries are not an atomic
transaction. GitHub release creation follows successful publishing steps and
marks hyphenated package versions as prereleases.

Configuring the workflow does not configure the external trusted-publishing
policy, publish a package, or authorize pushing a release tag.

## Issues?

If you find any issues, please [report them](../../issues).
