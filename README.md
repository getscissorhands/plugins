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

Open `http://localhost:5000` and inspect page source. Open Graph and Google
Analytics are enabled; analytics uses the fake ID `G-EXAMPLE`. A fake ID does
not prevent browser requests to Google. The sample guide includes the alternate
post-HTML hook mode (`--use-placeholders`), static generation and
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

@justinyoo owns release authorization. Plugin version numbers and release timing
are independent of the upstream engine; releases remain preview for now.
Compatibility claims cover verified plugin/engine combinations, not matching
version labels or every version permitted by floating dependencies.

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
versions so partial publication can be retried with the same verified artifacts;
the two registries are not an atomic transaction. GitHub release creation follows
successful publishing steps and marks hyphenated package versions as prereleases.

Configuring the workflow does not configure the external trusted-publishing
policy, publish a package, or authorize pushing a release tag.

## Support and recovery

Support is best-effort through [GitHub Issues](https://github.com/getscissorhands/plugins/issues),
triaged by @justinyoo. There is no guaranteed response time or commitment to
maintain every historical preview.

Fix defective code in a new preview version; do not replace a published package.
Consumers may temporarily pin a previously verified plugin/engine combination.
There is no automatic rollback.

For partial publication, inspect the failed run and each destination's results.
Reuse its original verified package/symbol artifacts to finish the missing
publication steps, allowing the workflow's duplicate skipping to preserve
existing versions. The release job downloads the build job's `artifacts` output;
do not blindly rerun the build or rebuild different bytes under the same version.
If the original artifacts cannot be recovered, stop and resolve that gap with
@justinyoo rather than assume a rebuild is identical. Record artifact identity
and destination results before declaring recovery complete.

With @justinyoo's explicit approval for the affected version, deprecate or unlist
a faulty preview where supported and point users to its replacement. This does
not remove installed copies. Agreement to this policy is not authorization for
a particular publication, deprecation or unlisting.

### Optional deferrals

Implement agreed plugin behavior and obtain required release evidence rather
than defer them; preview status does not waive those obligations. Only optional
extras may wait, with an issue recording the item, why it is nonblocking and
@justinyoo as owner. No specific item is deferred by this policy, and there is no
need to create a deferral list when none is needed. See the
[catalog release gates](PRD.md#3-release-expectations-and-question-routing).

## Issues?

If you find any issues, please [report them](../../issues).
