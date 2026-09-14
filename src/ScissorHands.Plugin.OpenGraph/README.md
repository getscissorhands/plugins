# ScissorHands.NET: Open Graph Plugin

Adds [Open Graph](https://ogp.me/) and Twitter-card metadata to a compatible ScissorHands.NET site.

## Getting Started

Install the preview package in your ScissorHands.NET host:

```bash
dotnet add package ScissorHands.Plugin.OpenGraph --prerelease
```

Configure the site and plugin in `appsettings.json`:

```json
{
  "Site": {
    "SiteUrl": "https://example.com",
    "BaseUrl": "/blog/",
    "Title": "My site",
    "Description": "About this site",
    "Locale": "en-US",
    "HeroImage": "/images/site.png"
  },
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

`TwitterSiteId` identifies the website's account; `TwitterCreatorId` is the default post-author handle. Both are optional: omit either or use `"Options": {}`. Null, non-string and whitespace-only values omit their optional tags.

In a layout supplied with the engine's cascading context, place the component inside `<head>`:

```razor
<OpenGraphComponent Id="open-graph" />
```

Alternatively, use the paired placeholder for the post-HTML hook:

```html
<plugin:open-graph></plugin:open-graph>
```

Choose one path per insertion to avoid duplicates. Hook placeholders must be paired, not self-closing; every matching pair is replaced. The `Id` is exact and case-sensitive; an optional manifest `Name` is only a display label. Remove the entry from `Plugins` to disable output.

## Metadata and publication URLs

Given equivalent host context, both integrations produce equivalent metadata:

- Individual source-backed documents use `Document title | Site title` and the document description, falling back to the site description only when null. Collections, source-less documents and missing documents use site title/description.
- `twitter:creator` appears only for an individual source-backed **post**, not pages, collections or source-less posts. A nonblank document `TwitterHandle` overrides `TwitterCreatorId`.
- A nonblank document hero image wins over the site image. When neither exists, both image tags are omitted; the card remains `summary_large_image`.

**Suppressing the inherited image:** the released `SiteManifest` supplies an external `hero.jpg` by default. Omitting `Site.HeroImage` therefore does not necessarily remove image metadata. Set `"HeroImage": ""` inside `Site` and leave the document image absent to omit both image tags.

Configured output requires site context and an absolute HTTP(S) `SiteUrl` with a host and no query/fragment. A path on `SiteUrl` is preserved. `BaseUrl` is an optional local path prefix, not an absolute/network URL or a query/fragment; use `""` or `"/"` for root deployment. Blank/root content slugs map to this publication root. In the example above, `/post` becomes `https://example.com/blog/post`, and `/images/site.png` becomes `https://example.com/blog/images/site.png`.

**Generated tag pages:** the current host supplies tag-route documents only to hooks, so components emit the site-root `og:url`. Use hook mode where accurate generated tag-page canonical URLs are needed. Component parity requires upstream to supply a resolved document/route cascade; see [OG-Q-005](https://github.com/getscissorhands/plugins/blob/main/src/ScissorHands.Plugin.OpenGraph/TRD.md#og-q-005-generated-tag-pages-receive-unequal-host-context).

Images accept local paths, including a single leading `/` or local backslash separators, and absolute HTTP(S) URLs with a host. External origins, supported queries/fragments, existing percent encoding and meaningful trailing slashes are preserved. Unsupported schemes (`data:`, `javascript:`, `file:`, `ftp:`, etc.), malformed references/percent escapes, control characters and network paths such as `//host/image.png` or `\\host\image.png` are rejected. Use explicit HTTP(S) URLs for external images and prefer percent-encoded spaces.

The plugin does not mount the host at `BaseUrl`. See the [technical requirements](https://github.com/getscissorhands/plugins/blob/main/src/ScissorHands.Plugin.OpenGraph/TRD.md) for complete metadata, URL, lifecycle and verification contracts.

## Breaking migration from the earlier permissive behavior

1. Supply valid publication context wherever enabled. Invalid origins now fail with a field-specific error instead of relative URLs/default tags; an absent hook marker does not hide invalid configuration.
2. Correct unsupported image references. Errors identify `Site.HeroImage` or `Document.Metadata.HeroImage` without echoing arbitrary input.
3. Account for omitted image/creator tags. Clear the inherited site image explicitly if no image is wanted.
4. Supply original metadata text, not HTML or pre-encoded entities. Both integrations treat metadata as data.

An absent manifest remains silent without validating unused site/image context. Components refresh as context changes; provide site context before enabling and disable before removing it.

## Preview and privacy

Configured preview and production use the same rules. Generation does not fetch images or contact social providers, but a later browser/crawler may request emitted external images. Metadata generation does not guarantee crawler acceptance or rich-preview appearance.

## Support

See the shared [support and recovery policy](https://github.com/getscissorhands/plugins#support-and-recovery) for best-effort issue support and preview-release recovery.
