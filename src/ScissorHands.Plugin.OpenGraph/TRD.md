# Open Graph - Technical requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin PRD](PRD.md) | [Usage](README.md)

## Baseline and inheritance

| Field | Value |
| --- | --- |
| Version / status | 0.1 / Review-ready |
| Last updated / PRD consulted | 2026-09-14 |
| Product baseline | Open Graph PRD v0.1, Review-ready |
| Shared baseline | Catalog PRD/TRD v0.2; apply shared obligations without silently overriding them |
| Source baseline | Catalog's inspected working tree and upstream reference; not a release |
| Approval / owners | No requirement sign-off; implementation, verification and release owners unassigned |

This TRD owns Open Graph's technical behavior and evidence expectations. Shared T-001 through T-004 and T-006 through T-009 apply; original T-005 is relocated here as the authoritative social-URL requirement, with a gateway redirect. New local records use `OG-TR-*`.

The plugin is an independent Razor class library consuming Plugin/Core. It overrides only `PostHtmlAsync`, has no `DependsOn` override, and performs no file/network operation in generation. It does not implement route loading, navigation, preview hosting or image fetching.

## OG-TR-001: Option and metadata behavior

**State / source:** Confirmed baseline; P-FR-003 and shared T-004; [hook](OpenGraphPlugin.cs), [component code](OpenGraphComponent.razor.cs) and [helper](OpenGraphPluginHelper.cs). Preserve fallback and attribution behavior.

Read nullable options by typed `TryGetValue` without mutation. Keep these rules unless a product change is explicitly scoped:

| Input / context | Current behavior |
| --- | --- |
| `TwitterSiteId`, `TwitterCreatorId` | Missing/null/non-string values are ignored; empty/whitespace values omit optional tags |
| `Document.Metadata.TwitterHandle` | Non-whitespace metadata overrides the creator option |
| Title/description | `UseContentMetadata` requires a document, no collection and a non-blank `SourcePath`. Use document title plus site title, and document description with null fallback; otherwise use site title/description |
| Creator scope | Hook retains creator only for `ContentKind.Post`; component suppresses it for a page or when `UseContentMetadata` is false |
| Site metadata | Use supplied `Site.Locale` and `Site.Title`; do not recompose engine locale/date routes |

The hook calls `UseContentMetadata(null, document)` because it has no collection parameter. A component can receive `Documents`; a source-less post can retain creator in the hook but not the component. This difference is OG-Q-003, not universal parity.

**Verification:** [Hook tests](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphPluginTests.cs), [bUnit tests](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphComponentTests.cs) and [helper tests](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphPluginHelperTests.cs) cover typed/default options, snapshots, metadata overrides, pages, collections and site fallbacks. Do not treat them as exhaustive edge-context coverage.

## OG-TR-002: Hook and component integration

**State / source:** Confirmed baseline; OG-FR-001 and shared P-FR-001/P-FR-004; T-001/T-002/T-003. Preserve host selection and output insertion.

Retain `Id="open-graph"` and non-empty implementation name. The hook checks cancellation, replaces all exact paired markers `<plugin:open-graph></plugin:open-graph>` case-insensitively and leaves unmarked HTML unchanged for otherwise valid inputs. It does not enforce host enablement or deduplicate tags.

The component calls `base.OnParametersSet()`, clears all derived fields and recomputes from the selected manifest/context. Razor emits nothing without a manifest. When the manifest exists but `Site` is absent, code-behind returns early while the Razor guard still permits empty/default tags. Do not claim valid social metadata for that missing-context case.

**Verification:** Existing tests cover paired/no/multiple markers and bUnit absent-manifest/selection changes. Self-closing README behavior needs host evidence (OG-Q-001); missing-site/removal/source-less cases remain OG-Q-003. Await the existing unawaited cancellation assertion before treating it as reliable evidence.

## T-005: Content and image URL boundaries

