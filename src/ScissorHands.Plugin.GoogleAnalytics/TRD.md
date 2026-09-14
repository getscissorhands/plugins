# Google Analytics - Technical requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin PRD](PRD.md) | [Usage](README.md)

## Baseline and inheritance

| Field | Value |
| --- | --- |
| Version / status | 0.1 / Review-ready |
| Last updated / PRD consulted | 2026-09-14 |
| Product baseline | Google Analytics PRD v0.1, Review-ready |
| Shared baseline | Catalog PRD/TRD v0.2; apply shared obligations without silently overriding them |
| Source baseline | Catalog's inspected working tree and upstream reference; not a release |
| Approval / owners | No requirement sign-off; implementation, verification and release owners unassigned |

This TRD owns Google Analytics's technical behavior and evidence expectations. Shared T-001 through T-004 and T-006 through T-009 apply. Open Graph's delegated T-005 does not: this plugin emits an external Google URL, not content/social-image URLs. Applicable shared P-NFR-005 still prevents prefixing that external URL with the site's base path.

The plugin is an independent Razor class library consuming upstream Plugin/Core. It overrides only `PostHtmlAsync`, has no `DependsOn` override, and performs no filesystem or outbound measurement operation during generation. It is not an analytics client service, registry, consent backend or sandbox.

## GA-TR-001: Measurement configuration

**State / source:** Confirmed baseline; P-FR-002, shared P-NFR-002/P-NFR-003 and T-004/T-006; [hook](GoogleAnalyticsPlugin.cs) and [component code](GoogleAnalyticsComponent.razor.cs). Preserve existing options and output.

Read nullable options with `TryGetValue("MeasurementId", ...)` and a string type check. Use the string in the Google tag loader and `gtag('config', ...)`; null options, missing/null/wrong-type values yield an empty ID. Do not mutate the upstream snapshot or nested caller values. Format and whitespace checks are not implemented.

The hook uses raw string replacement; the Razor surface inserts a value into a URL attribute and JavaScript string. Neither mechanism establishes safe arbitrary measurement-ID input. Accepted values/context-specific escaping or error behavior remain GA-Q-002/GA-Q-004, not new rules selected by this TRD.

**Verification:** [Hook tests](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests/GoogleAnalyticsPluginTests.cs) cover IDs, null/missing/non-string options, valid output and defensive-copy behavior. [Component tests](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests/GoogleAnalyticsComponentTests.cs) cover selected options/null options. Add synthetic output-boundary cases before stronger validation claims.

## GA-TR-002: Hook and component integration

**State / source:** Confirmed baseline; GA-FR-001 and shared P-FR-001/P-FR-004; T-001/T-002/T-003. Keep selection and insertion predictable.

Retain `Id="google-analytics"` and non-empty implementation display name. The hook must check cancellation before work and replace every exact paired marker `<plugin:google-analytics></plugin:google-analytics>` case-insensitively. Valid unmarked HTML is unchanged; multiple markers insert multiple scripts. It does not validate host enablement or deduplicate existing scripts.

The component calls `base.OnParametersSet()`, clears `MeasurementId`, and reads the newly resolved manifest. Its Razor file emits nothing when `Plugin` is null. Do not supply `Plugin` directly or use `Name` as a selector. Upstream owns the cascade and identity validation.

**Verification:** Existing hook tests cover absent/multiple markers and empty HTML; bUnit covers absent manifests, display-name independence and selection changes. The cancellation test's `ShouldThrowAsync` assertion is not awaited, so strengthen that case before claiming full cancellation evidence. Removal/cancellation cases and self-closing-marker host behavior remain GA-Q-003 and GA-Q-001 respectively; invalid measurement strings belong to GA-Q-002.

## GA-TR-003: Preview and privacy

**State / source:** Confirmed baseline; P-NFR-004 and shared T-006; both production paths. Distinguish generated markup from remote behavior.

Neither path checks `Site.IsPreview`; enabled preview and production output include the script. Generation does not send analytics events. A browser loading the output can request Google's script and run its measurement behavior.

The plugin must not claim consent enforcement, preview isolation, zero network, provider delivery, compliance or control of Google-side retention/deletion. Changes to empty-ID handling, suppression or consent integration must be resolved in the local PRD (GA-Q-004) first.

**Verification:** Inspect both paths and test local markup with synthetic values when behavior changes. Real credentials or Google requests are not required for unit tests. Provider/consent acceptance is outside existing evidence.

## Traceability and verification

| Product baseline | Technical coverage | Evidence / limits |
| --- | --- | --- |
| P-FR-002 | GA-TR-001 | Hook/component option assertions; malformed-string validation remains open |
| GA-FR-001 | GA-TR-002 | Marker and bUnit selection tests; removal/cancellation/self-closing evidence gaps remain |
| P-NFR-004 | GA-TR-003 | Source-backed preview/network boundary, not provider acceptance |
| Shared P-FR-001/P-FR-004 | GA-TR-002, T-001/T-002/T-003 | Exact identity, supported insertion and disabled component behavior |
| Shared P-FR-006 | T-009 and this PRD/TRD pair | Gateway links, stable IDs, status and evidence mapping |
| Shared P-NFR-001 | T-007/T-008 | Resolved graph, Release build, tests and package inspection |
| Shared P-NFR-002 | T-002/T-004, GA-TR-001/002 | Option ownership/failure behavior; await cancellation assertions |
| Shared P-NFR-003 | T-006, GA-TR-001/003 | Context-specific output/privacy review; not a comprehensive audit |
| Shared P-NFR-005 | GA-TR-001/003 | External loader URL; no local route/locale/UI generation |

Shared commands and package conventions remain in [AGENTS.md](../../AGENTS.md). Verify this package's assembly, README, license, icon, symbols and dependency metadata. Shared Q-005 controls release-version/publishing questions; packaging is not publication authorization.

## Gaps and readiness

[GA-Q-001 through GA-Q-004](PRD.md#plugin-questions-and-acceptance-limits) are authoritative: GA-TR-002 covers marker/cancellation evidence, GA-TR-001 covers malformed measurement IDs and output contexts, and GA-TR-003 covers preview/consent policy. Owners remain unassigned. Documentation does not close any gap or authorize a runtime change.

No visible controls, local storage, authentication, remote-generation API or performance SLA is in scope. Privacy/client implications are explicitly applicable because browser code contacts Google. Other capabilities require their own product change, not inherited sibling requirements.

**Readiness:** Review-ready against the local PRD and shared v0.2 baselines; not sign-off or implementation-ready for proposed policy changes. Plugin details formerly embedded in root T-002/T-003/T-004/T-006 now live here; shared IDs remain in the gateway. Source provenance is in the [catalog](../../PRD.md#sources-and-review-status), and no new executed verification is claimed by this document split.
