# Open Graph - Product requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin TRD](TRD.md) | [Usage](README.md)

## Baseline and scope

| Field | Value |
| --- | --- |
| Version / status | 0.6 / Implementation-ready |
| Last updated | 2026-09-14 |
| Parent baseline | Catalog PRD v0.7; shared requirements apply as described below |
| Delivery state | Accepted changes remain pending; implementation baseline and evidence are owned by the [TRD](TRD.md#baseline-and-inheritance) |
| Plugin ID | `open-graph` |
| Approval / owner | @justinyoo owns implementation, verification, support and release authorization; metadata, URL/output and regression requirements explicitly confirmed on 2026-09-14, not authorization to publish |
| Release stage | Preview; versioning and releases are independent of the upstream engine |

This PRD owns Open Graph's purpose, scope, observable behavior and product acceptance. It inherits catalog requirements without changing their meaning. The [plugin TRD](TRD.md) owns configuration formats, integration contracts, current-code details and verification; the [README](README.md) provides usage instructions. ScissorHands.NET remains authoritative for external contracts.

The primary user is a site author wanting social metadata without engine changes. A theme author integrates the plugin; browsers and sharing clients consume the result. The existing plugin establishes the current experience, not guaranteed crawler behavior or measured adoption. Technical evidence is recorded in the TRD.

In scope: Open Graph/Twitter-card metadata, optional Twitter identifiers, document/site fallback rules, metadata URLs, and both integration paths. Out of scope: fetching remote images, social-platform APIs, guaranteed rich-preview appearance, navigation/route generation, analytics or consent services. There are no visible interactive controls, plugin-owned accounts/storage, or performance thresholds; output encoding and locale/client behavior remain relevant.

## User journey and outcomes

The author installs the plugin in a compatible host, enables it, optionally supplies Twitter identifiers, and chooses a layout component or paired placeholders. Site address, subpath and content metadata describe the intended publication. Removing the plugin's configuration entry disables output; absent Twitter identifiers do not disable the remaining metadata.

Success is reusable social metadata that corresponds to the site's content and addresses. Candidate evaluation is a consumer integration with inspected metadata at root and subpath deployments. Numerical targets, adoption, evaluation windows and real sharing-client results are unknown.

Select one path per intended insertion to avoid duplicate metadata. Only paired placeholders are supported; self-closing support is not being added. Configuration examples and exact syntax are owned by [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) and [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration).

## Requirements

These records define the **accepted target policy** following the user's 2026-09-14 adoption of the review recommendations. Current behavior and implementation gaps are recorded below, separately from agreement. `P-FR-003` and `P-FR-005` retain their catalog v0.1 IDs; new local IDs use `OG-*`.

| ID | Need / required behavior | Observable acceptance and limits |
| --- | --- | --- |
| P-FR-003 | Site authors receive consistent social metadata from either integration | Equivalent context produces the same metadata values and optional-field presence; formatting need not be identical. Preserve content/site title and description fallbacks. Attribute a creator only to an individual post backed by source content, not pages, collections or source-less posts. Twitter identifiers remain optional. Titles and descriptions remain text, not injected markup. Field precedence and eligibility are defined in [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) |
| P-FR-005 | Sharing clients receive valid publication URLs without empty image metadata | Require site context and a valid publication address when producing metadata; otherwise fail clearly. Honor root and subpath deployments. Support local images and external web images without changing the meaning of their references; reject unsupported or malformed addresses. If no content or site image is available, omit image metadata instead of emitting empty values. The accepted URL forms and omission contract are defined in [T-005](TRD.md#t-005-content-and-image-url-boundaries) |
| OG-FR-001 | Theme authors control insertion and updates; specializes shared P-FR-001/P-FR-004 | Render at every requested insertion and preserve content without an insertion request for valid inputs. Disabling removes output; enabling without required context fails clearly. Selection/context changes must not leave stale metadata. Do not add global deduplication. Matching and refresh contracts are defined in [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration) |
| OG-NFR-001 | Authors inspect the same metadata in preview and production | Keep Open Graph enabled in both modes when configured, subject to the same validation rules. Generation does not fetch images or call a social platform; consumers may later request external image URLs. No provider-delivery or privacy conformance claim follows |

Shared `P-NFR-001/002/003/005` govern compatibility, author-supplied configuration, observable failures/cancellation, output integrity and the site's route/locale context. Preserve the host's intended publication structure rather than inventing navigation or locale routes. No analytics policy is inherited from the sibling plugin.

OG-NFR-001 retains the Open Graph portion of former P-NFR-004. [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) elaborates metadata parity without dropping the existing optional-value and title/description rules.

## Plugin questions and acceptance limits

Retain the original question IDs as decision/routing records. The user confirmed these policies on 2026-09-14. Exact field and URL rules, current-code differences and verification tracking belong in the TRD; relocation does not change source-backed-post eligibility, fallback behavior or preview policy.

| ID / origin | Decision or remaining work | Delivery state |
| --- | --- | --- |
| OG-Q-001 / Q-001 | Support paired placeholders, not self-closing ones | Settled; integration contract and evidence in [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration) |
| OG-Q-002 / Q-002 | Preserve text and supported image references; omit unavailable images | Settled; URL contract in [T-005](TRD.md#t-005-content-and-image-url-boundaries), output assurance in [OG-TR-003](TRD.md#og-tr-003-output-and-preview-boundaries) |
| OG-Q-003 / Q-003 | Keep both paths consistent and reject missing required publication context | Settled; eligibility and technical verification in [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) and [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration) |
| OG-Q-004 / Q-004 | Retain metadata generation in preview under the production rules | Settled; no suppression feature; technical boundary in [OG-TR-003](TRD.md#og-tr-003-output-and-preview-boundaries) |

Shared [Q-005](../../PRD.md#shared-release-question) governs independent preview releases, support, recovery and optional-only deferrals. A release claiming the target behavior requires evidence for that behavior and scoped consumer integration. @justinyoo selects and authorizes the release. Agreed behavior and evidence remain pending, not deferred; crawler delivery is not claimed.

**Release impact:** stricter publication-address validation and more selective creator/image metadata are breaking changes for affected consumers. They must supply valid site information, correct any unsupported image references and account for fields that may no longer appear; images remain optional. Current-code differences and technical migration details are in [the TRD](TRD.md#gaps-and-readiness); this revision does not deliver the runtime changes.

**Readiness:** Implementation-ready: scope, metadata parity, required/optional inputs, URL/output semantics and regression expectations are explicitly confirmed, with no unresolved policy decision blocking implementation. Runtime changes and evidence remain pending, not deferred. Crawler acceptance is outside the claimed scope; publishing verification remains a release gate under shared Q-005. Document readiness is not completed implementation, a completed audit or release approval. The [catalog source record](../../PRD.md#sources-and-review-status) retains provenance; upstream engine approval history is not inherited.

**v0.3 clarification:** recorded @justinyoo's ownership and engine-independent preview releases; it did not approve a recovery policy or a specific deferral.

**v0.4 alignment (2026-09-14):** adopts catalog v0.5's accepted support/recovery and optional-only deferral policies. No runtime change, specific deferral or publication is authorized by this policy update.

**v0.5 confirmation (2026-09-14):** records explicit confirmation of metadata consistency, required/optional inputs, URL/output handling and regressions and aligns with catalog v0.6's publishing-setup evidence. Existing IDs, scope and migration effects are preserved; no runtime delivery is claimed.

**v0.6 separation (2026-09-14):** move configuration examples, exact integration/URL contracts, current-code details and verification tracking to the TRD. Retain product acceptance and release impact here. Existing requirement/question IDs and delivery obligations remain intact.
