# ScissorHands Plugins - Product requirements gateway

## Baseline and authority

| Field | Value |
| --- | --- |
| Document version / status | 0.2 / Review-ready |
| Last updated | 2026-09-14 |
| Audience | Plugin authors, maintainers, and consuming site/theme authors |
| Scope | An extensible official plugin catalog; each plugin owns its product baseline |
| Local source baseline | Inspected working tree based on `b61a0539b017e02c9eaf7a31ad6791e4692938c6`, including pending package, URL-helper and build-props changes; not a released commit |
| Compatibility reference | Resolved ScissorHands.Core/Plugin `1.0.0-preview.20260914.1`; centrally managed major-version floats remain in use |
| Approval / release | User requested shared gateways and a PRD/TRD pair per plugin; feature sign-off, release timing and owners remain unestablished |

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

The [catalog](README.md), local source and the user's authoring/upgrade requests establish the need for maintainable integration. They do not establish measured adoption or time savings. Candidate outcomes are a consumer integration completed without engine edits and an upgrade without undocumented configuration/output changes. Numerical baselines, targets, owners and evaluation windows are unknown; no telemetry or benchmark program is introduced.

In scope: plugin authoring, compatibility, shared packaging/testing conventions, and separately documented plugin behavior. Out of scope: implementing the engine, registry, scheduler, theme framework, navigation, content loader, preview server, deployment service or extension sandbox. The host owns those responsibilities. A disabled plugin is not an unloaded or isolated assembly.

Additional plugin capabilities are not excluded forever: each requires its own product baseline. This documentation split authorizes no new plugin, feature or publishing change. Detailed design documents are optional when a concrete design warrants one; no speculative templates or roadmap commitments are created.

## 2. Shared product requirements

The retained records are **Confirmed baseline**; P-FR-006 records the user's catalog-growth/documentation decision. Acceptance describes obligations, not complete evidence.

| ID | Requirement / rationale | Shared acceptance and limits |
| --- | --- | --- |
| P-FR-001 | Stable identity lets site/theme authors select plugins independently of labels | Preserve each catalogued ID; implementations/configuration/selectors use exact lowercase ASCII kebab-case IDs. Display names do not control matching. Components, when supplied, omit disabled output. Upstream owns identity/dependency validation; no universal `Options.Enabled` switch is introduced |
| P-FR-004 | Authors control documented integration without incidental output changes | Each plugin identifies the hooks/components it supports and its insertion/update behavior. Existing Google Analytics/Open Graph marker, no-marker and refresh guarantees are retained in their PRDs. New plugins are not required to provide both a component and a marker hook; they must define their own observable contract |
| P-FR-006 | Every plugin has an independently reviewable product/technical baseline | Create `PRD.md` and `TRD.md` beside the plugin project, link both from these gateways, trace local requirements to verification, and record applicable shared constraints and unresolved questions. Adding a catalog entry does not approve its feature/release |
| P-NFR-001 | Keep upgrades compatible and package configuration consistent | Preserve centrally managed major-version floats and the chosen .NET/upstream boundary. Verify resolved dependencies and affected surfaces; document deliberate breaking changes. A floating major is not evidence that every preview in it is compatible |
| P-NFR-002 | Preserve caller-owned configuration and observable failures | Do not mutate option snapshots or nested caller objects; propagate observed cancellation and errors. Plugin-specific permissive defaults must remain documented, not silently tightened. No bounded interruption or host rollback guarantee is introduced |
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

**Proposed review gates, not authorization:** identify resolved dependencies; verify shared and affected plugin requirements; build Release; inspect package metadata/README/license/icon; obtain scoped consumer evidence and disclose risks. Publish/support/rollback policies, release owners and dates are not established.

The [current workflow](.github/workflows/main.yaml), updated at the user's 2026-09-14 request, publishes tagged packages to NuGet.org and GitHub Packages before creating a GitHub release. NuGet.org uses OIDC trusted publishing through `nuget-release`; [external setup](README.md#publishing-packages) remains required. Workflow configuration is not evidence of a completed publication or release authorization.

### Shared release question

**Q-005 (partly resolved):** publication targets are now NuGet.org plus GitHub Packages, and version overrides no longer edit project files. An appropriate prerelease version may still be necessary for the resolved dependency graph. Configure and verify the GitHub environment, `NUGET_USER` and NuGet trusted-publishing policy before publication; verify pack/consumer behavior and disclose that registry writes are not atomic. Release/support/rollback decisions and their owner remain unknown. See shared T-007/T-008 in [TRD](TRD.md).

### Delegated questions

The old Q-001 through Q-004 are retained as routing IDs; actionable details live with each plugin. No question is closed by this split.

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

**Readiness:** Review-ready for the catalog/document structure and stated shared baseline; not feature or release approval. Each plugin has its own readiness/gap assessment. No upstream approval, engine verification-program ID or issue-specific acceptance is inherited.

[upstream-prd]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/PRD.md
[upstream-trd]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/TRD.md
[upstream-tdd]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/TDD.md
[upstream-agents]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/AGENTS.md
