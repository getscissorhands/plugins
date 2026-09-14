# ScissorHands Plugins - Product requirements gateway

## Baseline and authority

| Field | Value |
| --- | --- |
| Document version / status | 0.5 / Review-ready |
| Last updated | 2026-09-14 |
| Audience | Plugin authors, maintainers, and consuming site/theme authors |
| Scope | An extensible official plugin catalog; each plugin owns its product baseline |
| Local source baseline | Commit `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`; the accepted policy changes below are pending implementation, not a released baseline |
| Compatibility reference | Resolved ScissorHands.Core/Plugin `1.0.0-preview.20260914.1`; centrally managed major-version floats remain in use |
| Owner | @justinyoo owns implementation, verification, support and release authorization |
| Approval / release | User accepted plugin, support/recovery and deferral policies and confirmed ownership on 2026-09-14; this is not implementation evidence or authorization to publish a specific release |
| Versioning / release stage | Plugins are versioned and released independently from the upstream engine; currently preview. Exact next version/date are selected by @justinyoo when releasing |

This document is the catalog entry point and owns **shared product requirements**. Each linked plugin PRD owns that plugin's behavior, examples, acceptance and questions. [TRD.md](TRD.md) owns shared technical obligations and indexes the plugin TRDs. [AGENTS.md](AGENTS.md) owns contributor commands and workflow.

Read the shared documents plus the relevant plugin pair. A plugin document specializes shared requirements; it cannot silently waive them or govern a sibling plugin. Resolve conflicts in the owning PRD before updating technical requirements. ScissorHands.NET remains authoritative for external engine/extension contracts.

**States:** Confirmed identifies user instructions or source-backed baseline behavior, not automatic future-policy approval. Proposed means awaiting a decision; Unknown means evidence/decisions are absent. Review-ready is not sign-off. New plugin proposals enter the catalog with their actual status, not inherited approval.

## Plugin catalog

| Plugin ID | Purpose | Product requirements | Technical requirements | Usage |
| --- | --- | --- | --- | --- |
| `google-analytics` | Emit Google tag configuration | [Google Analytics PRD](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md) | [Google Analytics TRD](src/ScissorHands.Plugin.GoogleAnalytics/TRD.md) | [README](src/ScissorHands.Plugin.GoogleAnalytics/README.md) |
| `open-graph` | Emit Open Graph/Twitter-card metadata | [Open Graph PRD](src/ScissorHands.Plugin.OpenGraph/PRD.md) | [Open Graph TRD](src/ScissorHands.Plugin.OpenGraph/TRD.md) | [README](src/ScissorHands.Plugin.OpenGraph/README.md) |

This lists current plugins, not a closed set. New entries need their own scope and documents; neither analytics defaults nor Open Graph metadata rules are universal plugin requirements.

## 1. Purpose, users and outcomes

Maintain reusable extensions that let site authors add capabilities without changing the engine. Authors choose installed plugins by ID; theme authors use the documented integration paths; maintainers evolve each package without undocumented behavior changes. Consumers receive generated output whose meaning and external dependencies are defined by the owning plugin.

The [catalog](README.md), local source and the user's authoring/upgrade requests establish the need for maintainable integration. They do not establish measured adoption or time savings. Candidate outcomes are a consumer integration completed without engine edits and an upgrade without undocumented configuration/output changes. Numerical baselines, targets and evaluation windows are unknown; no telemetry or benchmark program is introduced. @justinyoo owns any later evaluation decision.

In scope: plugin authoring, compatibility, shared packaging/testing conventions, separately documented plugin behavior, and a local integration sample consuming the upstream engine. Out of scope: implementing the engine, registry, scheduler, theme framework, navigation, content loader, preview server, deployment service or extension sandbox. The host owns those responsibilities. A disabled plugin is not an unloaded or isolated assembly.

Additional plugin capabilities are not excluded forever: each requires its own product baseline. The accepted policies are recorded in each owning pair and do not authorize unrelated plugin or publishing changes. Detailed design documents are optional when a concrete design warrants one; no speculative templates or roadmap commitments are created.

## 2. Shared product requirements

The retained records are **Confirmed baseline**; P-FR-006 records the user's catalog-growth/documentation decision and P-FR-007 records the local preview sample request. Acceptance describes obligations, not complete evidence.

