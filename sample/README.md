# Local plugin preview

Preview locally built plugins using the NuGet.org engine and its built-in theme. No package publishing, theme symlink or copied CSS/JavaScript source is needed. This is an inspection sample, not a production theme or deployment target.

## Run locally

**Analytics is enabled with fake ID `G-EXAMPLE`; browsing can still contact Google.** To avoid that, [disable analytics](#analytics-uses-a-fake-measurement-id) before browsing or inspect [generated files](#generate-static-output) without opening them in a browser.

From the repository root, using the SDK selected by [global.json](../global.json):

```bash
dotnet restore
dotnet build
cd sample
dotnet run -- --preview
```

Open `http://localhost:5000`; stop preview with Ctrl+C. The single `http` launch profile does not open a browser automatically. Pass `--preview` explicitly, including in an IDE. If the port is busy, stop your existing preview before starting another.

Always run from `sample`: content, configuration and output paths are relative to the working directory. Content edits regenerate the site; refresh the browser manually. After C#/Razor changes, rebuild in Release and restart before using `--no-build`.

Put general configuration overrides before `--preview` or `--build` to avoid command-line parsing problems. The sample's `--use-placeholders` switch works on either side of those mode flags.

## Compare rendering paths

Components render both plugins by default. To exercise the post-HTML hooks instead, run from `sample`:

```bash
dotnet run -- --preview --use-placeholders
```

No configuration edit or separate launch profile is needed. Omit the switch for components. Each render uses one path, and only configured plugins produce output. Hook mode uses paired placeholders, not self-closing markers.

Inspect page source, not just the visible body:

| Page | What to inspect |
| --- | --- |
| `/` | Site title/description metadata |
| `/welcome/` | Document title/description, local hero image and `@post-author` creator override |
| `/fallbacks/` | Site description/image and `@sample-author` creator fallback |
| `/about/` | Page metadata without `twitter:creator` |
| `/tags/` | Engine tag views through the same sample layout |
| `/404.html` | Custom not-found document; missing-URL routing remains a host responsibility |

## Analytics uses a fake measurement ID

`G-EXAMPLE` makes analytics markup inspectable in both modes; it is not a network-blocking or consent mechanism. The plugin does not suppress preview tracking.

To disable analytics, remove the `google-analytics` object from `Plugins` in [appsettings.json](appsettings.json) and restart preview. Clearing `MeasurementId` is not a disable switch. Do not commit a real site's configuration; the sample does not validate provider delivery or consent compliance.

## Generate static output

From `sample`, generate files without starting a server or contacting Google. Inspect them as text, not in a browser:

```bash
dotnet run -- --build
dotnet run -- --build --use-placeholders
```

Preview and build output goes to `sample/preview` and `sample/dist`, respectively. These directories are Git-ignored and replaced on fresh runs; do not keep authored files there or commit generated output.

For a metadata-only subpath check, use `--Site:BaseUrl=/blog/ --build` after `--` and inspect the generated URLs. Preview defaults to `/`: changing `BaseUrl` alone does not mount the server at that prefix.

## Built-in theme assets

The sample uses the packaged default theme's CSS, JavaScript, favicon and third-party notices. Its small `SampleLayout.razor` supplies plugin insertion and sample navigation; it does not reproduce the full built-in layout. The color toggle stores its preference in browser `localStorage` and works independently of analytics.

If styling is missing, rebuild and restart preview, then refresh. Confirm the stylesheet/script requests return HTTP 200. Avoid a partial `sample/themes/default` directory: a local theme directory shadows the bundled theme, even without a manifest.

Asset-copy and layout implementation details belong in [T-010](../TRD.md#t-010-local-preview-integration); [sample tests](../test/ScissorHands.Plugins.Sample.Tests) cover both rendering paths and bundled assets.
