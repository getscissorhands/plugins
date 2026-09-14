# Local plugin preview

This non-packable sample follows the small web-host structure of the
[theme-template sample](https://github.com/getscissorhands/theme-template/tree/main/sample).
It consumes the NuGet.org `ScissorHands.Web` engine and references both plugin
projects in this checkout, so plugin changes do not require package publishing.

`SampleLayout.razor` supplies plugin insertion points and forwards the upstream
cascading context. The other six views come from the engine's default theme.
This is a minimal inspection layout, not a new production theme; no theme
symlinks, CSS build step, or copied engine sources are required.

## Run locally

From the repository root:

```powershell
dotnet restore .\ScissorHandsPlugins.sln
dotnet build .\ScissorHandsPlugins.sln -c Release --no-restore
Set-Location sample
dotnet run -c Release --no-build -- --preview
```

Open `http://localhost:5073`. The launch profile does not open a browser
automatically or choose the application mode. Pass `--preview` explicitly,
including when launching from an IDE. Stop the server with Ctrl+C.

Run from the `sample` directory: the engine resolves content/configuration and
output relative to the working directory. Do not invoke the built executable
from the repository root. If the port is busy, select another loopback port:

```powershell
dotnet run --no-launch-profile -- --urls=http://localhost:5074 --preview
```

Keep configuration overrides before the final `--preview` or `--build` flag;
the host's command-line configuration parser can consume the next argument
as the value of a bare mode flag.

Preview regenerates after content changes; refresh the browser manually.
Changes to C#/Razor require a rebuild and restart. Do not use `--no-build` after
editing plugin or sample code unless you have rebuilt that configuration.

## Compare rendering paths

The default `http` profile renders `OpenGraphComponent` and
`GoogleAnalyticsComponent`. Google Analytics emits nothing unless configured.
The `hooks` profile instead inserts paired markers for configured plugins,
which the engine's post-HTML pipeline replaces:

```powershell
dotnet run --launch-profile hooks -- --preview
```

Alternatively set `Sample:UsePlaceholders` to `true` via configuration. The
layout never inserts both paths in one render, and does not emit markers for
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

## Analytics is opt-in

The checked-in `Plugins` array enables only Open Graph. Adding an analytics
manifest is explicit opt-in because visiting that output can contact Google,
even in preview; the plugin itself does not implement consent or suppression.

For local markup inspection with a synthetic ID:

```powershell
dotnet run --no-launch-profile -- --Plugins:1:Id=google-analytics --Plugins:1:Options:MeasurementId=G-EXAMPLE --preview
```

`G-EXAMPLE` is not a working measurement configuration or a network-blocking
mechanism. Inspect the generated file without browsing it if you do not want
Google requests. Do not commit a real site's configuration; provider delivery
and consent compliance are not validated by this sample.

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
cover both rendering modes and analytics enablement without provider requests.
