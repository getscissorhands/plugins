# Google Analytics - Technical requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin PRD](PRD.md) | [Usage](README.md)

## Baseline and inheritance

| Field | Value |
| --- | --- |
| Version / status | 0.6 / Implementation-ready |
| Last updated / PRD consulted | 2026-09-14 |
| Product baseline | Google Analytics PRD v0.6, Implementation-ready with unchanged product behavior and acceptance |
| Shared baseline | Catalog PRD/TRD v0.7; apply shared obligations without silently overriding them |
| Source baseline | Commit `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`; target validation changes remain pending |
| Package / plugin ID | `ScissorHands.Plugin.GoogleAnalytics` / `google-analytics` |
| Approval / owner | @justinyoo owns implementation, verification, support and release authorization; validation/output and regression requirements confirmed on 2026-09-14, not runtime acceptance or approval to publish |
| Release stage | Preview, with versioning/releases independent from the upstream engine |

This TRD owns Google Analytics's technical behavior and evidence expectations. Shared T-001 through T-004 and T-006 through T-009 apply. Open Graph's delegated T-005 does not: this plugin emits an external Google URL, not content/social-image URLs. Applicable shared P-NFR-005 still prevents prefixing that external URL with the site's base path.

The plugin is an independent Razor class library consuming upstream Plugin/Core. It overrides only `PostHtmlAsync`, has no `DependsOn` override, and performs no filesystem or outbound measurement operation during generation. Removing its manifest disables host hooks/component output; it does not unload or sandbox the assembly. It is not an analytics client service, registry or consent backend. Source links and verification below own the implementation details formerly repeated in the PRD.

## GA-TR-001: Measurement configuration

**State / source:** Confirmed requirement (user, 2026-09-14); P-FR-002, shared P-NFR-002/P-NFR-003 and T-004/T-006. [Hook](GoogleAnalyticsPlugin.cs) and [component code](GoogleAnalyticsComponent.razor.cs) still implement the earlier permissive behavior.

**Configuration example, relocated from PRD v0.5:**

```json
{
  "Plugins": [
    { "Id": "google-analytics", "Options": { "MeasurementId": "G-EXAMPLE" } }
  ]
}
```

Read nullable options with typed access, without mutating the upstream snapshot or nested caller values. An enabled plugin must require a string `MeasurementId` matching the whole value `G-` plus one or more uppercase ASCII letters/digits (`\AG-[A-Z0-9]+\z`). Do not trim malformed input into acceptance. `G-EXAMPLE` remains an explicitly supported synthetic value, not proof of an active Google property. Missing/null/wrong-type, blank, whitespace-padded and invalid-character values must fail with a contextual configuration exception naming `google-analytics` and `MeasurementId`, without echoing the supplied payload.

Both hook and component must apply the same validation and output semantics; disabling still means removing the manifest. Preserve ordinary valid loader/config output and handle the HTML-attribute/URL and JavaScript-string contexts separately. Do not use a blanket sanitizer or assume HTML encoding is JavaScript encoding. No provider request is made to validate an ID.

**Current gap (GA-Q-002/GA-Q-004):** arbitrary strings and empty-ID fallbacks remain in code; stricter validation and output evidence are not delivered by this document.

**Verification required:** update [hook](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests/GoogleAnalyticsPluginTests.cs) and [component](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests/GoogleAnalyticsComponentTests.cs) cases to assert the new failure matrix, valid IDs, `G-EXAMPLE`, immutable options and non-leaking diagnostics. Cover quotes, script terminators, whitespace and markup-like values. Earlier tests expecting empty IDs are evidence of old behavior, not acceptance of the new policy.

## GA-TR-002: Hook and component integration

**State / source:** Confirmed baseline; GA-FR-001 and shared P-FR-001/P-FR-004; T-001/T-002/T-003. Keep selection and insertion predictable.

Retain `Id="google-analytics"` and non-empty implementation display name. The hook must check cancellation before work and replace every exact paired marker `<plugin:google-analytics></plugin:google-analytics>` case-insensitively. Paired markers are the only documented hook syntax; no self-closing support is added. Valid unmarked HTML is unchanged; multiple markers insert multiple scripts. It does not validate host enablement or deduplicate existing scripts.

The alternative component syntax is `<GoogleAnalyticsComponent Id="google-analytics" />` within a layout supplying the upstream cascade. Use one path per intended insertion unless duplicate output is intended. The hook marker and self-closing Razor component syntax are distinct contracts.

The component calls `base.OnParametersSet()`, clears `MeasurementId`, and reads the newly resolved manifest. Its [Razor markup](GoogleAnalyticsComponent.razor) emits nothing when `Plugin` is null. Do not supply `Plugin` directly or use `Name` as a selector. Upstream owns the cascade and identity validation.

