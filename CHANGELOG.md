# Changelog

Notable plugin and repository changes are recorded here, using the
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

Plugins are currently preview releases and are versioned independently from the
ScissorHands.NET engine. Release dates below use GitHub publication dates in UTC.
Older history is available in [GitHub Releases][releases].

## [Unreleased]

## [1.0.0-preview.20260928.1] - 2026-09-28

Published both plugins for the ScissorHands.NET `1.0.0-preview.20260928.1`
engine after [PR #20][theme-images-pr]. Plugin and engine versions remain
independent.

### Changed

- Clarified the previous release's compatibility, installation examples and
  post-release verification evidence in [PR #19][compatibility-pr]; that
  documentation change did not alter runtime or dependency policy.
- **Breaking consumer migration:** Open Graph now reads its site-wide image
  fallback from the first `Theme.HeroImages` entry instead of the removed
  `Site.HeroImage` setting. Document images still take precedence; with no
  image, both image tags are omitted. Move site images to `Theme.HeroImages`,
  forward `ThemeSettings` in custom layouts, and pass the effective settings
  when constructing the hook plugin manually. The previous Open Graph binary
  is not compatible with the new engine's removed image API.
- Refreshed both plugins and the sample against engine
  `1.0.0-preview.20260928.1` without changing central version ranges or
  Google Analytics markup. The sample now forwards theme settings in both
  insertion modes.

The [release workflow][theme-images-release-run] completed its build/test
matrix, package publication and GitHub release creation. The
[source verification](TRD.md#nuget-refresh-evidence-2026-09-28) covers 425
tests and both sample modes; no separate NuGet-only consumer or browser/provider
acceptance is recorded for this release.

## [1.0.0-preview.20260927.1] - 2026-09-26

Published Google Analytics and Open Graph packages for the verified
ScissorHands.NET `1.0.0-preview.20260927.1` combination, including the migration
from [PR #18][localization-pr]. Plugin and engine version policies remain independent.

### Added

- Contributor, code-of-conduct, security-reporting, and support guides.
- Default code ownership by `@justinyoo`, GitHub sponsorship configuration, and
  Git attributes for text, line endings, and binary files.
- Weekly Dependabot updates for GitHub Actions only, preserving the existing
  NuGet major-version floating dependency policy.
- This changelog and its README entry point.
- Localization regressions covering disabled/enabled sites, actual translations
  and primary-content fallbacks, generated routes, and preservation of
  engine-required notices and preview publication badges.

### Changed

- Migrate both official plugin sources, their fixtures and the sample to
  ScissorHands.NET `1.0.0-preview.20260927.1`, retaining central major-version
  floats and independent plugin release versions.
- **Breaking consumer migration:** replace removed `Site.Locale`/`UseLocaleInUrl` and
  frontmatter `locale` with ordered `Site.Locales` and directory-based
  translations; supply complete application `Theme.Localization` messages.
  Custom layouts must forward locale context and preserve required notices and
  publication badges. See the [compatibility guide](README.md#engine-compatibility).
- Replaced legacy Markdown bug and feature templates with project-specific YAML
  issue forms.
- Updated README, issue-chooser, and pull-request guidance to link the community
  policies and existing .NET/MTP contribution workflow.

### Fixed

- Open Graph now uses actual content language and resolved render routes, omits
  `og:locale` when localization is disabled, and leaves theme-owned
  canonical/alternate-language metadata unchanged. Analytics retains its
  existing manifest enablement and preview behavior.

### Migration

Upgrade both plugins as well as the engine. Open Graph
`1.0.0-preview.20260915.1` can compile in a host using the new engine but fails
during generation when it calls the removed `SiteManifest.get_Locale()` API.
The published `1.0.0-preview.20260927.1` binary resolves that incompatibility.
Follow the [current migration guidance](README.md#engine-compatibility) for
ordered locales, application messages and custom-layout context.

The [release workflow][localization-release-run] completed NuGet.org OIDC
publication, GitHub Packages publication and GitHub release creation. A fresh
NuGet-only consumer verified the published binaries in both component/hook build
and preview modes at root and subpath URLs. See the
[post-release evidence](TRD.md#published-release-verification-2026-09-27);
browser/provider acceptance and future release authorization remain separate.

## [1.0.0-preview.20260915.1] - 2026-09-15

Published changes from [PR #10][migration-pr].

### Added

- A local-plugin sample with component and paired-placeholder modes, the
  engine's built-in theme assets, and synthetic analytics configuration.
- Catalog and per-plugin product/technical requirements, migration guides, and
  documented compatibility and integration evidence.
- Regression coverage for configuration failures, immutable inputs, component
  updates, cancellation, output encoding, and generated-page metadata.

### Changed

- **Breaking:** migrated both plugins to published ScissorHands.NET vNext
  contracts. Manifest and component selection use exact plugin IDs
  (`google-analytics`, `open-graph`), not display names.
- **Breaking:** `google-analytics` now rejects missing or malformed
  `MeasurementId` values instead of emitting empty or arbitrary identifiers.
- **Breaking:** `open-graph` now requires valid publication context and supported
  image references, treats metadata as text, omits unavailable image tags, and
  restricts creator metadata to individual source-backed posts.
- Centralized .NET 10 build and test configuration, adopted
  `ScissorHandsPlugins.slnx` and xUnit v3/Microsoft.Testing.Platform, and retained
  central major-version floating package ranges.
- Configured tag-only NuGet.org OIDC trusted publishing while retaining GitHub
  Packages publishing and subsequent GitHub release creation.

### Fixed

- Aligned Open Graph metadata values and optional-field presence across hook and
  component integrations for equivalent context.
- Preserved site subpaths and engine-escaped generated tag routes in canonical
  URLs with engine `1.0.0-preview.20260915.1`, resolving OG-Q-005 without
  reconstructing routes.

### Migration

Rebuild against compatible vNext packages and configure the exact plugin IDs.
For analytics, supply `G-` followed by one or more uppercase ASCII letters or
digits, or remove the manifest to disable output. For Open Graph, supply valid HTTP(S)
publication context, correct unsupported image references, and account for
omitted image/creator tags. Supply metadata as original text rather than markup
or pre-encoded entities. See the release's [analytics migration guide][ga-migration]
and [Open Graph migration guide][og-migration] for details.

For generated tag-page component URLs, refresh cached engine dependencies to
the verified `1.0.0-preview.20260915.1` release and rebuild. Custom layouts must
forward `Document` through `CascadingMainLayoutBase`; older
`1.0.0-preview.20260914.1` hosts still require hook mode for those URLs.
Explicit test targets use MTP's `--project` or `--solution` selectors.

Configured preview output remains enabled. Browsing the sample can still contact
Google even with its fake analytics ID; this release does not add consent
management or automatic preview suppression.

[Unreleased]: https://github.com/getscissorhands/plugins/compare/v1.0.0-preview.20260928.1...HEAD
[1.0.0-preview.20260928.1]: https://github.com/getscissorhands/plugins/releases/tag/v1.0.0-preview.20260928.1
[1.0.0-preview.20260927.1]: https://github.com/getscissorhands/plugins/releases/tag/v1.0.0-preview.20260927.1
[1.0.0-preview.20260915.1]: https://github.com/getscissorhands/plugins/releases/tag/v1.0.0-preview.20260915.1
[releases]: https://github.com/getscissorhands/plugins/releases
[migration-pr]: https://github.com/getscissorhands/plugins/pull/10
[compatibility-pr]: https://github.com/getscissorhands/plugins/pull/19
[theme-images-pr]: https://github.com/getscissorhands/plugins/pull/20
[theme-images-release-run]: https://github.com/getscissorhands/plugins/actions/runs/36477251269
[localization-pr]: https://github.com/getscissorhands/plugins/pull/18
[localization-release-run]: https://github.com/getscissorhands/plugins/actions/runs/36280943492
[ga-migration]: https://github.com/getscissorhands/plugins/blob/v1.0.0-preview.20260915.1/src/ScissorHands.Plugin.GoogleAnalytics/README.md#configuration-and-breaking-migration
[og-migration]: https://github.com/getscissorhands/plugins/blob/v1.0.0-preview.20260915.1/src/ScissorHands.Plugin.OpenGraph/README.md#breaking-migration-from-the-earlier-permissive-behavior
