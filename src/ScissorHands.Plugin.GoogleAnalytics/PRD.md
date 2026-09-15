# Google Analytics - Product requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin TRD](TRD.md) | [Usage](README.md)

## Baseline and scope

| Field | Value |
| --- | --- |
| Version / status | 0.7 / Implementation-ready |
| Last updated | 2026-09-15 |
| Parent baseline | Catalog PRD v0.8; shared requirements apply as described below |
| Delivery state | Implementation follow-up 2026-09-15: behavior implemented; targeted regressions and shared solution/sample/package checks passed. [Technical evidence](TRD.md#targeted-evidence-2026-09-15) retains external verification and release-authorization limits |
| Plugin ID | `google-analytics` |
| Owner | @justinyoo owns implementation, verification, support and release authorization |
| Sign-off | @justinyoo signed off v0.7 at commit `02fbfc029c7560e2dc24543ee99b6d3fdce2b669` on 2026-09-15 (UTC+09:00). Approval covers requirements and acceptance criteria, not completed implementation or publication authorization |
| Release stage | Preview; versioning and releases are independent of the upstream engine |

This PRD owns Google Analytics's purpose, scope, observable behavior and product acceptance. It inherits the local catalog's shared compatibility, identity, failure, output-integrity and authoring constraints. The [plugin TRD](TRD.md) owns configuration formats, integration contracts, current-code details and verification; the [README](README.md) provides usage instructions. Product policy is self-contained in these local documents; the engine is a compatibility dependency, not a source of additional product requirements.

The primary user is a site author who wants Google Analytics markup without changing the engine. A theme author chooses where to integrate it; visitors run the emitted browser code. The existing plugin establishes the current experience, not proof of real provider delivery or measured user benefit. Technical evidence is recorded in the TRD.

In scope: measurement-ID configuration, layout-component and paired-placeholder integration, package compatibility and documentation. Out of scope: analytics dashboards, server-side measurement requests, consent-management UI, provider retention/deletion controls, automatic preview suppression, or other analytics providers. The plugin adds no visible interactive controls; browser/network/privacy behavior still matters. There is no plugin-owned database, account system, or measured performance target.

## User journey and outcomes

The author installs the plugin in a compatible host, enables it with a supported measurement identifier, and chooses one integration path per intended insertion. Removing the plugin's configuration entry disables output; leaving an enabled entry without the required identifier is invalid. Invalid configuration produces a clear error rather than silently disabling analytics or emitting an empty identifier.

Success is reusable analytics-markup integration without engine changes. An integration demonstration could measure this outcome; adoption, time savings, evaluation windows and quantitative targets are not established. Rendering a script is acceptance of markup behavior, not proof of analytics collection.

Authors may use a layout component or paired placeholders; self-closing placeholders are not supported. Combining both paths can duplicate output, so choose one unless duplication is intended. Configuration examples and exact syntax are owned by [GA-TR-001](TRD.md#ga-tr-001-measurement-configuration) and [GA-TR-002](TRD.md#ga-tr-002-hook-and-component-integration).

## Requirements

The following records define the **accepted policy** following the user's 2026-09-14 decision to adopt the review recommendations and 2026-09-15 requirements sign-off. The implementation follow-up delivers these scoped behaviors; targeted evidence and its limits are recorded separately in the TRD. Preserve `P-FR-002` and `P-NFR-004` from catalog v0.1. New local IDs use `GA-*`.

| ID | Need / required behavior | Observable acceptance and limits |
| --- | --- | --- |
| P-FR-002 | Site authors configure the Google tag without silent misconfiguration | When enabled, require a supported measurement identifier. Missing or invalid configuration must fail clearly, never silently disable analytics or emit an empty identifier. Explicitly supported synthetic examples remain accepted; acceptance does not verify a Google property. Both integration paths must produce intact markup. The accepted identifier format and failure matrix are specified in [GA-TR-001](TRD.md#ga-tr-001-measurement-configuration) |
| GA-FR-001 | Theme authors control insertion and updates; specializes shared P-FR-001/P-FR-004 | Render at every requested insertion point; preserve content without an insertion request for otherwise valid inputs. Disabling the plugin removes output, and selection/configuration changes must not leave stale output. Multiple requested insertions may produce duplicates; no global deduplication is promised. Matching and refresh contracts are specified in [GA-TR-002](TRD.md#ga-tr-002-hook-and-component-integration) |
| P-NFR-004 | Site owners explicitly control enablement, including preview | Retain rendering in both preview and production when configured; no new automatic suppression switch is introduced. The fake-ID sample remains enabled. Generation makes no measurement request, but visiting output can contact Google even with a fake ID. Consent integration and provider-side retention/deletion remain the consuming site's/provider's responsibility; no compliance, offline or delivery guarantee is provided |

Shared `P-NFR-001/002/003/005` apply: preserve compatibility and author-supplied configuration, expose observed failures/cancellation and protect output. The plugin depends on an external Google service; it does not own site navigation or localized visible UI. The TRD records the associated URL and client constraints.

## Plugin questions and acceptance limits

Retain the original question IDs as decision/routing records. The user confirmed these policies on 2026-09-14. Technical verification tracking belongs in the TRD; its relocation does not waive any accepted requirement.

| ID / origin | Decision or remaining work | Delivery state |
| --- | --- | --- |
| GA-Q-001 / Q-001 | Support paired placeholders, not self-closing ones | Settled; integration contract and evidence in [GA-TR-002](TRD.md#ga-tr-002-hook-and-component-integration) |
| GA-Q-002 / Q-002 | Reject invalid identifiers and preserve valid analytics output | Settled; accepted format and output assurance in [GA-TR-001](TRD.md#ga-tr-001-measurement-configuration) |
| GA-Q-003 / Q-003 | Technical verification record, not a product choice | Local regression evidence recorded under [GA-TR-002](TRD.md#ga-tr-002-hook-and-component-integration) |
| GA-Q-004 / Q-004 | Keep configured preview output and host-owned consent; fail on invalid required configuration | Settled; delivery limits in [GA-TR-001](TRD.md#ga-tr-001-measurement-configuration) and [GA-TR-003](TRD.md#ga-tr-003-preview-and-privacy) |

Shared [Q-005](../../PRD.md#shared-release-question) governs independent preview releases, support, recovery and optional-only deferrals. Scoped consumer integration and package checks are complete in the [shared implementation evidence](../../TRD.md#implementation-evidence-2026-09-15). External publishing verification and release authorization remain separate; @justinyoo selects and authorizes the release. Real provider delivery is not claimed.

**Release impact:** stricter validation breaks configurations that rely on permissive identifier handling. Affected users must supply a supported identifier or disable the plugin by removing its manifest before adopting this implementation. Technical migration details are in [the TRD](TRD.md#gaps-and-readiness) and [README](README.md#configuration-and-breaking-migration).

**Implementation follow-up (2026-09-15, UTC+09:00):** strict, non-leaking configuration failures and preserved insertion, update, disabled and preview behavior are implemented and regression-tested, with shared consumer/package checks completed. The original v0.7 sign-off remains requirements approval, not runtime or release approval. External publication, provider acceptance and a comprehensive audit remain unverified; see [technical evidence and limits](TRD.md#gaps-and-readiness). Release authorization remains under shared Q-005. Source provenance remains in the [catalog](../../PRD.md#sources-and-review-status).

**v0.3 clarification:** recorded @justinyoo's ownership and engine-independent preview releases; it did not approve a recovery policy or a specific deferral.

**v0.4 alignment (2026-09-14):** adopts catalog v0.5's accepted support/recovery and optional-only deferral policies. No runtime change, specific deferral or publication is authorized by this policy update.

**v0.5 confirmation (2026-09-14):** records explicit confirmation of validation, output handling and regression requirements and aligns with catalog v0.6's publishing-setup evidence. Existing IDs, scope and migration effects are preserved; no runtime delivery is claimed.

**v0.6 separation (2026-09-14):** move configuration examples, exact integration/validation contracts, current-code details and verification tracking to the TRD. Retain product acceptance and release impact here. GA-Q-003 remains a redirect to its technical owner; no policy, ID or delivery obligation is removed.

**v0.7 reference policy (2026-09-14):** align with catalog v0.8's local authority and API-only external-reference policy. Accepted behavior, IDs, evidence gaps and implementation readiness remain unchanged.
