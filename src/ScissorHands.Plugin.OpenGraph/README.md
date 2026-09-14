# ScissorHands.NET: Open Graph Plugin

Adds [Open Graph](https://ogp.me/) and Twitter-card metadata to a compatible ScissorHands.NET site.

## Getting Started

Install the preview package in your ScissorHands.NET host:

```bash
dotnet add package ScissorHands.Plugin.OpenGraph --prerelease
```

Add this entry to the `Plugins` array in `appsettings.json`:

```json
{
  "Plugins": [
    {
      "Id": "open-graph",
      "Options": {
        "TwitterSiteId": "@example",
        "TwitterCreatorId": "@author"
      }
    }
  ]
}
```

`TwitterSiteId` identifies the website's account; `TwitterCreatorId` is the default content-author handle. Both are optional: omit either or use `"Options": {}`.

In a layout supplied with the engine's cascading context, place the component inside `<head>`:

```razor
<OpenGraphComponent Id="open-graph" />
```

Alternatively, use the paired placeholder for the post-HTML hook:

```html
<plugin:open-graph></plugin:open-graph>
```

Choose one path per insertion to avoid duplicates. Hook placeholders must be paired, not self-closing. The `Id` is exact and case-sensitive; an optional manifest `Name` is only a display label. Remove the entry from `Plugins` to disable output.

## Support

See the shared [support and recovery policy](https://github.com/getscissorhands/plugins#support-and-recovery) for best-effort issue support and preview-release recovery.
