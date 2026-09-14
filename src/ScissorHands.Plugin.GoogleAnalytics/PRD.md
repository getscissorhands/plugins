# Google Analytics - Product requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin TRD](TRD.md) | [Usage](README.md)

## Baseline and scope

| Field | Value |
| --- | --- |
| Version / status | 0.1 / Review-ready |
| Last updated | 2026-09-14 |
| Parent baseline | Catalog PRD v0.2; shared requirements apply as described below |
| Implementation baseline | Catalog source baseline, including pending local changes; not a published release |
| Package / plugin ID | `ScissorHands.Plugin.GoogleAnalytics` / `google-analytics` |
| Approval / owners | Documentation split requested; requirement sign-off and release/verification owners are not established |

This PRD owns Google Analytics behavior, acceptance and plugin-specific questions. It inherits the catalog's shared compatibility, identity, failure, output-integrity and authoring constraints; it cannot silently override them. The [plugin TRD](TRD.md) supplies technical acceptance. No engine requirements or upstream approvals are imported.

The primary user is a site author who wants Google Analytics markup without changing the engine. A theme author chooses where to integrate it; visitors run the emitted browser code. The [implementation](GoogleAnalyticsPlugin.cs), [component](GoogleAnalyticsComponent.razor), and [tests](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests) are the source of the baseline below, not evidence of real provider delivery or measured user benefit.

In scope: measurement-ID configuration, the post-HTML and Razor paths, package compatibility and documentation. Out of scope: analytics dashboards, server-side measurement requests, consent-management UI, provider retention/deletion controls, automatic preview suppression, or other analytics providers. The plugin adds no visible interactive controls; browser/network/privacy behavior still matters. There is no plugin-owned database, account system, or measured performance target.

## User journey and outcomes

The author installs the package in a compatible host, configures ID `google-analytics` and an example `MeasurementId`, then chooses one integration path for each intended insertion. Removing the manifest disables the plugin's host hooks and component output; it does not unload its assembly. Missing options currently do not disable it.

Success is reusable analytics-markup integration without engine changes. An integration demonstration could measure this outcome; adoption, time savings, evaluation windows and quantitative targets are not established. Rendering a script is acceptance of markup behavior, not proof of analytics collection.

```json
{
  "Plugins": [
    { "Id": "google-analytics", "Options": { "MeasurementId": "G-EXAMPLE" } }
  ]
}
```

Use `<GoogleAnalyticsComponent Id="google-analytics" />` in a layout supplying upstream cascading context, or the supported paired hook marker `<plugin:google-analytics></plugin:google-analytics>`. Do not combine paths unless duplicate output is intended. The self-closing README form has the unresolved status in GA-Q-001.

## Requirements

All records are **Confirmed current baseline**, scoped to maintaining this plugin unless an explicit change is agreed. Preserve `P-FR-002` and `P-NFR-004` from catalog v0.1; they now have their authoritative Google Analytics records here. New local IDs use `GA-*`.

| ID | Need / required behavior | Observable acceptance and limits |
| --- | --- | --- |
| P-FR-002 | Site authors configure the Google tag | A string `MeasurementId` appears in the Google tag loader/configuration through either supported path. Null options or a missing/null/non-string value yield an empty ID. No format/whitespace validation or error is implemented; GA-Q-004 governs stricter behavior |
| GA-FR-001 | Theme authors control insertion and updates; specializes shared P-FR-001/P-FR-004 | Hook replaces every supported paired marker case-insensitively and preserves HTML with no marker for otherwise valid inputs. Component emits nothing without a matched manifest and recomputes when its selected ID changes. No global deduplication is promised |
| P-NFR-004 | Site owners understand preview/privacy behavior | The plugin ignores `Site.IsPreview`; enabled preview output includes the analytics script. Generation makes no measurement request, but visiting the output can contact Google. Consent integration and provider-side retention/deletion are external; no compliance, offline or delivery guarantee is provided |

Shared `P-NFR-001/002/003/005` apply: preserve package compatibility and read-only configuration, propagate observed failures/cancellation, review each output context, and assess client/locale impact. The loader URL is an external Google URL, not a site-local asset to prefix with `Site.BaseUrl`; this plugin does not calculate content routes or translate visible UI.

## Plugin questions and acceptance limits

These questions retain the relevant parts of catalog v0.1's Q-001 through Q-004. Owners and decision dates are unassigned; the current behavior is not changed by this document.

| ID / origin | Issue | Impact / next action |
| --- | --- | --- |
| GA-Q-001 / Q-001 | README shows a self-closing marker; the hook/test baseline supports paired markers | Verify host normalization or reconcile the example before claiming end-to-end support |
| GA-Q-002 / Q-002 | The measurement ID is inserted into raw script/URL contexts; exhaustive context-specific validation evidence is absent | Define accepted values and failure policy, then verify synthetic quote/script/URL inputs before claiming safe arbitrary strings |
| GA-Q-003 / Q-003 | Cancellation assertions are not awaited; component removal cases are not fully covered | Improve evidence when scoped; current tests do not establish complete cancellation or stale-output coverage |
| GA-Q-004 / Q-004 | Empty IDs, preview suppression and consent behavior need a decision before stricter behavior is implemented | No change from this PRD alone; consult the site owner on product policy before implementation |

Shared [Q-005](../../PRD.md#shared-release-question) governs package versioning/publishing and support policy. Use the catalog's proposed release gates plus the plugin TRD's evidence mapping. Release authorization, timing and provider acceptance are not established.

**Readiness:** Review-ready for the documented baseline, not approved or implementation-ready for the unresolved policy changes. This initial plugin document relocates existing behavior and gaps without introducing a feature or closing a question. Source/approval provenance is retained in the [catalog source record](../../PRD.md#sources-and-review-status).
