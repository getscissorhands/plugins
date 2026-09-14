# ScissorHands Plugins

Official plugins for ScissorHands.NET. Packages are currently preview releases, versioned independently from the engine.

## List of Plugins

| Plugin | Purpose |
| --- | --- |
| [Google Analytics](src/ScissorHands.Plugin.GoogleAnalytics/README.md) | Add Google tag markup |
| [Open Graph](src/ScissorHands.Plugin.OpenGraph/README.md) | Add Open Graph and Twitter-card metadata |

## Local preview

Use the SDK selected by [global.json](global.json). From the repository root:

```powershell
dotnet build ./ScissorHandsPlugins.sln -c Release
Set-Location sample
dotnet run -c Release --no-build -- --preview
```

The sample uses locally built plugins and the engine's built-in theme. Open `http://localhost:5000` and inspect page source; stop preview with Ctrl+C.

**Analytics is enabled with fake ID `G-EXAMPLE`; browsing can still contact Google.** See the [sample guide](sample/README.md) to disable analytics, generate files without browsing, or compare component and hook modes.

## Build Your Plugin

Start with [AGENTS.md](AGENTS.md) for repository conventions and build/test commands. The [catalog PRD](PRD.md) defines product scope; the [catalog TRD](TRD.md) links API contracts and each plugin's technical requirements. Use the existing plugins as examples rather than duplicating engine services.

## Publishing packages

Only push a release tag with @justinyoo's explicit approval and after meeting the [release gates](PRD.md#3-release-expectations-and-question-routing). The [workflow](.github/workflows/main.yaml) uses a `v*` tag's version to publish to NuGet.org and GitHub Packages, then creates a GitHub release after successful publication. Branch and pull-request builds do not publish. Use a preview tag/version while the packages remain preview; prerelease dependencies also require a prerelease package version.

NuGet.org uses [OIDC trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing), not a stored long-lived API key. Verify the following setup before publishing:

1. Configure the GitHub environment `nuget-release` with appropriate protection rules and permission to deploy the intended release tags.
2. Set the `NUGET_USER` secret in that environment or the repository to the NuGet.org profile username (not an email address).
3. In the publishing account's NuGet.org trusted-publishing settings, authorize repository owner `getscissorhands`, repository `plugins`, workflow filename **`main.yaml`** (not its full path), and environment `nuget-release`. Scope the policy to the plugin package IDs and allow new packages as well as new versions if needed for the first publication. The selected NuGet owner must have permission to publish those package IDs.

Workflow configuration alone does not configure external trust or prove publishing succeeds. Technical details and setup evidence are in [T-008](TRD.md#t-008-package-and-consumer-documentation).

## Support and recovery

Support is best-effort through [GitHub Issues](https://github.com/getscissorhands/plugins/issues), triaged by @justinyoo. There is no guaranteed response time or commitment to maintain every historical preview.

Fix defective code in a new preview version; do not replace a published package. Consumers may temporarily pin a previously verified plugin/engine combination. There is no automatic rollback.

For partial publication, inspect the failed run and both destinations. Retry the missing steps with the same verified package/symbol artifacts, skipping existing versions; do not rebuild different bytes under the same version. If the original artifacts are unavailable, stop and resolve the gap with @justinyoo. Record artifact identity and destination results before declaring recovery complete.

Deprecate or unlist a faulty version only with @justinyoo's explicit approval, and point users to its replacement. This does not remove installed copies.

### Optional deferrals

Only optional extras may be deferred, with an issue recording the rationale and @justinyoo as owner. Required behavior and release evidence are not optional; see the [catalog policy](PRD.md#support-recovery-and-deferrals).
