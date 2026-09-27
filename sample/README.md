# Local plugin preview

Preview locally built plugins using the NuGet.org engine and its built-in theme. No package publishing, theme symlink or copied CSS/JavaScript source is needed. This is an inspection sample, not a production theme or deployment target.

## Verified engine baseline and migration

This sample targets the verified Core/Plugin/Theme/Web `1.0.0-preview.20260927.1` contracts ([release-matched guide](https://github.com/getscissorhands/ScissorHands.NET/blob/v1.0.0-preview.20260927.1/docs/website-documentation.md#locale-specific-sites)); central major-version floating ranges remain unchanged.

Both plugin packages are also [published as `1.0.0-preview.20260927.1`](https://github.com/getscissorhands/plugins/releases/tag/v1.0.0-preview.20260927.1). This checked-in sample intentionally retains local project references. A separate copy using only the published NuGet packages passed clean restore/build and both component/hook `--build`/`--preview` modes at `/` and `/blog/`; see the [release verification](../TRD.md#published-release-verification-2026-09-27). NuGet consumers must upgrade the plugins as well as the engine; the older published Open Graph binary is not compatible with the removed locale API.

- Replace removed `Site.Locale` and `Site.UseLocaleInUrl` with ordered `Site.Locales`. The sample declares `["en-US", "ko-KR"]`: primary content stays unprefixed and additional content uses `ko-kr/` exactly once.
- Remove frontmatter `locale`; it is a migration error. Place real translations under the declared additional-locale directory with the same relative filename as the primary document. The integration tests create an isolated Korean About fixture with explicit `ko-kr/about` slug: the localized loader strips its prefix before composition; with localization disabled it remains a distinct ordinary route.
- Every declared locale, including the primary, requires all three nonblank application messages: `TranslationUnavailable`, `Draft`, and `ScheduledOn` under `Theme.Localization`. `ScheduledOn` must contain the real `{0}` date argument. [appsettings.json](appsettings.json) supplies complete English/Korean catalogs; package defaults cannot replace missing entries.
- Set `Site.Locales` to `[]` to disable localization. No implicit language, HTML `lang`, Open Graph locale, translation notice, or paired-document SEO is inferred from English UI defaults. Locale-looking folders remain ordinary content, not excluded translations.

The layout forwards `LocaleContext` and all document collections unchanged. Publication status travels on the engine's `ContentDocument.PublicationStatus` snapshots. Reused engine metadata, language-switcher and fallback-banner components render the prepared context; built-in content/listing views render required publication badges. The sample does not duplicate notices/badges, compute publication eligibility, or disable engine validation.

## Run locally

**Analytics is enabled with fake ID `G-EXAMPLE`; browsing can still contact Google.** To avoid that, [disable analytics](#analytics-uses-a-fake-measurement-id) before browsing or inspect [generated files](#generate-static-output) without opening them in a browser.

From the repository root, using the SDK selected by [global.json](../global.json):

```bash
dotnet restore ./ScissorHandsPlugins.slnx --force-evaluate --no-cache
dotnet build ./ScissorHandsPlugins.slnx -c Release --no-restore -warnaserror
cd sample
dotnet run -c Release --no-build -- --preview
```

Open `http://localhost:5000`; stop preview with Ctrl+C. The single `http` launch profile does not open a browser automatically. Pass `--preview` explicitly, including in an IDE. If the port is busy, stop your existing preview before starting another.

Always run from `sample`: content, configuration and output paths are relative to the working directory. Content edits regenerate the site; refresh the browser manually. After C#/Razor changes, rebuild in Release and restart before using `--no-build`.

Put general configuration overrides before `--preview` or `--build` to avoid command-line parsing problems. The sample's `--use-placeholders` switch works on either side of those mode flags.

## Compare rendering paths

Components render both plugins by default. To exercise the post-HTML hooks instead, run from `sample`:

```bash
dotnet run -c Release --no-build -- --preview --use-placeholders
```

No configuration edit or separate launch profile is needed. Omit the switch for components. Each render uses one path, and only configured plugins produce output. Hook mode uses paired placeholders, not self-closing markers.

If rendering reports a configuration error, correct the measurement identifier or site publication URL in `appsettings.json`; enabled plugins no longer emit empty tracking identifiers or default metadata for missing required site context. See the [Google Analytics](../src/ScissorHands.Plugin.GoogleAnalytics/README.md) and [Open Graph](../src/ScissorHands.Plugin.OpenGraph/README.md) guides. Images remain optional: without a content or site image, image metadata is omitted.

The engine supplies a default site image when its setting is omitted. Set `Site.HeroImage` to `""` and leave content images empty to inspect image-tag omission.

**Document and collection URLs:** both modes use engine-resolved slugs for Open Graph `og:url`, including localized and escaped tag routes. The engine's canonical/alternate links are separate: a translated document is self-canonical, a fallback points to its primary document, and alternatives include only real translations. Canonical document links include trailing slashes; existing Open Graph route formatting does not. Generated home/tag pages and the shared 404 have no paired-document canonical/alternate links. No route reconstruction or hook-only workaround is needed.

Inspect page source, not just the visible body:

| Page | What to inspect |
| --- | --- |
| `/` | Site title/description metadata |
| `/welcome/` | Document title/description, local hero image and `@post-author` creator override |
| `/fallbacks/` | Site description/image and `@sample-author` creator fallback |
| `/about/` | Page metadata without `twitter:creator` |
| `/ko-kr/about/`, `/ko-kr/welcome/`, `/ko-kr/fallbacks/` | English article/`og:locale`, Korean requested HTML language and exactly one Korean translation notice; canonical points to primary content |
| `/ko-kr/`, `/tags/`, `/ko-kr/tags/`, `/ko-kr/tags/preview/` | Generated home/tag views, requested-locale navigation and metadata, no document fallback notice or paired SEO |
| `/404.html` | One custom, primary-language not-found document; no translated duplicate, fallback notice or paired SEO |

The checked-in content is published primary-language content; Korean routes demonstrate fallbacks, not authored translations. The integration tests create real Korean About, draft post/page, scheduled post (`2099-12-31`) and escaped-tag fixtures only inside isolated test sites, then remove them. Those fixtures exercise translated language and reciprocal alternatives, preview-only detail/listing badges, Korean fallback labels, and production exclusion from routes/navigation/plugin-visible collections. No extra Markdown fixture files are checked in.

The sample uses `Site.TimeZone: UTC`. Never deploy preview output: authored drafts/future posts are deliberately included there. Eligibility is a generation-time snapshot, not a timer or publishing service; rebuild when publication time arrives. Production hosts still configure missing-URL handling.

## Analytics uses a fake measurement ID

`G-EXAMPLE` makes analytics markup inspectable in both modes; it is not a network-blocking or consent mechanism. The plugin does not suppress preview tracking.

To disable analytics, remove the `google-analytics` object from `Plugins` in [appsettings.json](appsettings.json) and restart preview. Clearing `MeasurementId` is not a disable switch. Do not commit a real site's configuration; the sample does not validate provider delivery or consent compliance.

## Generate static output

From `sample`, generate files without starting a server or contacting Google. Inspect them as text, not in a browser:

```bash
dotnet run -c Release --no-build --no-launch-profile -- --build
dotnet run -c Release --no-build --no-launch-profile -- --build --use-placeholders
```

Preview and build output goes to `sample/preview` and `sample/dist`, respectively. These directories are Git-ignored and replaced on fresh runs; do not keep authored files there or commit generated output.

For a subpath check, use `--Site:BaseUrl=/blog/ --build` after `--` and inspect generated URLs. With this verified engine release, `--Site:BaseUrl=/blog/ --preview` also mounts preview at `/blog/`; domain-root `/` redirects to the unprefixed primary homepage `/blog/`, not `/blog/en-us/`. Do not create an extra physical `blog` directory. Production mounting remains the deployment host's responsibility.

## Built-in theme assets

The sample uses the packaged default theme's CSS, JavaScript, favicon and third-party notices. Its small `SampleLayout.razor` supplies plugin insertion and the prepared top-level sample navigation; it does not reproduce the full built-in navigation tree or theme. The color toggle stores its preference in browser `localStorage` and works independently of analytics.

If styling is missing, rebuild and restart preview, then refresh. Confirm the stylesheet/script requests return HTTP 200. Avoid a partial `sample/themes/default` directory: a local theme directory shadows the bundled theme, even without a manifest.

Asset-copy and layout implementation details belong in [T-010](../TRD.md#t-010-local-preview-integration); [sample tests](../test/ScissorHands.Plugins.Sample.Tests) cover both rendering paths and bundled assets.

## Validation and limits

From the repository root:

```bash
dotnet build test/ScissorHands.Plugins.Sample.Tests/ScissorHands.Plugins.Sample.Tests.csproj -c Release --no-restore -warnaserror
dotnet test --project test/ScissorHands.Plugins.Sample.Tests/ScissorHands.Plugins.Sample.Tests.csproj -c Release --no-build --verbosity normal
```

The integration matrix uses the actual released content loader, Markdown service, theme service, generator, renderer and plugin runner, not preconstructed route documents. It covers both plugin modes, localization disabled/enabled, root/subpath deployments, preview/production, copied sample content plus runtime-created fixtures, escaped tags, exact publication routes, notices, badges, metadata parity, and collections observed by a test plugin component. A fixed test clock keeps scheduling deterministic. HTML parsing does not fetch scripts or images. Bundled asset bytes and notices are compared with the package-provided build output.

These checks do not execute browser JavaScript, establish color-toggle interaction/accessibility, contact Google or sharing providers, demonstrate production hosting, or authorize package publication. Existing toggle markup and package script are retained; browser/provider acceptance remains separate.
