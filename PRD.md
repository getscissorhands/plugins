# ScissorHands Plugins - Product requirements gateway

## Baseline and authority

| Field | Value |
| --- | --- |
| Document version / status | 0.7 / Implementation-ready |
| Last updated | 2026-09-14 |
| Audience | Plugin authors, maintainers, and consuming site/theme authors |
| Scope | An extensible official plugin catalog; each plugin owns its product baseline |
| Delivery state | Accepted behavior changes remain pending; the [TRD](TRD.md#baseline-and-authority) records the implementation baseline and technical evidence |
| Compatibility scope | Verified plugin/engine combinations; the [TRD](TRD.md#t-007-shared-build-and-compatibility-configuration) owns dependency and build constraints |
| Owner | @justinyoo owns implementation, verification, support and release authorization |
| Approval / release | User confirmed the engineering requirements, regression coverage, support/recovery and deferral policies on 2026-09-14; this is not implementation evidence or authorization to publish a specific release |
| Versioning / release stage | Plugins are versioned and released independently from the upstream engine; currently preview. Exact next version/date are selected by @justinyoo when releasing |

This document is the catalog entry point and owns **shared product requirements**. Each linked plugin PRD owns its purpose, users, scope, observable behavior and product acceptance. [TRD.md](TRD.md) and the plugin TRDs own contracts, configuration formats, implementation constraints, verification methods and technical delivery gaps. READMEs provide usage and operational guidance; [AGENTS.md](AGENTS.md) owns contributor commands and workflow.

Read the shared documents plus the relevant plugin pair. A plugin document specializes shared requirements; it cannot silently waive them or govern a sibling plugin. Resolve conflicts in the owning PRD before updating technical requirements. ScissorHands.NET remains authoritative for external engine/extension contracts.

**States:** Confirmed identifies user instructions or source-backed baseline behavior, not automatic future-policy approval. Proposed means awaiting a decision; Unknown means evidence/decisions are absent. Review-ready is not sign-off. Implementation-ready means the confirmed requirements support starting work, not that implementation or release gates have passed. New plugin proposals enter the catalog with their actual status, not inherited approval.

## Plugin catalog

| Plugin ID | Purpose | Product requirements | Technical requirements | Usage |
| --- | --- | --- | --- | --- |
| `google-analytics` | Emit Google tag configuration | [Google Analytics PRD](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md) | [Google Analytics TRD](src/ScissorHands.Plugin.GoogleAnalytics/TRD.md) | [README](src/ScissorHands.Plugin.GoogleAnalytics/README.md) |
| `open-graph` | Emit Open Graph/Twitter-card metadata | [Open Graph PRD](src/ScissorHands.Plugin.OpenGraph/PRD.md) | [Open Graph TRD](src/ScissorHands.Plugin.OpenGraph/TRD.md) | [README](src/ScissorHands.Plugin.OpenGraph/README.md) |

This lists current plugins, not a closed set. New entries need their own scope and documents; neither analytics defaults nor Open Graph metadata rules are universal plugin requirements.

## 1. Purpose, users and outcomes

Maintain reusable extensions that let site authors add capabilities without changing the engine. Authors choose installed plugins by ID; theme authors use the documented integration paths; maintainers evolve each package without undocumented behavior changes. Consumers receive generated output whose meaning and external dependencies are defined by the owning plugin.

The [catalog](README.md), local source and the user's authoring/upgrade requests establish the need for maintainable integration. They do not establish measured adoption or time savings. Candidate outcomes are a consumer integration completed without engine edits and an upgrade without undocumented configuration/output changes. Numerical baselines, targets and evaluation windows are unknown; no telemetry or benchmark program is introduced. @justinyoo owns any later evaluation decision.

In scope: plugin authoring, compatibility, shared release-quality expectations, separately documented plugin behavior, and a local integration sample consuming the upstream engine. Out of scope: implementing the engine, registry, scheduler, theme framework, navigation, content loader, preview server, deployment service or extension sandbox. The host owns those responsibilities. Disabling a plugin is not a security-isolation boundary.

Additional plugin capabilities are not excluded forever: each requires its own product baseline. The accepted policies are recorded in each owning pair and do not authorize unrelated plugin or publishing changes. Detailed design documents are optional when a concrete design warrants one; no speculative templates or roadmap commitments are created.

## 2. Shared product requirements

The retained records are **Confirmed product requirements**; P-FR-006 records the user's catalog-growth/documentation decision and P-FR-007 records the local preview sample request. Acceptance describes observable obligations, not complete evidence. Their technical constraints and verification criteria remain binding through the [TRD mapping](TRD.md#3-product-to-technical-routing).

| ID | Requirement / rationale | Shared acceptance and limits |
| --- | --- | --- |
| P-FR-001 | Stable identity lets site/theme authors select plugins independently of labels | Preserve each catalogued identity; changing a display label must not change which plugin is selected. Disabled plugins produce no output through their supported host integrations. Selection and dependency resolution remain the engine's responsibility |
| P-FR-004 | Authors control documented integration without incidental output changes | Each plugin identifies the hooks/components it supports and its insertion/update behavior. Existing Google Analytics/Open Graph marker, no-marker and refresh guarantees are retained in their PRDs. New plugins are not required to provide both a component and a marker hook; they must define their own observable contract |
| P-FR-006 | Every plugin has an independently reviewable product/technical baseline | Each catalog entry links its product and technical requirements, identifies applicable shared constraints and exposes unresolved decisions and acceptance limits. Adding an entry does not approve its feature/release |
| P-FR-007 | Plugin authors can preview local changes without publishing packages | The [sample](sample/README.md) supports local preview and static generation, using components by default and an alternate paired-placeholder mode selectable without editing configuration files. Reuse the built-in theme's styling and color toggle while preserving plugin insertion. Both current plugins are enabled by default, with an explicitly synthetic analytics identifier; this does not prevent browser requests to Google. The sample is for local validation, not a distributable plugin or provider/production-host acceptance |
| P-NFR-001 | Keep upgrades predictable and packages consistent | Consumers can identify verified engine compatibility and understand deliberate breaking changes and migration. Package conventions remain consistent across the catalog. Do not claim compatibility with unverified engine versions |
| P-NFR-002 | Preserve caller-owned configuration and observable failures | Plugin execution must not alter author-supplied configuration, including nested values, or turn observed errors/cancellation into successful output. Stricter accepted behavior requires explicit migration and evidence, not silent tightening. No bounded interruption or host rollback guarantee is introduced |
| P-NFR-003 | Protect content and configuration without unsupported safety claims | Prevent unintended interpretation of supplied data as executable instructions, secret disclosure and unintended output injection. Preserve the meaning of legitimate content rather than imposing blanket sanitization. Each plugin states its privacy and assurance limits; do not claim a completed audit or broader protection than the evidence supports |
| P-NFR-005 | Preserve the site's publication context and assess client concerns | Honor the configured site subpath, routes and locale rather than inventing a different publication structure. Each plugin addresses applicable localization, accessibility, performance and client expectations. The current head-only plugins do not establish that future plugins have no UI, network or storage concerns |

### Delegated requirements and ID continuity

Plugin-specific records retain their original v0.1 IDs at their new authoritative locations. These rows are navigation, not duplicate requirements.

| Retained ID | Authoritative owner |
| --- | --- |
| P-FR-002 | [Google Analytics configuration](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#requirements) |
| P-FR-003 | [Open Graph metadata](src/ScissorHands.Plugin.OpenGraph/PRD.md#requirements) |
| P-FR-005 | [Open Graph publication URLs](src/ScissorHands.Plugin.OpenGraph/PRD.md#requirements) |
| P-NFR-004 | [Google Analytics privacy/preview boundary](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#requirements); its former Open Graph preview portion is retained as [OG-NFR-001](src/ScissorHands.Plugin.OpenGraph/PRD.md#requirements) |

## 3. Release expectations and question routing

**Accepted release gates, not publication authorization:** demonstrate the claimed behavior and compatibility, complete package and consumer documentation, and obtain scoped consumer-integration evidence. Implement agreed behavior rather than defer it; an implementation gap blocks that release claim. Preview status does not waive these obligations, and evidence for earlier behavior does not establish acceptance of newly agreed behavior. [T-007/T-008](TRD.md#t-007-shared-build-and-compatibility-configuration) define the technical release checks.

Plugin versions and release timing are independent from the upstream engine. Current releases remain preview; a new engine release does not automatically require a plugin release or matching version number. Compatibility claims cover verified plugin/engine combinations, not an indefinite promise to support every engine version.

@justinyoo owns implementation, verification, support and release authorization and selects each next plugin version/date. Ownership is not blanket authorization for an agent to tag or publish.

Distribute approved releases through NuGet.org and GitHub Packages, followed by a GitHub release. @justinyoo reports that publishing-account setup is complete; independent publishing verification is still a release gate. [T-008](TRD.md#t-008-package-and-consumer-documentation) owns the publishing mechanism, setup observations and evidence limits. Setup is not publication authorization.

### Support, recovery and deferrals

**Confirmed on 2026-09-14:** the user accepted the recommended support/recovery and nonblocking-deferral policies.

- Support is best-effort through [GitHub Issues](https://github.com/getscissorhands/plugins/issues), triaged by @justinyoo, without a guaranteed response time or a commitment to maintain every historical preview.
- Defective code is fixed in a new preview version, not by replacing a published package. Consumers may temporarily pin a previously verified plugin/engine combination; no automatic rollback is promised.
- Recover incomplete publication without changing versions already delivered to consumers. [T-008](TRD.md#t-008-package-and-consumer-documentation) defines the artifact-integrity and retry requirements.
- A faulty version may be deprecated or unlisted where supported only with @justinyoo's explicit approval for that action, pointing users to its replacement. This does not remove installed copies.
- Only optional extras may be deferred, with an issue recording the item, rationale for remaining nonblocking and @justinyoo as owner. Agreed behavior and required release evidence are not optional extras. No specific item is deferred by this decision; do not invent a deferral list.

The [release guide](README.md#support-and-recovery) describes these operational responses. Accepting this policy does not authorize a particular publication, deprecation or unlisting.

### Shared release question

**Q-005 (policy settled; release execution remains):** ownership, independent preview versioning, publishing destinations, release gates and support/recovery/optional-only deferral policies are settled. Exact next version/date, delivery evidence and authorization remain @justinyoo's release responsibilities. Technical readiness and publishing verification are tracked in [T-007/T-008](TRD.md#t-007-shared-build-and-compatibility-configuration), not reopened as product decisions here.

### Delegated questions

Q-001 through Q-004 remain routing IDs. On 2026-09-14, the user explicitly confirmed Google Analytics validation, Open Graph consistency, required/optional metadata behavior, URL/output handling and regression coverage. Local records distinguish these confirmed requirements from pending implementation/evidence. Paired-marker and preview policies remain unchanged. No unresolved policy decision blocks starting the agreed engineering work.

| Previous ID / subject | Google Analytics owner | Open Graph owner |
| --- | --- | --- |
| Q-001: marker examples | [GA-Q-001](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#plugin-questions-and-acceptance-limits) | [OG-Q-001](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits) |
| Q-002: output validation | [GA-Q-002](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#plugin-questions-and-acceptance-limits) | [OG-Q-002](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits) |
| Q-003: consistency and verification | [GA-Q-003 technical record](src/ScissorHands.Plugin.GoogleAnalytics/TRD.md#ga-tr-002-hook-and-component-integration) | [OG-Q-003 product decision](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits); [technical record](src/ScissorHands.Plugin.OpenGraph/TRD.md#og-tr-002-hook-and-component-integration) |
| Q-004: option/preview/consent policy | [GA-Q-004](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#plugin-questions-and-acceptance-limits) | [OG-Q-004](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits); analytics-specific policy does not apply |

## Sources and review status

Product decisions come from the user's authoring, upgrade, sample and requirements-review requests recorded below. The [upstream PRD][upstream-prd], consulted on 2026-09-14, supplies the plugin-enablement and trusted-extension product boundary only. The [catalog TRD](TRD.md#1-shared-boundaries) owns pinned technical sources, dependency versions, implementation evidence and engineering constraints; no engine plan or approval is inherited.

**v0.2 change:** on the user's 2026-09-14 request, convert the root documents into extensible gateways, retain shared IDs, relocate specific requirements/questions with explicit mappings, and introduce P-FR-006. Existing runtime scope, upstream provenance and unresolved decisions are preserved.

**Sample addition (2026-09-14):** the user requested a local preview directory based on theme-template. P-FR-007 adds a consuming host and fixtures, not a new engine/theme product or a change to either plugin's preview/consent policy.

**v0.3 decisions (2026-09-14):** the user accepted paired markers only, explicit invalid-configuration failures, consistent Open Graph metadata with optional images, preserved configured preview behavior and external consent responsibility, and verified release gates. The detailed contracts are retained in the plugin TRDs. Documentation agreement does not claim runtime delivery.

**v0.4 clarification (2026-09-14):** @justinyoo confirmed ownership and that plugin versioning/releases are independent of the upstream engine and currently preview. Recovery/support and nonblocking-deferral recommendations were left open for explanation at that revision. This did not change pipeline version overrides, mandate separate release trains between the two local plugins, or authorize a release.

**v0.5 decisions (2026-09-14):** the user accepted best-effort issue support, fixes in new previews, same-artifact recovery for partial publication, approval-gated deprecation/unlisting, and optional-only issue-tracked deferrals. Q-005's policy choices are settled; no specific work is deferred or registry action authorized.

**v0.6 confirmation (2026-09-14):** the user confirmed the remaining requirements and reported completing publishing setup. This closed requirement confirmation, not implementation or publishing verification; technical observations are retained in T-008.

**v0.7 separation (2026-09-14):** at the user's request, move technical contracts, configuration, verification details and operational evidence into the TRDs. Product behavior, release policy and requirement/question IDs are unchanged. The [relocation record](TRD.md#prd-to-trd-relocation) identifies their technical owners.

**Readiness:** Implementation-ready for the agreed product scope: observable requirements, acceptance conditions, ownership and release policies are confirmed. Delivery and release evidence remain pending, not deferred; the TRDs retain the specific engineering gaps. Quantitative outcome evaluation remains outside the committed scope and under @justinyoo's ownership. No completed audit, upstream approval or release authorization is inherited.

[upstream-prd]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/PRD.md
