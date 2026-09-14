# Open Graph - Technical requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin PRD](PRD.md) | [Usage](README.md)

## Baseline and inheritance

| Field | Value |
| --- | --- |
| Version / status | 0.7 / Implementation-ready |
| Last updated / PRD consulted | 2026-09-14 |
| Product baseline | Open Graph PRD v0.7, Implementation-ready with unchanged product behavior and acceptance |
| Shared baseline | Catalog PRD/TRD v0.8; apply shared obligations without silently overriding them |
| Source baseline | Commit `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`; target runtime changes remain pending |
| Package / plugin ID | `ScissorHands.Plugin.OpenGraph` / `open-graph` |
| Approval / owner | @justinyoo owns implementation, verification, support and release authorization; metadata, URL/output and regression requirements confirmed on 2026-09-14, not runtime acceptance or approval to publish |
| Release stage | Preview, with versioning/releases independent from the upstream engine |

This TRD owns Open Graph's technical behavior and evidence expectations. Shared T-001 through T-004 and T-006 through T-009 apply; original T-005 is relocated here as the authoritative social-URL requirement, with a gateway redirect. New local records use `OG-TR-*`.

The local PRD, this TRD and catalog requirements define the accepted obligations. Engine identity, hook and component API references are centralized in the [catalog TRD](../../TRD.md#1-shared-boundaries). That release-matching policy also applies to the Core URL-helper reference below. Engine planning/contributor documents are not prerequisites or sources of additional policy.

The plugin is an independent Razor class library consuming Plugin/Core. It overrides only `PostHtmlAsync`, has no `DependsOn` override, and performs no file/network operation in generation. It does not implement route loading, navigation, preview hosting or image fetching. Source links and verification below own the implementation details formerly repeated in the PRD.

## OG-TR-001: Option and metadata behavior

**State / source:** Confirmed requirement (user, 2026-09-14); P-FR-003 and shared T-004. [Hook](OpenGraphPlugin.cs), [component code](OpenGraphComponent.razor.cs) and [helper](OpenGraphPluginHelper.cs) retain the current behavior until implemented.

**Configuration example, relocated from PRD v0.5:**

```json
{
  "Plugins": [
    {
      "Id": "open-graph",
      "Options": { "TwitterSiteId": "@example", "TwitterCreatorId": "@author" }
    }
  ]
}
```

Read nullable options by typed `TryGetValue` without mutation. Equivalent input contexts must produce equivalent metadata values and optional-tag presence across both paths. Retain the following defaults except for the explicit creator change:

| Input / context | Accepted target |
| --- | --- |
| `TwitterSiteId`, `TwitterCreatorId` | Missing/null/non-string values are ignored; empty/whitespace values omit optional tags |
| `Document.Metadata.TwitterHandle` | Non-whitespace metadata overrides the creator option |
| Title/description | `UseContentMetadata` requires a document, no collection and a non-blank `SourcePath`. Use document title plus site title, and document description with null fallback; otherwise use site title/description |
| Creator scope | Both paths retain creator only for an individual source-backed `ContentKind.Post` for which content metadata is used. Suppress it for pages, collections and source-less posts |
| Site metadata | Use supplied `Site.Locale` and `Site.Title`; do not recompose engine locale/date routes |

The hook has no collection parameter and uses the host-provided document; synthetic collection documents have no source path. The component additionally considers its `Documents` cascade. Compare equivalent contexts rather than inventing a new hook parameter or engine pass. HTML serialization/whitespace need not be identical.

**Current gap / verification:** the hook still emits a creator for source-less posts. Add comparable-context cases for source-backed/source-less posts, pages and synthetic collections in [hook tests](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphPluginTests.cs) and [bUnit tests](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphComponentTests.cs), retaining typed/default option, snapshot, override and fallback coverage. Policy agreement does not make the earlier tests evidence of parity.

## OG-TR-002: Hook and component integration

**State / source:** Confirmed requirement (user, 2026-09-14); OG-FR-001 and shared P-FR-001/P-FR-004; T-001/T-002/T-003. Preserve host selection/insertion while replacing the missing-context fallback with an explicit failure.

Retain `Id="open-graph"` and non-empty implementation name. The hook checks cancellation, replaces all exact paired markers `<plugin:open-graph></plugin:open-graph>` case-insensitively and leaves unmarked HTML unchanged for otherwise valid inputs. Paired markers are the supported hook syntax; self-closing support is not added. It does not enforce host enablement or deduplicate tags.

The alternative component syntax is `<OpenGraphComponent Id="open-graph" />` within the upstream cascade. Select one path per intended insertion to avoid duplicates. The hook marker and self-closing Razor component syntax are distinct contracts.

The component must call `base.OnParametersSet()`, clear derived fields and recompute from the selected manifest/context. An absent manifest emits nothing; a matched manifest with missing required site/origin information must fail clearly rather than render default tags. Missing document context is valid for site-level pages when site configuration is valid.

**Current gap / verification:** code-behind currently returns early for a missing site while Razor permits default tags. Add explicit failure cases for that path and invalid origins, preserve absent-manifest suppression, and cover transitions/removal. Await existing cancellation assertions. OG-Q-001 is resolved by paired README examples; OG-Q-003's runtime/evidence work remains pending.

## T-005: Content and image URL boundaries

**State / source:** Confirmed requirement (user, 2026-09-14); P-FR-005, shared P-NFR-005; [local helper](OpenGraphPluginHelper.cs) and [Core URL-helper API reference](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/docs/website-documentation.md#shared-url-helpers). Retain the original catalog ID and locally agreed validation/omission rules; the helper reference describes dependency semantics, not product policy.

For non-blank slugs, use `ContentUrlHelper.GetContentUrl`: trim outer whitespace, normalize both slash separators, escape segments and reject literal `.`/`..` segments. Convert the helper's root result `.` to the site root. Null/blank local slugs also return the site root; this local fallback does not change upstream null-argument behavior.

When emitting metadata, require a supplied site and an absolute HTTP(S) `SiteUrl` with a non-empty host. Missing/blank/malformed or unsupported-scheme site URLs must fail with actionable context, not return relative social URLs. Compose site-local URLs with `SiteUrl` and `BaseUrl`; preserve subpaths rather than independently adding routes, dates or locales.

Hero images select a non-whitespace document image, otherwise the site image. If both are absent, omit both `og:image` and `twitter:image`; the helper may retain an empty-string absence result, without changing its public return type. Other optional-tag defaults and the card type do not change as an incidental consequence.

Classify images before normalization: accept site-local paths (including a leading single `/`) and absolute HTTP(S) URLs with a host. Other absolute schemes, malformed absolute references and network-path references such as `//host/image.png` must not be silently reinterpreted as local assets; use an explicit HTTP(S) form for an external image. Preserve supported query strings, fragments, existing percent encoding and significant trailing slashes, without applying content-slug escaping to images. Use Core's image helper for its narrow formatting semantics only, after local input policy is enforced.

**Current gap / verification:** the helper still accepts general absolute URI schemes, permits missing origins and composes empty image values; raw renderers always emit image tags. Add root/subpath, HTTP/HTTPS, missing site, malformed/disallowed URL, query/fragment/encoding/trailing-slash and absent-image cases across helper/hook/component tests. Include failures that would otherwise be hidden by slash trimming. Invalid required URLs fail explicitly; optional absence omits tags. Old nullable-helper/fallback assertions must be reconciled with the changed contract. Host mounting and provider acceptance are separate checks.

## OG-TR-003: Output and preview boundaries

**State / source:** Confirmed requirement (user, 2026-09-14); OG-NFR-001, shared P-NFR-003 and T-006. Keep metadata context and external behavior explicit.

Normal metadata must remain text in both renderers: escape the hook's attribute values correctly while preserving Razor encoding and avoiding double encoding. Metadata containing quotes, markup-like or placeholder-like text must not become elements, attributes or further template substitutions. Do not blanket-sanitize unrelated document HTML. Apply T-005's accepted URL policy separately from HTML encoding.

Retain Open Graph output in preview and production under the same validation rules. Neither path fetches external images or calls social providers during generation; consumers can subsequently request image URLs. No crawler-acceptance, privacy, offline or delivery guarantee is claimed. OG-Q-004 is settled without a suppression feature.

**Current gap / verification (OG-Q-002):** hook metadata is still substituted without explicit HTML encoding. Add synthetic text/attribute/template-shaped and invalid URL cases alongside comparable Razor output. Existing tests do not establish completion of the accepted encoding/scheme rules.

## Traceability and verification

| Product baseline | Technical coverage | Evidence / limits |
| --- | --- | --- |
| P-FR-003 | OG-TR-001/003 | Target parity/encoding accepted; runtime/regressions pending |
| P-FR-005 | T-005 | Target required-origin, image-omission and URL policy accepted; runtime/regressions pending |
| OG-FR-001 | OG-TR-002 | Paired-marker examples aligned; missing-context/cancellation/removal work pending |
| OG-NFR-001 | OG-TR-003 | Source-backed preview/external-reference boundary, not provider acceptance |
| Shared P-FR-001/P-FR-004 | OG-TR-002, T-001/T-002/T-003 | Exact identity, supported insertion and absent-manifest behavior |
| Shared P-FR-006 | T-009 and this pair | Gateway links, versioned baseline and evidence mapping |
| Shared P-NFR-001 | T-007/T-008 | Resolved graph, Release build, tests and package inspection |
| Shared P-NFR-002 | T-002/T-004, OG-TR-001/002 | Read-only options and observable errors/cancellation; evidence caveats retained |
| Shared P-NFR-003 | OG-TR-003, T-006 | Context-specific validation review; no comprehensive audit claim |
| Shared P-NFR-005 | T-005, OG-TR-001/003 | Resolved URLs/site locale; head-only output, no visible UI or measured performance contract |

Use [AGENTS.md](../../AGENTS.md) for commands. Verify this package's assembly, README, license, icon, symbols and dependency metadata under shared [T-008](../../TRD.md#t-008-package-and-consumer-documentation). It and [Q-005](../../PRD.md#shared-release-question) govern accepted release/support/recovery and optional-only deferral policies. Required parity/context, URL/output and regression work is pending, not deferred. Package creation and policy agreement are not authorization to publish.

## Gaps and readiness

The [PRD decision records](PRD.md#plugin-questions-and-acceptance-limits) retain product choices and routing IDs. This TRD owns their technical delivery state: OG-Q-001's paired-marker documentation is aligned and OG-Q-004's preview behavior is retained. OG-Q-002's image/URI/encoding work and OG-Q-003's parity, missing-context and awaited cancellation/removal evidence remain pending. @justinyoo owns the follow-ups. Existing passing tests do not establish the stricter target.

**Migration / current behavior, relocated from PRD v0.5:** the hook still permits creator metadata for source-less posts, missing-site components can emit defaults, and image/helper paths can return empty or relative values and accept general absolute URI schemes. Before adopting the stricter implementation, configure valid site context and an absolute HTTP(S) `SiteUrl`, use the image forms allowed by T-005, and account for creator/image tags that may no longer be emitted. Optional absent images are omitted, not a reason to disable the plugin. These are breaking behavior changes; this revision does not implement them.

No analytics/consent service, database, account system, remote-generation API, navigation subsystem or accessibility/browser-conformance program is part of this plugin baseline. External image and locale/output semantics remain applicable; future features require a separate applicability review.

**v0.5 confirmation (2026-09-14):** the user explicitly confirmed metadata consistency, required versus optional metadata, URL/output handling and regression coverage. OG-TR-001/002/003 and T-005 now record confirmed requirements without changing scope, migration effects or IDs.

**v0.6 separation (2026-09-14):** consolidates package/source metadata, JSON/component examples, exact field/URL contracts and technical delivery/migration records here. PRD question IDs retain their product decisions and link to this evidence; no behavior, ID or acceptance obligation changes.

**v0.7 reference policy (2026-09-14):** makes local requirement authority and release-matched API references explicit. All metadata, URL, lifecycle, privacy, migration and verification obligations remain unchanged.

**Readiness:** Implementation-ready against plugin PRD v0.7 and shared v0.8 baselines. No unresolved policy or technical requirement blocks implementation. Runtime parity, validation, omission, URL/output changes and regression evidence remain pending, not deferred. Shared T-008 records reported publishing setup and the limits of independent verification; release evidence and authorization are still required. This is not completed implementation, a completed audit or release approval.
