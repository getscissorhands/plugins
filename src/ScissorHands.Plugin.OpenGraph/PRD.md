# Open Graph - Product requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin TRD](TRD.md) | [Usage](README.md)

## Baseline and scope

| Field | Value |
| --- | --- |
| Version / status | 0.3 / Review-ready |
| Last updated | 2026-09-14 |
| Parent baseline | Catalog PRD v0.4; shared requirements apply as described below |
| Implementation baseline | Commit `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`; accepted target changes below are not yet implemented |
| Package / plugin ID | `ScissorHands.Plugin.OpenGraph` / `open-graph` |
| Approval / owner | @justinyoo owns implementation, verification, support and release authorization; policy recommendations accepted on 2026-09-14, not authorization to publish |
| Release stage | Preview; versioning and releases are independent of the upstream engine |

This PRD owns Open Graph behavior, acceptance and plugin-specific questions. It inherits catalog requirements rather than duplicating them or changing their meaning. The [plugin TRD](TRD.md) defines technical acceptance; ScissorHands.NET remains authoritative for external contracts.

The primary user is a site author wanting social metadata without engine changes. A theme author integrates the plugin; browsers and sharing clients consume the result. The [hook](OpenGraphPlugin.cs), [component](OpenGraphComponent.razor.cs), [helper](OpenGraphPluginHelper.cs) and [tests](../../test/ScissorHands.Plugin.OpenGraph.Tests) establish the current baseline, not guaranteed crawler behavior or measured adoption.

In scope: Open Graph/Twitter-card metadata, optional Twitter identifiers, document/site fallback rules, metadata URLs, and both integration paths. Out of scope: fetching remote images, social-platform APIs, guaranteed rich-preview appearance, navigation/route generation, analytics or consent services. There are no visible interactive controls, plugin-owned accounts/storage, or performance thresholds; output encoding and locale/client behavior remain relevant.

## User journey and outcomes

The author installs the package in a compatible host, configures `open-graph`, optionally supplies Twitter identifiers, and chooses a Razor component or hook marker. Configure site URL/base path and content metadata so social URLs describe the intended publication. Removing the manifest disables host hooks/component output; absent Twitter identifiers do not disable the remaining metadata.

Success is reusable social metadata that corresponds to the site's content and addresses. Candidate evaluation is a consumer integration with inspected metadata at root and subpath deployments. Numerical targets, adoption, evaluation windows and real sharing-client results are unknown.

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

Use `<OpenGraphComponent Id="open-graph" />` within the upstream cascade, or the paired marker `<plugin:open-graph></plugin:open-graph>`. Select one path per intended insertion. Only paired hook markers are supported; self-closing support is not being added.

## Requirements

These records define the **accepted target policy** following the user's 2026-09-14 adoption of the review recommendations. Current behavior and implementation gaps are recorded below, separately from agreement. `P-FR-003` and `P-FR-005` retain their catalog v0.1 IDs; new local IDs use `OG-*`.

| ID | Need / required behavior | Observable acceptance and limits |
| --- | --- | --- |
| P-FR-003 | Site authors receive consistent social metadata from either integration | For equivalent context, both paths must emit equivalent metadata values and optional-tag presence, not necessarily identical whitespace/serialization. Preserve document/site title/description fallbacks. Emit creator only for an individual source-backed post; omit it for pages, collections and source-less posts. Keep absent optional Twitter identifiers optional. Render normal metadata as text, not injected HTML |
| P-FR-005 | Sharing clients receive valid publication URLs without empty image tags | Require site context and a valid absolute HTTP(S) `SiteUrl` when emitting metadata; otherwise fail clearly. Preserve root/subpath and segment-escaping behavior. Accept site-local image references and absolute HTTP(S) URLs, reject unsupported schemes/malformed references, and preserve supported queries/fragments/percent encoding. If neither document nor site supplies an image, omit `og:image` and `twitter:image` rather than emit empty values |
| OG-FR-001 | Theme authors control insertion and updates; specializes shared P-FR-001/P-FR-004 | Preserve case-insensitive replace-all paired-marker behavior and unchanged unmarked HTML for valid inputs. An absent manifest emits no component output; a matched manifest with missing required context fails. Refresh selection/context state. Do not add global deduplication or require byte-identical formatting for parity |
| OG-NFR-001 | Authors inspect the same metadata in preview and production | Keep Open Graph enabled in both modes when configured, subject to the same validation rules. Generation does not fetch images or call a social platform; consumers may later request external image URLs. No provider-delivery or privacy conformance claim follows |

Shared `P-NFR-001/002/003/005` govern compatibility, read-only options, failures/cancellation, output boundaries and resolved route/locale context. Preserve the supplied slug and site locale rather than independently composing engine locale/date routes. No analytics policy is inherited from the sibling plugin.

OG-NFR-001 retains the Open Graph portion of former P-NFR-004. [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) elaborates metadata parity without dropping the existing optional-value and title/description rules.

## Plugin questions and acceptance limits

Retain the original question IDs as decision/evidence records. The user accepted the recommendations on 2026-09-14. To resolve the remaining parity detail, this revision uses the component's existing source-backed-post eligibility as the common creator rule; it does not change source-less title/description fallback policy.

| ID / origin | Decision or remaining work | Delivery state |
| --- | --- | --- |
| OG-Q-001 / Q-001 | Standardize on paired hook markers; do not add self-closing support | Decision settled; README examples corrected |
| OG-Q-002 / Q-002 | Treat metadata as text; allow site-local and HTTP(S) images, reject unsupported schemes, preserve supported URL details and omit unavailable image tags | Policy recorded; implementation and URI/output boundary evidence pending |
| OG-Q-003 / Q-003 | Align equivalent contexts using source-backed-post creator eligibility and fail on missing site/origin context | Policy recorded; parity/context changes and awaited cancellation/removal regressions pending |
| OG-Q-004 / Q-004 | Retain metadata generation in preview with the same rules as production | Decision settled; no suppression feature is introduced |

Shared [Q-005](../../PRD.md#shared-release-question) covers verified compatibility and release gates. A release claiming the accepted behavior requires parity, context, omission and URL/output regressions plus consumer evidence. @justinyoo selects the next independent preview version/date and authorizes publication. Recovery/deferral details and crawler acceptance remain unsettled or unverified as applicable.

**Migration / current behavior:** the current hook still permits a creator for source-less posts, missing-site components can emit defaults, and image/helper paths can return empty or relative values and accept general absolute URI schemes. The stricter target will require valid site configuration and accepted URLs and may remove formerly emitted creator/image tags. It is a breaking behavior change, not an implemented fix in this revision.

**Readiness:** Review-ready with accepted policy direction and explicit technical elaboration, not completed implementation, whole-document sign-off or release approval. v0.2 resolves policy alternatives while retaining delivery/evidence gaps and IDs. The [catalog source record](../../PRD.md#sources-and-review-status) retains provenance; upstream engine approval history is not inherited.

**v0.3 clarification:** records @justinyoo's ownership and engine-independent preview releases. No new behavior, recovery commitment or nonblocking deferral is approved.