| ID | Requirement / rationale | Shared acceptance and limits |
| --- | --- | --- |
| P-FR-001 | Stable identity lets site/theme authors select plugins independently of labels | Preserve each catalogued ID; implementations/configuration/selectors use exact lowercase ASCII kebab-case IDs. Display names do not control matching. Components, when supplied, omit disabled output. Upstream owns identity/dependency validation; no universal `Options.Enabled` switch is introduced |
| P-FR-004 | Authors control documented integration without incidental output changes | Each plugin identifies the hooks/components it supports and its insertion/update behavior. Existing Google Analytics/Open Graph marker, no-marker and refresh guarantees are retained in their PRDs. New plugins are not required to provide both a component and a marker hook; they must define their own observable contract |
| P-FR-006 | Every plugin has an independently reviewable product/technical baseline | Create `PRD.md` and `TRD.md` beside the plugin project, link both from these gateways, trace local requirements to verification, and record applicable shared constraints and unresolved questions. Adding a catalog entry does not approve its feature/release |
| P-FR-007 | Plugin authors can preview local changes without publishing packages | The user-requested [sample](sample/README.md) references local plugins and a NuGet.org engine, supports preview/build modes, and uses components by default or paired markers with `--use-placeholders`, without editing a `Sample` JSON block. It reuses packaged default-theme styling/scripts and the color toggle, without replacing the plugin insertion layout. At the user's request, default configuration enables Open Graph and Google Analytics with fake measurement ID `G-EXAMPLE`; this does not prevent browser requests to Google. Generated output is ignored and the sample is not packable. Local output/HTTP checks are not provider or production-host acceptance |
| P-NFR-001 | Keep upgrades compatible and package configuration consistent | Preserve centrally managed major-version floats and the chosen .NET/upstream boundary. Verify resolved dependencies and affected surfaces; document deliberate breaking changes. A floating major is not evidence that every preview in it is compatible |
| P-NFR-002 | Preserve caller-owned configuration and observable failures | Do not mutate option snapshots or nested caller objects; propagate observed cancellation and errors. Keep current defaults and accepted changes distinct: the stricter local policies agreed on 2026-09-14 require explicit migration and regressions, not silent tightening. No bounded interruption or host rollback guarantee is introduced |
| P-NFR-003 | Protect output/data boundaries without unsupported safety claims | Review relevant text, HTML, JavaScript and URL contexts with synthetic inputs; prevent secret disclosure and content-derived command execution. Raw output and formatting helpers are not universal sanitizers. Each plugin records its own validation/privacy evidence gaps |
| P-NFR-005 | Preserve resolved site context and assess applicable client concerns | Reuse supplied route/locale context and respect `Site.BaseUrl` for site-local references. Each plugin documents URL, localization, accessibility, performance and client applicability. Existing head-only plugins do not imply every future plugin has no UI/network/storage concerns |

### Delegated requirements and ID continuity

Plugin-specific records retain their original v0.1 IDs at their new authoritative locations. These rows are navigation, not duplicate requirements.

