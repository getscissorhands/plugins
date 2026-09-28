# ScissorHands.NET: Open Graph Plugin

Adds [Open Graph](https://ogp.me/) and Twitter-card metadata to a compatible ScissorHands.NET site.

## Getting Started

Install the verified preview package in your ScissorHands.NET host:

```bash
dotnet add package ScissorHands.Plugin.OpenGraph --version 1.0.0-preview.20260927.1
```

**Published compatibility:** Open Graph `1.0.0-preview.20260927.1` is verified with engine Core/Plugin `1.0.0-preview.20260927.1` (.NET 10). The [release](https://github.com/getscissorhands/plugins/releases/tag/v1.0.0-preview.20260927.1) is available on NuGet.org, and a clean NuGet-only consumer passed component/hook build and preview checks at root and subpath URLs. Plugin versions do not track engine version numbers.

**Source upgrade:** this repository now builds against engine `1.0.0-preview.20260928.1`; the published plugin above does not support the removed `SiteManifest.HeroImage` API. Do not combine that binary with the new engine. A compatible plugin release has not been published.

Do not retain Open Graph `1.0.0-preview.20260915.1` with the `20260927.1` engine: its binary calls the removed `SiteManifest.get_Locale()` API. A successful host build does not prevent the resulting `MissingMethodException` during generation. Upgrade the plugin and refresh the resolved dependency graph.

Configure the site and plugin in `appsettings.json`:

```json
{
  "Site": {
    "SiteUrl": "https://example.com",
    "BaseUrl": "/blog/",
    "Title": "My site",
    "Description": "About this site"
  },
  "Theme": {
    "HeroImages": [
      { "Source": "/images/site.png", "Alt": "Site illustration" }
    ]
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

The host injects its validated `ThemeSettings` into the Open Graph hook plugin. If you construct `OpenGraphPlugin` manually, pass those settings to its constructor; the parameterless constructor has no site-wide image fallback.

## Metadata and publication URLs

Given equivalent host context, both integrations produce equivalent metadata:

- Individual source-backed documents use `Document title | Site title` and the document description, falling back to the site description only when null. Collections, source-less documents and missing documents use site title/description.
- `twitter:creator` appears only for an individual source-backed **post**, not pages, collections or source-less posts. A nonblank document `TwitterHandle` overrides `TwitterCreatorId`.
- A nonblank document hero image wins over the first `Theme.HeroImages` entry's `Source`. When neither exists, both image tags are omitted; the card remains `summary_large_image`. Later entries are not used for social metadata.
- With no `Site.Locales`, localization is disabled and `og:locale` is omitted. With locales configured, the tag describes actual content language: a Korean fallback containing English content reports English, while a real Korean translation reports Korean.

**Omitting image metadata:** `Theme.HeroImages` is optional; omit it or use `[]`, and leave the document image absent. A null or empty settings list also produces no fallback image. The old `Site.HeroImage` key is rejected by the new engine.

Configured output requires site context and an absolute HTTP(S) `SiteUrl` with a host and no query/fragment. A path on `SiteUrl` is preserved. `BaseUrl` is an optional local path prefix, not an absolute/network URL or a query/fragment; use `""` or `"/"` for root deployment. Blank/root content slugs map to this publication root. In the example above, `/post` becomes `https://example.com/blog/post`, and `/images/site.png` becomes `https://example.com/blog/images/site.png`.

**Resolved routes:** custom layouts must forward `Document`, `LocaleContext` and `ThemeSettings` through `CascadingMainLayoutBase`. `og:url` identifies the current route, including translated/fallback copies and generated home/tag/404 pages, with no extra locale prefix or generated-route escaping. A fallback at `/blog/ko-kr/about` keeps that social URL; the theme's separate canonical still identifies the primary `/blog/about`. Open Graph emits no canonical/hreflang links and leaves existing theme metadata, fallback notices and preview badges untouched. Generated collections and the shared 404 do not acquire paired-document SEO.

Components prefer the engine's actual-language and current-route snapshot; hooks receive the resolved document, not `LocaleContext`. Do not assume arbitrary slug/language changes by earlier Markdown hooks refresh prepared snapshots or preserve cross-surface parity.

The intentional fallback distinction follows the released plugin contract's supplied-route composition. Open Graph does not infer a primary route or read canonical links back from theme HTML. Consequently, `og:url` need not equal the theme canonical, and consolidation of fallback sharing identifiers by social platforms is not guaranteed.

**Historical compatibility:** the previous source revision verified generated tag-route parity with `1.0.0-preview.20260915.1`, closing [OG-Q-005](https://github.com/getscissorhands/plugins/blob/main/src/ScissorHands.Plugin.OpenGraph/TRD.md#og-q-005-generated-tag-pages-receive-unequal-host-context). Its older `1.0.0-preview.20260914.1` host required hook mode because tag documents did not reach components. This is retained history, not a claim that this newly compiled revision supports those older binaries.

Images accept local paths, including a single leading `/` or local backslash separators, and absolute HTTP(S) URLs with a host. External origins, supported queries/fragments, existing percent encoding and meaningful trailing slashes are preserved. Unsupported schemes (`data:`, `javascript:`, `file:`, `ftp:`, etc.), malformed references/percent escapes, control characters and network paths such as `//host/image.png` or `\\host\image.png` are rejected. Use explicit HTTP(S) URLs for external images and prefer percent-encoded spaces.

The plugin does not mount the host at `BaseUrl`. See the [technical requirements](https://github.com/getscissorhands/plugins/blob/main/src/ScissorHands.Plugin.OpenGraph/TRD.md) for complete metadata, URL, lifecycle and verification contracts.

## Locale migration for engine `1.0.0-preview.20260927.1`

1. Remove `Site.Locale`, `Site.LocalizationFallbackMessages`, `UseLocaleInUrl` and frontmatter `locale`. The engine rejects obsolete settings; renaming only a C# property is not a complete host migration.
2. For a nonlocalized site, omit `Site.Locales` (as above), or use `[]`/`null`. No language is inferred from English UI defaults. For localization, declare the primary first, for example `"Locales": ["en-US", "ko-KR"]`. Primary routes stay unprefixed; translations belong in the matching additional-locale directory with matching relative filenames/slugs.
3. Supply all required application-owned messages under top-level `Theme.Localization` for **every** declared locale:

   ```json
   {
     "Theme": {
       "Localization": {
         "en-us": { "TranslationUnavailable": "Translation unavailable.", "Draft": "Draft", "ScheduledOn": "Scheduled on {0}" },
         "ko-kr": { "TranslationUnavailable": "번역을 사용할 수 없습니다.", "Draft": "초안", "ScheduledOn": "{0} 게시 예정" }
       }
     }
   }
   ```

   `Site.Theme` remains the theme slug string. The host's effective theme supplies the catalog; Open Graph neither validates nor modifies those messages.
4. Upgrade custom layouts to forward resolved context and render the engine's theme-owned localization/status contracts. Keep canonical/hreflang markup separate from Open Graph, and retain fallback notices/badges in both component and placeholder modes. Never deploy preview output.
5. Refresh cached major floats with `dotnet restore --force-evaluate --no-cache`, rebuild, and verify the actual dependency graph and both insertion paths. A newer engine alone does not upgrade an already-published plugin binary.

See the [release-matched engine migration guide](https://github.com/getscissorhands/ScissorHands.NET/blob/7b1c53296fe1e806c465c549c8429dba7eac61d7/docs/website-documentation.md#locale-routing-migration) for content pairing, publication eligibility and required theme rendering.

## Breaking migration from the earlier permissive behavior

1. Supply valid publication context wherever enabled. Invalid origins now fail with a field-specific error instead of relative URLs/default tags; an absent hook marker does not hide invalid configuration.
2. Correct unsupported image references. Errors identify `Theme.HeroImages[0].Source` or `Document.Metadata.HeroImage` without echoing arbitrary input.
3. Account for omitted image/creator tags. Without a configured first theme image or document image, image tags are absent.
4. Supply original metadata text, not HTML or pre-encoded entities. Both integrations treat metadata as data.

An absent manifest remains silent without validating unused site/image context. Components refresh as context changes; provide site context before enabling and disable before removing it.

## Preview and privacy

Configured preview and production use the same rules. Generation does not fetch images or contact social providers, but a later browser/crawler may request emitted external images. Metadata generation does not guarantee crawler acceptance or rich-preview appearance.

## Support

See the shared [support and recovery policy](https://github.com/getscissorhands/plugins#support-and-recovery) for best-effort issue support and preview-release recovery.
