# Changelog

Notable plugin and repository changes are recorded here, using the
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

Plugins are currently preview releases and are versioned independently from the
ScissorHands.NET engine. Release dates below use GitHub publication dates in UTC.
Older history is available in [GitHub Releases][releases].

## [Unreleased]

These changes are not part of the latest published release.

### Added

- Contributor, code-of-conduct, security-reporting, and support guides.
- Default code ownership by `@justinyoo`, GitHub sponsorship configuration, and
  Git attributes for text, line endings, and binary files.
- Weekly Dependabot updates for GitHub Actions only, preserving the existing
  NuGet major-version floating dependency policy.
- This changelog and its README entry point.

### Changed

- Replaced legacy Markdown bug and feature templates with project-specific YAML
  issue forms.
- Updated README, issue-chooser, and pull-request guidance to link the community
  policies and existing .NET/MTP contribution workflow.

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

[Unreleased]: https://github.com/getscissorhands/plugins/compare/v1.0.0-preview.20260915.1...HEAD
[1.0.0-preview.20260915.1]: https://github.com/getscissorhands/plugins/releases/tag/v1.0.0-preview.20260915.1
[releases]: https://github.com/getscissorhands/plugins/releases
[migration-pr]: https://github.com/getscissorhands/plugins/pull/10
[ga-migration]: https://github.com/getscissorhands/plugins/blob/v1.0.0-preview.20260915.1/src/ScissorHands.Plugin.GoogleAnalytics/README.md#configuration-and-breaking-migration
[og-migration]: https://github.com/getscissorhands/plugins/blob/v1.0.0-preview.20260915.1/src/ScissorHands.Plugin.OpenGraph/README.md#breaking-migration-from-the-earlier-permissive-behavior