**Verification:** Existing hook tests cover paired/absent/multiple markers and empty HTML; bUnit covers absent manifests, display-name independence and selection changes. GA-Q-001 is resolved by correcting README examples to paired markers. The cancellation test's `ShouldThrowAsync` assertion must be awaited and removal transitions covered under GA-Q-003. These are pending engineering follow-ups, not unsettled product choices.

## GA-TR-003: Preview and privacy

**State / source:** Accepted policy retaining current behavior; P-NFR-004 and shared T-006. Distinguish generated markup from remote behavior.

Neither path checks `Site.IsPreview`; enabled preview and production output include the script. Generation does not send analytics events. A browser loading the output can request Google's script and run its measurement behavior.

Retain enabled-when-configured behavior in preview and production, including the fake-ID sample. No suppression option is added in this scope. Consent integration belongs to the consuming site; provider retention/deletion remains external. The plugin must not claim consent enforcement, preview isolation, zero network, provider delivery or compliance. Future changes to that settled boundary require a new product decision.

**Verification:** Inspect both paths and test local markup with synthetic values when behavior changes. Real credentials or Google requests are not required for unit tests. Provider/consent acceptance is outside existing evidence.

## Traceability and verification

| Product baseline | Technical coverage | Evidence / limits |
| --- | --- | --- |
| P-FR-002 | GA-TR-001 | Strict option/output policy accepted; implementation/regression changes pending |
| GA-FR-001 | GA-TR-002 | Paired-marker examples aligned; removal/cancellation evidence pending |
| P-NFR-004 | GA-TR-003 | Source-backed preview/network boundary, not provider acceptance |
| Shared P-FR-001/P-FR-004 | GA-TR-002, T-001/T-002/T-003 | Exact identity, supported insertion and disabled component behavior |
| Shared P-FR-006 | T-009 and this PRD/TRD pair | Gateway links, stable IDs, status and evidence mapping |
| Shared P-NFR-001 | T-007/T-008 | Resolved graph, Release build, tests and package inspection |
| Shared P-NFR-002 | T-002/T-004, GA-TR-001/002 | Option ownership/failure behavior; await cancellation assertions |
| Shared P-NFR-003 | T-006, GA-TR-001/003 | Context-specific output/privacy review; not a comprehensive audit |
| Shared P-NFR-005 | GA-TR-001/003 | External loader URL; no local route/locale/UI generation |

Shared commands and package conventions remain in [AGENTS.md](../../AGENTS.md). Verify this package's assembly, README, license, icon, symbols and dependency metadata. Shared [Q-005](../../PRD.md#shared-release-question) and [T-008](../../TRD.md#t-008-package-and-consumer-documentation) govern accepted release/support/recovery and optional-only deferral policies. Required validation/encoding and regression work is pending, not deferred. Packaging and policy agreement are not publication authorization.

## Gaps and readiness

The [PRD decision records](PRD.md#plugin-questions-and-acceptance-limits) retain product choices and routing IDs. This TRD owns their technical delivery state: GA-Q-001's paired-marker examples are aligned; GA-Q-002/GA-Q-004's validation/encoding changes remain pending; GA-Q-003's awaited cancellation assertions and removal-transition evidence remain pending under GA-TR-002. Preview/consent policy is retained. @justinyoo owns the follow-ups; agreement is not evidence of completion.

**Migration / current behavior, relocated from PRD v0.5:** current hooks/components accept arbitrary strings and emit an empty ID for missing or non-string values. Strict validation is a breaking change for callers relying on that behavior. Configure a `MeasurementId` accepted by GA-TR-001, or remove the plugin manifest, before adopting the future implementation. This documentation change does not deliver that implementation.

No visible controls, local storage, authentication, remote-generation API or performance SLA is in scope. Privacy/client implications are explicitly applicable because browser code contacts Google. Other capabilities require their own product change, not inherited sibling requirements.

**v0.5 confirmation (2026-09-14):** the user explicitly confirmed validation, output handling and regression coverage. GA-TR-001's syntax/error/output requirements and GA-TR-002's awaited cancellation/removal evidence are confirmed without changing scope, migration effects or IDs.

**v0.6 separation (2026-09-14):** consolidates package/source metadata, JSON/component examples, validation contracts and technical delivery/migration records here. GA-Q-003 is technically owned by GA-TR-002, with its PRD entry retained as a redirect. Product acceptance and all existing IDs remain unchanged.

**Readiness:** Implementation-ready against local PRD v0.6 and shared v0.7 baselines. No unresolved policy or technical requirement blocks implementation. Runtime validation/encoding and related regressions remain pending, not deferred. Shared T-008 records reported publishing setup and the limits of independent verification; release evidence and authorization are still required. This is not completed implementation, a completed audit or release approval.
