# Open Graph - Product requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin TRD](TRD.md) | [Usage](README.md)

## Baseline and scope

| Field | Value |
| --- | --- |
| Version / status | 0.1 / Review-ready |
| Last updated | 2026-09-14 |
| Parent baseline | Catalog PRD v0.2; shared requirements apply as described below |
| Implementation baseline | Catalog source baseline, including pending local changes; not a published release |
| Package / plugin ID | `ScissorHands.Plugin.OpenGraph` / `open-graph` |
| Approval / owners | Documentation split requested; requirement sign-off and release/verification owners are not established |

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

Use `<OpenGraphComponent Id="open-graph" />` within the upstream cascade, or the supported paired marker `<plugin:open-graph></plugin:open-graph>`. Select one path per intended insertion. Do not assume the self-closing README example is equivalent without resolving OG-Q-001.

## Requirements

All records are **Confirmed current baseline**, scoped to maintaining this plugin unless a change is explicitly agreed. `P-FR-003` and `P-FR-005` retain their catalog v0.1 IDs and now have authoritative records here. New local IDs use `OG-*`.

| ID | Need / required behavior | Observable acceptance and limits |
| --- | --- | --- |
| P-FR-003 | Site authors supply social metadata | Emit Open Graph and Twitter-card title, description and image plus the relevant site/locale/URL metadata. Source-backed document titles use document plus site title; null document descriptions fall back to the site description. Collection/source-less contexts use site title/description. Optional Twitter identifiers and creator overrides follow [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior), including existing surface differences |
| P-FR-005 | Sharing clients receive intended publication URLs | With `SiteUrl=https://example.com`, `BaseUrl=/blog/`, slug `guides/about & team` yields `https://example.com/blog/guides/about%20%26%20team`. Root slugs yield the site root; invalid literal dot segments fail. External HTTP(S) hero images are not site-prefixed. Missing images yield an empty string; no origin means no guarantee of an absolute social URL |
| OG-FR-001 | Theme authors control insertion and updates; specializes shared P-FR-001/P-FR-004 | Hook replaces all supported paired markers case-insensitively and leaves unmarked HTML unchanged for valid inputs. Component emits nothing without a matched manifest and refreshes selected-manifest state. Full parity and deduplication are not promised |
| OG-NFR-001 | Authors know preview/external-reference behavior; retains the Open Graph part of former P-NFR-004 | The plugin ignores `Site.IsPreview` and generates metadata in both modes. Generation does not fetch images or call a social platform; consumers may later request external image URLs. No provider-delivery or privacy conformance claim follows |

Shared `P-NFR-001/002/003/005` govern compatibility, read-only options, failures/cancellation, output boundaries and resolved route/locale context. Preserve the supplied slug and site locale rather than independently composing engine locale/date routes. No analytics policy is inherited from the sibling plugin.

## Plugin questions and acceptance limits

These questions retain the Open Graph parts of catalog v0.1 Q-001 through Q-004. Owners and dates remain unassigned.

| ID / origin | Issue | Impact / next action |
| --- | --- | --- |
| OG-Q-001 / Q-001 | Self-closing README marker versus paired-hook implementation | Verify actual host normalization or reconcile documented support before claiming working examples |
| OG-Q-002 / Q-002 | Raw metadata substitution and general URI handling lack complete context-specific validation evidence | Define accepted values/errors and verify text, attribute and URI cases; formatting helpers are not scheme sanitizers |
| OG-Q-003 / Q-003 | Source-less creator behavior differs between hook/component; absent site produces default tags; cancellation assertions are not awaited | Decide intended edge behavior before changing it and strengthen evidence; no full parity/cancellation claim is supported |
| OG-Q-004 / Q-004 | Preview output is current behavior; a different preview policy is not agreed | A suppression/change requires a scoped product decision, not an incidental documentation or dependency update |

Shared [Q-005](../../PRD.md#shared-release-question) covers package versioning/publishing and support policy. Apply the catalog's proposed release gates and this plugin's TRD verification; actual crawler acceptance, release authorization and timing are not established.

**Readiness:** Review-ready for this existing baseline, not approved or implementation-ready for open policy changes. This document relocates requirements without changing runtime scope. The [catalog source record](../../PRD.md#sources-and-review-status) retains provenance; upstream engine approval history is not inherited.
