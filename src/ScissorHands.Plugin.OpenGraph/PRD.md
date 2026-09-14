# Open Graph - Product requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin TRD](TRD.md) | [Usage](README.md)

## Baseline and scope

| Field | Value |
| --- | --- |
| Version / status | 0.7 / Implementation-ready |
| Last updated | 2026-09-15 |
| Parent baseline | Catalog PRD v0.8; shared requirements apply as described below |
| Delivery state | Accepted behavior implemented in the delivery follow-up, with targeted local verification; release/integration limits and evidence are owned by the [TRD](TRD.md#delivery-evidence) |
| Plugin ID | `open-graph` |
| Owner | @justinyoo owns implementation, verification, support and release authorization |
| Sign-off | @justinyoo signed off v0.7 at commit `02fbfc029c7560e2dc24543ee99b6d3fdce2b669` on 2026-09-15 (UTC+09:00). Approval covers requirements and acceptance criteria, not completed implementation or publication authorization |
| Release stage | Preview; versioning and releases are independent of the upstream engine |

This PRD owns Open Graph's purpose, scope, observable behavior and product acceptance. It inherits local catalog requirements without changing their meaning. The [plugin TRD](TRD.md) owns configuration formats, integration contracts, current-code details and verification; the [README](README.md) provides usage instructions. Product policy is self-contained in these local documents; the engine is a compatibility dependency, not a source of additional product requirements.

The primary user is a site author wanting social metadata without engine changes. A theme author integrates the plugin; browsers and sharing clients consume the result. The existing plugin establishes the current experience, not guaranteed crawler behavior or measured adoption. Technical evidence is recorded in the TRD.

In scope: Open Graph/Twitter-card metadata, optional Twitter identifiers, document/site fallback rules, metadata URLs, and both integration paths. Out of scope: fetching remote images, social-platform APIs, guaranteed rich-preview appearance, navigation/route generation, analytics or consent services. There are no visible interactive controls, plugin-owned accounts/storage, or performance thresholds; output encoding and locale/client behavior remain relevant.

## User journey and outcomes

The author installs the plugin in a compatible host, enables it, optionally supplies Twitter identifiers, and chooses a layout component or paired placeholders. Site address, subpath and content metadata describe the intended publication. Removing the plugin's configuration entry disables output; absent Twitter identifiers do not disable the remaining metadata.

Success is reusable social metadata that corresponds to the site's content and addresses. Candidate evaluation is a consumer integration with inspected metadata at root and subpath deployments. Numerical targets, adoption, evaluation windows and real sharing-client results are unknown.

Select one path per intended insertion to avoid duplicate metadata. Only paired placeholders are supported; self-closing support is not being added. Configuration examples and exact syntax are owned by [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) and [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration).

## Requirements

These records define the **accepted product policy** following the user's 2026-09-14 adoption of the review recommendations. Delivery evidence is recorded separately from requirements approval. `P-FR-003` and `P-FR-005` retain their catalog v0.1 IDs; new local IDs use `OG-*`.

| ID | Need / required behavior | Observable acceptance and limits |
| --- | --- | --- |
| P-FR-003 | Site authors receive consistent social metadata from either integration | Equivalent context produces the same metadata values and optional-field presence; formatting need not be identical. Preserve content/site title and description fallbacks. Attribute a creator only to an individual post backed by source content, not pages, collections or source-less posts. Twitter identifiers remain optional. Titles and descriptions remain text, not injected markup. Field precedence and eligibility are defined in [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) |
| P-FR-005 | Sharing clients receive valid publication URLs without empty image metadata | Require site context and a valid publication address when producing metadata; otherwise fail clearly. Honor root and subpath deployments. Support local images and external web images without changing the meaning of their references; reject unsupported or malformed addresses. If no content or site image is available, omit image metadata instead of emitting empty values. The accepted URL forms and omission contract are defined in [T-005](TRD.md#t-005-content-and-image-url-boundaries) |
| OG-FR-001 | Theme authors control insertion and updates; specializes shared P-FR-001/P-FR-004 | Render at every requested insertion and preserve content without an insertion request for valid inputs. Disabling removes output; enabling without required context fails clearly. Selection/context changes must not leave stale metadata. Do not add global deduplication. Matching and refresh contracts are defined in [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration) |
| OG-NFR-001 | Authors inspect the same metadata in preview and production | Keep Open Graph enabled in both modes when configured, subject to the same validation rules. Generation does not fetch images or call a social platform; consumers may later request external image URLs. No provider-delivery or privacy conformance claim follows |

Shared `P-NFR-001/002/003/005` govern compatibility, author-supplied configuration, observable failures/cancellation, output integrity and the site's route/locale context. Preserve the host's intended publication structure rather than inventing navigation or locale routes. No analytics policy is inherited from the sibling plugin.

OG-NFR-001 retains the Open Graph portion of former P-NFR-004. [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) elaborates metadata parity without dropping the existing optional-value and title/description rules.

## Plugin questions and acceptance limits

Retain the original question IDs as decision/routing records. The user confirmed OG-Q-001 through OG-Q-004 policies on 2026-09-14; OG-Q-005 records a subsequently verified integration limitation. Exact field and URL rules, current-code differences and verification tracking belong in the TRD; relocation does not change source-backed-post eligibility, fallback behavior or preview policy.

| ID / origin | Decision or remaining work | Delivery state |
| --- | --- | --- |
| OG-Q-001 / Q-001 | Support paired placeholders, not self-closing ones | Settled; integration contract and evidence in [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration) |
| OG-Q-002 / Q-002 | Preserve text and supported image references; omit unavailable images | Settled; URL contract in [T-005](TRD.md#t-005-content-and-image-url-boundaries), output assurance in [OG-TR-003](TRD.md#og-tr-003-output-and-preview-boundaries) |
| OG-Q-003 / Q-003 | Keep both paths consistent and reject missing required publication context | Settled; eligibility and technical verification in [OG-TR-001](TRD.md#og-tr-001-option-and-metadata-behavior) and [OG-TR-002](TRD.md#og-tr-002-hook-and-component-integration) |
| OG-Q-004 / Q-004 | Retain metadata generation in preview under the production rules | Settled; no suppression feature; technical boundary in [OG-TR-003](TRD.md#og-tr-003-output-and-preview-boundaries) |
| OG-Q-005 / delivery finding | Generated tag pages receive different publication context in the two host integrations: component URLs point to the site root, while hooks receive the actual tag route | Open upstream integration limitation; use hooks for accurate generated tag-page URLs. Equivalent-input parity is verified, not full-host parity. Source evidence and closure conditions are in [OG-Q-005](TRD.md#og-q-005-generated-tag-pages-receive-unequal-host-context) |

OG-Q-005 does not authorize inferred tag routes or changing all source-less documents to site-root URLs. The host must supply resolved document/route context for equivalent component output. @justinyoo owns tracking that integration gap and revalidation; this is distinct from shared release-policy question Q-005. Configured preview/output validation remains unchanged.

Shared [Q-005](../../PRD.md#shared-release-question) governs independent preview releases, support, recovery and optional-only deferrals. A release claiming the behavior requires evidence for that behavior and scoped consumer integration. Completed full-suite, scoped sample and package-content validation is recorded in the [shared implementation evidence](../../TRD.md#implementation-evidence-2026-09-15), including the OG-Q-005 limit. @justinyoo selects and authorizes the release; actual publishing verification and authorization remain separate, and crawler delivery is not claimed.

**Release impact:** stricter publication-address validation, text-safe raw output and more selective creator/image metadata are breaking changes for affected consumers. They must supply valid site information, correct unsupported image references, supply metadata as text rather than markup and account for fields that may no longer appear; images remain optional. Technical migration details are in [the TRD](TRD.md#gaps-and-readiness) and usage/recovery steps in the [README](README.md#breaking-migration-from-the-earlier-permissive-behavior).

**Readiness:** The signed-off v0.7 requirements baseline remains implementation-ready. Its behavior under equivalent inputs has been delivered and verified, with completed scoped sample and package validation linked above; this is a delivery update, not a changed sign-off or new approval. OG-Q-005 remains an explicit host-integration gap, so full-host hook/component parity is not claimed. Crawler acceptance is outside the claimed scope; actual publishing verification remains a release gate under shared Q-005. Neither document approval nor validation is a completed audit or release approval. The [catalog source record](../../PRD.md#sources-and-review-status) records the local decision basis.

**v0.3 clarification:** recorded @justinyoo's ownership and engine-independent preview releases; it did not approve a recovery policy or a specific deferral.

**v0.4 alignment (2026-09-14):** adopts catalog v0.5's accepted support/recovery and optional-only deferral policies. No runtime change, specific deferral or publication is authorized by this policy update.

**v0.5 confirmation (2026-09-14):** records explicit confirmation of metadata consistency, required/optional inputs, URL/output handling and regressions and aligns with catalog v0.6's publishing-setup evidence. Existing IDs, scope and migration effects are preserved; no runtime delivery is claimed.

**v0.6 separation (2026-09-14):** move configuration examples, exact integration/URL contracts, current-code details and verification tracking to the TRD. Retain product acceptance and release impact here. Existing requirement/question IDs and delivery obligations remain intact.

**v0.7 reference policy (2026-09-14):** align with catalog v0.8's local authority and API-only external-reference policy. Accepted behavior, IDs, evidence gaps and implementation readiness remain unchanged.

**Delivery follow-up (2026-09-15, UTC+09:00):** implements consistent metadata, explicit required-context failures, optional-image omission, supported image-reference preservation and text-safe output, while retaining configured preview behavior. The reviewed v0.7 baseline and its historical sign-off at `02fbfc029c7560e2dc24543ee99b6d3fdce2b669` are unchanged. The [TRD delivery record](TRD.md#delivery-evidence) owns exact verification and remaining release limits; no new commit identity, user approval, production/crawler acceptance or publication authorization is asserted.

**Integration finding (2026-09-15, UTC+09:00):** OG-Q-005 records unequal host-supplied context for generated tag pages and the hook-mode workaround. Prior requirement/question IDs and the historical sign-off are preserved; no plugin route-generation responsibility or provider-acceptance claim is introduced.
