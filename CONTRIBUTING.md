# Contributing to ScissorHands Plugins

This repository maintains the official ScissorHands.NET plugin packages and a
non-packable local validation sample. It consumes the upstream engine rather
than implementing generation, navigation, preview serving, or a production theme.

Start with [AGENTS.md](AGENTS.md) for repository conventions and the
[catalog PRD](PRD.md) and [catalog TRD](TRD.md) for shared requirements. Read the
owning plugin's adjacent PRD/TRD before changing its behavior. These local
documents remain authoritative; this guide provides the contributor entry point.

## Code of Conduct

This project adheres to the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md).
By participating, you are expected to uphold this code.

## Getting Started

Install Git and the .NET SDK selected by [global.json](global.json). The projects
target .NET 10 and use NuGet central package management and Microsoft.Testing.Platform
(MTP) with xUnit v3.

Fork the repository if you do not have write access, then clone your fork:

```powershell
git clone https://github.com/YOUR-USERNAME/plugins.git
Set-Location plugins
git switch -c feat/plugin-change
```

Start independent work from an up-to-date `main`. For manually created branches,
use `feat/` or `hotfix/` with a descriptive kebab-case name. In an app-managed
worktree, reuse its assigned branch and branch-management tools instead.

Run from the repository root:

```powershell
dotnet restore .\ScissorHandsPlugins.slnx
dotnet build .\ScissorHandsPlugins.slnx -c Release --no-restore -warnaserror
dotnet test -c Release --no-build --verbosity normal
```

For a focused run after building, select the affected test project:

```powershell
dotnet test --project .\test\ScissorHands.Plugin.OpenGraph.Tests\ScissorHands.Plugin.OpenGraph.Tests.csproj -c Release --no-build
```

Use MTP's `--project` or `--solution` selectors, not positional VSTest arguments.
Keep build/test configurations aligned and confirm that tests were discovered.
Run the affected suite first and the full suite for shared or cross-plugin changes.
Documentation-only changes need source/link review, not an unrelated build.

Keep the existing major-version floating dependencies in
[Directory.Packages.props](Directory.Packages.props). Dependency upgrades require
re-evaluating and recording the resolved graph as described in
[AGENTS.md](AGENTS.md#commands-and-validation); do not clear shared NuGet caches
or replace floats with pins as incidental cleanup. Dependabot proposes weekly
GitHub Actions updates only; it does not manage NuGet ranges.

## Making Changes

Follow [.editorconfig](.editorconfig) and nearby code. Add regression coverage for
each affected integration surface using the configured xUnit v3, Shouldly,
NSubstitute, and bUnit tools. Use `Given_..._When_..._Then_...` test names and
synthetic inputs without real credentials or external provider requests.

Public behavior or configuration changes require the affected package README
and local PRD/TRD to stay in sync. New plugins need their own adjacent PRD/TRD
and entries in both root catalogs; they do not inherit a sibling's approval.
Keep engine responsibilities and unresolved evidence gaps explicit.

Use the [sample guide](sample/README.md) for local integration. The sample enables
analytics with a fake identifier, but browsing can still contact Google. Prefer
static output for analytics checks, or remove its manifest in an isolated
configuration before browser checks. Exercise both component and paired-marker
modes when changing insertion behavior. Do not commit generated `bin`, `obj`,
test results, packages, `sample/preview`, `sample/dist`, or credentials.

## Pull Request Process

Keep each PR focused on one coherent outcome and normally target `main`. Use a
Conventional Commit-style title and complete every section of the
[PR template](.github/PULL_REQUEST_TEMPLATE.md), using `N/A` when appropriate.
Explain affected plugin IDs, breaking changes and migration, documentation
updates, actual validation results, and any skipped or blocked checks.

Reference related issues; use closing keywords only when the PR fully resolves
their scope. Use a draft PR for intentionally incomplete work. Follow up on
review feedback with focused commits and replies in the original threads.
Review and support are best-effort, without a guaranteed response time.

Plugin releases are preview and versioned independently from the engine.
Passing CI is not release authorization. Do not publish packages, push release
tags, or trigger publishing without @justinyoo's explicit approval. The
[release guide](README.md#publishing-packages) and
[release gates](PRD.md#3-release-expectations-and-question-routing) remain in force.

## Commit Convention

Use [Conventional Commits](https://www.conventionalcommits.org/) with a short
imperative description, for example:

- `fix(open-graph): preserve subpath metadata URLs`
- `build(packages): update test dependencies`
- `docs: clarify plugin authoring guidance`

Keep tightly coupled code, tests, and documentation in one logical commit.
Mark breaking changes with `!` or a `BREAKING CHANGE:` footer and explain the
consumer migration. Do not rewrite existing history without explicit approval.

## Reporting Bugs

Use the [bug report form](https://github.com/getscissorhands/plugins/issues/new?template=bug_report.yml).
Include the plugin ID, exact plugin/engine versions, integration mode, minimal
reproduction, and expected versus actual behavior. Redact secrets and private
site content.

Do not report vulnerabilities or conduct incidents in public issues. Follow
[SECURITY.md](SECURITY.md) or [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) instead.

## Requesting Features

Use the [feature request form](https://github.com/getscissorhands/plugins/issues/new?template=feature_request.yml).
Describe the affected plugin or proposed catalog addition, the user problem,
desired outcome, and alternatives. A request does not imply acceptance or a
release commitment. See [SUPPORT.md](SUPPORT.md) for support boundaries.
