# Google Analytics - Product requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin TRD](TRD.md) | [Usage](README.md)

## Baseline and scope

| Field | Value |
| --- | --- |
| Version / status | 0.4 / Review-ready |
| Last updated | 2026-09-14 |
| Parent baseline | Catalog PRD v0.5; shared requirements apply as described below |
| Implementation baseline | Commit `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`; accepted target changes below are not yet implemented |
| Package / plugin ID | `ScissorHands.Plugin.GoogleAnalytics` / `google-analytics` |
| Approval / owner | @justinyoo owns implementation, verification, support and release authorization; policy recommendations accepted on 2026-09-14, not authorization to publish |
| Release stage | Preview; versioning and releases are independent of the upstream engine |

This PRD owns Google Analytics behavior, acceptance and plugin-specific questions. It inherits the catalog's shared compatibility, identity, failure, output-integrity and authoring constraints; it cannot silently override them. The [plugin TRD](TRD.md) supplies technical acceptance. No engine requirements or upstream approvals are imported.

The primary user is a site author who wants Google Analytics markup without changing the engine. A theme author chooses where to integrate it; visitors run the emitted browser code. The [implementation](GoogleAnalyticsPlugin.cs), [component](GoogleAnalyticsComponent.razor), and [tests](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests) are the source of the baseline below, not evidence of real provider delivery or measured user benefit.

In scope: measurement-ID configuration, the post-HTML and Razor paths, package compatibility and documentation. Out of scope: analytics dashboards, server-side measurement requests, consent-management UI, provider retention/deletion controls, automatic preview suppression, or other analytics providers. The plugin adds no visible interactive controls; browser/network/privacy behavior still matters. There is no plugin-owned database, account system, or measured performance target.

## User journey and outcomes

The author installs the package in a compatible host, configures ID `google-analytics` and a `MeasurementId`, then chooses one integration path for each intended insertion. Removing the manifest disables the plugin's host hooks and component output; it does not unload its assembly. Under the accepted target policy, invalid configuration fails rather than silently disabling the plugin or emitting an empty ID. Current code still emits an empty ID for missing/non-string values.

Success is reusable analytics-markup integration without engine changes. An integration demonstration could measure this outcome; adoption, time savings, evaluation windows and quantitative targets are not established. Rendering a script is acceptance of markup behavior, not proof of analytics collection.

```json
{
  "Plugins": [
    { "Id": "google-analytics", "Options": { "MeasurementId": "G-EXAMPLE" } }
  ]
}
```

Use `<GoogleAnalyticsComponent Id="google-analytics" />` in a layout supplying upstream cascading context, or the paired hook marker `<plugin:google-analytics></plugin:google-analytics>`. Only paired hook markers are part of the supported contract; self-closing marker support is not being added. Do not combine paths unless duplicate output is intended.

## Requirements

The following records define the **accepted target policy** following the user's 2026-09-14 decision to adopt the review recommendations. Existing behavior is distinguished from pending changes below; agreement is not implementation or release evidence. Preserve `P-FR-002` and `P-NFR-004` from catalog v0.1. New local IDs use `GA-*`.

| ID | Need / required behavior | Observable acceptance and limits |
| --- | --- | --- |
| P-FR-002 | Site authors configure the Google tag without silent misconfiguration | When enabled, require a string ID using `G-` followed by one or more uppercase ASCII letters/digits. Preserve `G-EXAMPLE` as an explicitly supported synthetic value; syntax acceptance is not Google-property verification. Missing/null/non-string, blank, whitespace-padded and other malformed values must fail with a clear configuration error, not emit an empty ID. Handle JavaScript and URL output contexts correctly in both paths |
| GA-FR-001 | Theme authors control insertion and updates; specializes shared P-FR-001/P-FR-004 | Hook replaces every supported paired marker case-insensitively and preserves HTML with no marker for otherwise valid inputs. Component emits nothing without a matched manifest and recomputes when its selected ID changes. No global deduplication is promised |
| P-NFR-004 | Site owners explicitly control enablement, including preview | Retain rendering in both preview and production when configured; no new automatic suppression switch is introduced. The fake-ID sample remains enabled. Generation makes no measurement request, but visiting output can contact Google even with a fake ID. Consent integration and provider-side retention/deletion remain the consuming site's/provider's responsibility; no compliance, offline or delivery guarantee is provided |

Shared `P-NFR-001/002/003/005` apply: preserve package compatibility and read-only configuration, propagate observed failures/cancellation, review each output context, and assess client/locale impact. The loader URL is an external Google URL, not a site-local asset to prefix with `Site.BaseUrl`; this plugin does not calculate content routes or translate visible UI.

## Plugin questions and acceptance limits

Retain the original question IDs as decision/evidence records. The user accepted the recommendations on 2026-09-14. This revision interprets the remaining preview choice as retaining enabled-when-configured behavior, consistent with the user's fake-ID sample request; the restricted ID shape above is the technical acceptance elaboration, not a claim about provider registration.

| ID / origin | Decision or remaining work | Delivery state |
| --- | --- | --- |
| GA-Q-001 / Q-001 | Standardize on paired hook markers; do not add self-closing support | Decision settled; README examples corrected to the supported form |
| GA-Q-002 / Q-002 | Restrict IDs as in P-FR-002 and protect JavaScript/URL output contexts | Policy recorded; validation/encoding implementation and synthetic boundary evidence pending |
| GA-Q-003 / Q-003 | Await cancellation assertions and cover component removal transitions | Engineering follow-up, not an unresolved product choice; evidence remains incomplete |
| GA-Q-004 / Q-004 | Reject invalid required IDs, retain configured preview output, and keep consent outside the plugin | Decisions recorded; rejection behavior pending. Preview/consent boundaries already match the target |

Shared [Q-005](../../PRD.md#shared-release-question) governs versioning, verified compatibility and release gates. A release claiming this target requires the new validation/output regressions and consumer evidence; passing the older permissive tests is not acceptance. @justinyoo selects the next independent preview version/date and authorizes publication. The shared support/recovery and optional-only deferral policies are accepted; this plugin's agreed behavior and required evidence remain pending, not deferred. Real provider acceptance remains unverified and outside the plugin's delivery claims.

**Migration / current behavior:** strict validation is a breaking behavior change for callers relying on empty or arbitrary IDs. Configure an accepted ID or remove the manifest before adopting that future implementation. The current hook/component code still accepts arbitrary strings and falls back to an empty ID; this documentation revision does not change it.

**Readiness:** Review-ready with accepted policy direction and explicit technical elaboration; not a completed implementation, full-document sign-off or release approval. v0.2 replaces the earlier unresolved policy alternatives while preserving question IDs and evidence gaps. Source provenance remains in the [catalog](../../PRD.md#sources-and-review-status).

**v0.3 clarification:** recorded @justinyoo's ownership and engine-independent preview releases; it did not approve a recovery policy or a specific deferral.

**v0.4 alignment (2026-09-14):** adopts catalog v0.5's accepted support/recovery and optional-only deferral policies. No runtime change, specific deferral or publication is authorized by this policy update.
