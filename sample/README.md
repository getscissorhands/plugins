# Local plugin preview

This non-packable sample follows the small web-host structure of the
[theme-template sample](https://github.com/getscissorhands/theme-template/tree/main/sample).
It consumes the NuGet.org `ScissorHands.Web` engine and references both plugin
projects in this checkout, so plugin changes do not require package publishing.

`SampleLayout.razor` supplies plugin insertion points and forwards the upstream
cascading context. The other six views come from the engine's default theme.
This is an inspection layout, not a new production theme. It uses the built-in
theme's packaged CSS, JavaScript, favicon and color toggle while retaining the
sample navigation and plugin controls. No theme symlink or CSS/JavaScript source
copy needs to be maintained.

## Built-in theme assets

The installed `ScissorHands.Web` package includes `themes/default` as NuGet
content files, but the current release does not automatically copy them to a
consumer's output directory. `sample.csproj` opts that package's content into
build/publish output copying, preserving its linked paths. The package version
is resolved through central management, not a hardcoded NuGet-cache path.

At runtime, the engine finds `themes/default/theme.json` below the application
output and copies the assets into the generated site. `SampleLayout` emits
the manifest's stylesheet/script references using `GetThemeUrl`, so the browser
can load `themes/default/assets/theme.css` and `theme.js`. The package's
third-party notice is carried alongside the assets.

The layout initializes the color preference before loading styles, following
the built-in layout's convention. The packaged theme script implements the
toggle and stores the preference in the browser's `localStorage`. It is
unrelated to analytics and loads locally even
when analytics is disabled. This does not reproduce the complete built-in
`MainLayout` or its hierarchical navigation.

If styling is missing, rebuild the sample and restart preview before refreshing
the browser. Confirm the stylesheet/script requests return HTTP 200. Avoid
creating a partial `sample/themes/default` directory: a local theme directory
takes precedence over the bundled output directory, even if it has no manifest.

## Run locally

From the repository root:

```powershell
dotnet restore .\ScissorHandsPlugins.sln
dotnet build .\ScissorHandsPlugins.sln -c Release --no-restore
Set-Location sample
dotnet run -c Release --no-build -- --preview
```

Open `http://localhost:5000`. The single `http` launch profile does not open a
browser automatically or choose the application mode. Pass `--preview` explicitly,
including when launching from an IDE. Stop the server with Ctrl+C.

Run from the `sample` directory: the engine resolves content/configuration and
output relative to the working directory. Do not invoke the built executable
from the repository root. The launch profile specifies `http://localhost:5000`,
and `Site.SiteUrl` uses it for static metadata. Without a launch profile or
another endpoint override, ASP.NET Core defaults to the same address. If the
port is busy, stop your existing preview before starting another.

Keep configuration overrides before the final `--preview` or `--build` flag;
the host's command-line configuration parser can consume the next argument
as the value of a bare mode flag.

Preview regenerates after content changes; refresh the browser manually.
Changes to C#/Razor require a rebuild and restart. Do not use `--no-build` after
editing plugin or sample code unless you have rebuilt that configuration.

## Compare rendering paths

The default configuration renders `OpenGraphComponent` and
`GoogleAnalyticsComponent`, with the fake measurement ID `G-EXAMPLE`.
To insert paired markers for the engine's post-HTML pipeline instead, set
`Sample:UsePlaceholders` through a command-line override using the same `http`
profile:

```powershell
dotnet run -- --Sample:UsePlaceholders=true --preview
```

There is no separate `hooks` launch profile. You can also set
`Sample:UsePlaceholders` to `true` in `appsettings.json`. The layout never
inserts both paths in one render, and does not emit markers for
unconfigured plugins. These examples use paired markers rather than assuming
that self-closing markers are normalized by the host.

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

The checked-in `Plugins` array enables Open Graph and Google Analytics. Google
Analytics uses the synthetic measurement ID `G-EXAMPLE` to make its markup easy
to inspect in both component and hook modes.

The fake ID is not a working measurement configuration or a network-blocking
mechanism. Browsing the output can still contact Google, including in preview;
the plugin does not implement consent or suppression. To inspect the markup
without making browser requests, generate the files without opening them:

```powershell
dotnet run --no-launch-profile -- --build
```

Remove the `google-analytics` object from the `Plugins` array and restart
preview to disable it. Clearing `MeasurementId` is not a disable switch.
Do not commit a real site's configuration; provider delivery and consent
compliance are not validated by this sample.

## Generate static output

From `sample`, use the build mode to inspect output without starting a server:

```powershell
dotnet run --no-launch-profile -- --build
dotnet run --no-launch-profile -- --Sample:UsePlaceholders=true --build
```

Preview/build output goes to `sample/preview` and `sample/dist`. These directories
are ignored by Git and excluded from project inputs. The engine replaces the
chosen output on a fresh run; do not keep authored files there.

For metadata-only subpath checks, pass `--Site:BaseUrl=/blog/` before `--build`
and inspect the generated URLs. The sample defaults to `/` because the current
engine preview server does not mount output at a configured prefix; changing
`BaseUrl` alone is not proof of subpath HTTP serving.

The sample is built by the solution but is not a NuGet package or a deployment
target. Its [layout integration tests](../test/ScissorHands.Plugins.Sample.Tests)
cover both rendering modes, analytics enablement, theme references and bundled
assets without provider requests.