| Retained ID | Authoritative owner |
| --- | --- |
| P-FR-002 | [Google Analytics configuration](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#requirements) |
| P-FR-003 | [Open Graph metadata](src/ScissorHands.Plugin.OpenGraph/PRD.md#requirements) |
| P-FR-005 | [Open Graph publication URLs](src/ScissorHands.Plugin.OpenGraph/PRD.md#requirements) |
| P-NFR-004 | [Google Analytics privacy/preview boundary](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#requirements); its former Open Graph preview portion is retained as [OG-NFR-001](src/ScissorHands.Plugin.OpenGraph/PRD.md#requirements) |

## 3. Release expectations and question routing

**Accepted release gates, not publication authorization:** identify resolved dependencies; pass applicable hook/component, cancellation and output-boundary regressions; build Release; inspect package metadata/README/license/icon/symbols; obtain scoped consumer evidence. Implement the agreed plugin behavior rather than defer it; an implementation gap in a claimed requirement blocks that release claim. Preview status does not waive these obligations, and old passing tests do not establish acceptance of newly agreed behavior.

Plugin versions and release timing are independent from the upstream engine. Current releases remain preview; a new engine release does not automatically require a plugin release or matching version number. Compatibility claims still cover verified plugin/upstream combinations, not every version allowed by a floating development range. Keep the resolved `1.0.0-preview.20260914.1` baseline as historical evidence and record the actual graph again for each release.

@justinyoo owns implementation, verification, support and release authorization and selects each next plugin version/date. Ownership is not blanket authorization for an agent to tag or publish.

The [current workflow](.github/workflows/main.yaml), updated at the user's 2026-09-14 request, publishes tagged packages to NuGet.org and GitHub Packages before creating a GitHub release. NuGet.org uses OIDC trusted publishing through `nuget-release`; [external setup](README.md#publishing-packages) remains required. Workflow configuration is not evidence of a completed publication or release authorization.

### Support, recovery and deferrals

**Confirmed on 2026-09-14:** the user accepted the recommended support/recovery and nonblocking-deferral policies.

- Support is best-effort through [GitHub Issues](https://github.com/getscissorhands/plugins/issues), triaged by @justinyoo, without a guaranteed response time or a commitment to maintain every historical preview.
- Defective code is fixed in a new preview version, not by replacing a published package. Consumers may temporarily pin a previously verified plugin/engine combination; no automatic rollback is promised.
- Partial multi-registry publication is recovered using the same verified artifacts for missing destinations, skipping already-published duplicates. Do not rebuild different bytes under the same published version.
- A faulty version may be deprecated or unlisted where supported only with @justinyoo's explicit approval for that action, pointing users to its replacement. This does not remove installed copies.
- Only optional extras may be deferred, with an issue recording the item, rationale for remaining nonblocking and @justinyoo as owner. Agreed behavior and required release evidence are not optional extras. No specific item is deferred by this decision; do not invent a deferral list.

The [release guide](README.md#support-and-recovery) describes these operational responses. Accepting this policy does not authorize a particular publication, deprecation or unlisting.

### Shared release question

**Q-005 (policy settled; release execution remains):** @justinyoo owns delivery, support and release authorization. Independent preview versioning, both publishing destinations, release gates and the support/recovery/optional-only deferral policies above are settled. Configure/verify the environment, `NUGET_USER` and trusted-publishing policy, complete required implementation/evidence, and obtain authorization for the chosen release. Registry writes remain non-atomic and previously published versions are not replaced. Exact next version/date and external setup are execution decisions, not claimed complete here. See T-007/T-008 in [TRD](TRD.md).

### Delegated questions

Q-001 through Q-004 remain routing IDs. Local records now distinguish settled policy from pending implementation/evidence: paired-marker support and preview policy are settled; stricter validation, metadata parity and missing-context behavior require implementation. Cancellation/removal tests are engineering follow-ups, not product choices to reopen.

| Previous ID / subject | Google Analytics owner | Open Graph owner |
| --- | --- | --- |
| Q-001: marker examples | [GA-Q-001](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#plugin-questions-and-acceptance-limits) | [OG-Q-001](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits) |
| Q-002: output validation | [GA-Q-002](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#plugin-questions-and-acceptance-limits) | [OG-Q-002](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits) |
| Q-003: parity/cancellation evidence | [GA-Q-003](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#plugin-questions-and-acceptance-limits) | [OG-Q-003](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits) |
| Q-004: option/preview/consent policy | [GA-Q-004](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md#plugin-questions-and-acceptance-limits) | [OG-Q-004](src/ScissorHands.Plugin.OpenGraph/PRD.md#plugin-questions-and-acceptance-limits); analytics-specific policy does not apply |

## Sources and review status

The current source, tests, [shared build props](src/Directory.Build.props), [central versions](Directory.Packages.props) and workflow establish the local baseline. Upstream source was consulted at `7b5db6e1f27327cd8be50c08e4163e72e0a28425` on 2026-09-14:

- [PRD][upstream-prd]: plugin enablement and trusted-extension boundary only.
- [TRD][upstream-trd]: plugin identity, immutable input, compatibility, cancellation and output/URL obligations.
- [TDD][upstream-tdd]: extension execution/raw-rendering context, not a local engine design.
- [AGENTS][upstream-agents]: applicable authoring, testing and package guardrails.

**v0.2 change:** on the user's 2026-09-14 request, convert the root documents into extensible gateways, retain shared IDs, relocate specific requirements/questions with explicit mappings, and introduce P-FR-006. Existing runtime scope, upstream provenance and unresolved decisions are preserved.

**Sample addition (2026-09-14):** the user requested a local preview directory based on theme-template. P-FR-007 adds a consuming host and fixtures, not a new engine/theme product or a change to either plugin's preview/consent policy.

**v0.3 decisions (2026-09-14):** the user accepted the review recommendations: paired markers only, explicit validation/failure rules, Open Graph parity/optional-image/URL policy, preserved configured preview behavior and external consent responsibility, and verified release gates. The plugin PRDs define the chosen technical acceptance details; current source behavior is separately retained. Documentation agreement does not claim runtime delivery.

**v0.4 clarification (2026-09-14):** @justinyoo confirmed ownership and that plugin versioning/releases are independent of the upstream engine and currently preview. Recovery/support and nonblocking-deferral recommendations were left open for explanation at that revision. This did not change pipeline version overrides, mandate separate release trains between the two local plugins, or authorize a release.

**v0.5 decisions (2026-09-14):** the user accepted best-effort issue support, fixes in new previews, same-artifact recovery for partial publication, approval-gated deprecation/unlisting, and optional-only issue-tracked deferrals. Q-005's policy choices are settled; no specific work is deferred or registry action authorized.

**Readiness:** Review-ready with accepted policies and a named owner. Specific validation/parity work and external release setup/evidence remain pending, not deferred. Each plugin has its own delivery assessment; no upstream approval, engine verification-program ID, completed audit or release authorization is inherited.

[upstream-prd]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/PRD.md
[upstream-trd]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/TRD.md
[upstream-tdd]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/TDD.md
[upstream-agents]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/AGENTS.md