**State / source:** Confirmed baseline; P-FR-005, shared P-NFR-005; [local helper](OpenGraphPluginHelper.cs) and [upstream formatting contract](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/docs/website-documentation.md#shared-url-helpers). This retains the original catalog ID and obligation.

For non-blank slugs, use `ContentUrlHelper.GetContentUrl`: trim outer whitespace, normalize both slash separators, escape segments and reject literal `.`/`..` segments. Convert the helper's root result `.` to the site root. Null/blank local slugs also return the site root; this local fallback does not change upstream null-argument behavior.

Compose site-local metadata URLs with `SiteUrl` and `BaseUrl`. A configured origin makes social URLs absolute; this is not the base-relative navigation-link contract. Do not drop or double-prefix subpaths, infer slugs from filenames or independently add locale/date prefixes.

Hero images select a non-whitespace document image, otherwise the site image; absent images yield an empty string. Use `ContentUrlHelper.GetImageUrl`, not content-slug escaping, then compose. Absolute URIs are retained through `Uri.TryCreate`/`ToString`; this is not a scheme allowlist or byte-for-byte preservation guarantee. Without a site origin, the helper can return relative text rather than an absolute social URL.

**Verification:** Existing helper tests cover root/subpath/null behavior, segment escaping, slash normalization, invalid dot segments, image fallback and absolute HTTPS images. Hook/component tests cover subpaths. Additional image query/fragment/percent-encoding, trailing-slash and scheme cases remain needed before broader preservation/safety claims; helper tests do not establish host mounting.

## OG-TR-003: Output and preview boundaries

**State / source:** Confirmed baseline; OG-NFR-001, shared P-NFR-003 and T-006. Keep metadata context and external behavior explicit.

Ordinary Razor metadata expressions retain HTML encoding. The hook template substitutes metadata/options without explicit HTML encoding; it is not equivalent to the Razor path. Inspect text, attributes and URLs separately with synthetic values. Accepted values/errors and a URI-scheme policy remain OG-Q-002; do not infer a sanitizer from formatting or select a new allowlist in this document.

Neither rendering path uses `Site.IsPreview`. Generation does not fetch external images or call social providers; consumers can subsequently request image URLs. No crawler-acceptance, privacy, offline or delivery guarantee is claimed. Preview-policy changes belong in the plugin PRD (OG-Q-004).

**Verification:** Source inspection and scoped output tests when modifying contexts. Existing tests do not constitute comprehensive encoding/scheme or provider acceptance evidence.

## Traceability and verification

| Product baseline | Technical coverage | Evidence / limits |
| --- | --- | --- |
| P-FR-003 | OG-TR-001/003 | Option/fallback/tag assertions; parity/output gaps remain |
| P-FR-005 | T-005 | Helper/hook/component URLs; wider URI and host checks not established |
| OG-FR-001 | OG-TR-002 | Marker and selection tests; self-closing/missing-context/cancellation gaps remain |
| OG-NFR-001 | OG-TR-003 | Source-backed preview/external-reference boundary, not provider acceptance |
| Shared P-FR-001/P-FR-004 | OG-TR-002, T-001/T-002/T-003 | Exact identity, supported insertion and absent-manifest behavior |
| Shared P-FR-006 | T-009 and this pair | Gateway links, versioned baseline and evidence mapping |
| Shared P-NFR-001 | T-007/T-008 | Resolved graph, Release build, tests and package inspection |
| Shared P-NFR-002 | T-002/T-004, OG-TR-001/002 | Read-only options and observable errors/cancellation; evidence caveats retained |
| Shared P-NFR-003 | OG-TR-003, T-006 | Context-specific validation review; no comprehensive audit claim |
| Shared P-NFR-005 | T-005, OG-TR-001/003 | Resolved URLs/site locale; head-only output, no visible UI or measured performance contract |

Use [AGENTS.md](../../AGENTS.md) for commands. Verify this package's assembly, README, license, icon, symbols and dependency metadata under shared T-008. Shared Q-005 governs release/publishing decisions; package creation is not authorization to publish.

## Gaps and readiness

[OG-Q-001 through OG-Q-004](PRD.md#plugin-questions-and-acceptance-limits) remain authoritative: OG-TR-002 covers integration/cancellation gaps, OG-TR-001 covers metadata parity, T-005/OG-TR-003 cover URI/output gaps, and OG-TR-003 covers preview policy. Owners remain unassigned; the split closes no question.

No analytics/consent service, database, account system, remote-generation API, navigation subsystem or accessibility/browser-conformance program is part of this plugin baseline. External image and locale/output semantics remain applicable; future features require a separate applicability review.

**Readiness:** Review-ready against plugin PRD v0.1 and shared v0.2 baselines, not approved or implementation-ready for open policy changes. Plugin-specific portions of former root T-003/T-004/T-006 are now here; T-005 retains its ID. The [catalog](../../PRD.md#sources-and-review-status) retains source provenance; no new executed verification is claimed from document restructuring.
