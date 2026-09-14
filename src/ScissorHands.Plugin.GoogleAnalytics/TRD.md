# Google Analytics - Technical requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin PRD](PRD.md) | [Usage](README.md)

## Baseline and inheritance

| Field | Value |
| --- | --- |
| Version / status | 0.7 / Implementation-ready |
| Last updated / PRD consulted | 2026-09-15 |
| Product baseline | Google Analytics PRD v0.7; signed-off requirements with an implementation follow-up, not a new product decision |
| Shared baseline | Catalog PRD/TRD v0.8; apply shared obligations without silently overriding them |
| Source baseline | Historical implementation `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`; implementation follow-up 2026-09-15 based on `9107f3e` |
| Delivery state | Strict configuration/output handling and lifecycle/cancellation regressions implemented; targeted and shared solution/sample/package checks passed. [Evidence](#targeted-evidence-2026-09-15) retains external verification and release-authorization limits |
| Package / plugin ID | `ScissorHands.Plugin.GoogleAnalytics` / `google-analytics` |
| Owner | @justinyoo owns implementation, verification, support and release authorization |
| Sign-off | @justinyoo signed off v0.7 at commit `02fbfc029c7560e2dc24543ee99b6d3fdce2b669` on 2026-09-15 (UTC+09:00). Approval covers requirements and acceptance criteria, not completed implementation or publication authorization |
| Release stage | Preview, with versioning/releases independent from the upstream engine |

This TRD owns Google Analytics's technical behavior and evidence expectations. Shared T-001 through T-004 and T-006 through T-009 apply. Open Graph's delegated T-005 does not: this plugin emits an external Google URL, not content/social-image URLs. Applicable shared P-NFR-005 still prevents prefixing that external URL with the site's base path.

The local PRD, this TRD and catalog requirements define the accepted obligations. Engine identity, hook and component API references are centralized in the [catalog TRD](../../TRD.md#1-shared-boundaries); verify them against the resolved package release when changing integration. Engine planning/contributor documents are not prerequisites or sources of additional policy.

The plugin is an independent Razor class library consuming upstream Plugin/Core. It overrides only `PostHtmlAsync`, has no `DependsOn` override, and performs no filesystem or outbound measurement operation during generation. Removing its manifest disables host hooks/component output; it does not unload or sandbox the assembly. It is not an analytics client service, registry or consent backend. Source links and verification below own the implementation details formerly repeated in the PRD.

## GA-TR-001: Measurement configuration

**State / source:** Confirmed requirement (user, 2026-09-14), implemented in the 2026-09-15 follow-up; P-FR-002, shared P-NFR-002/P-NFR-003 and T-004/T-006. [Hook](GoogleAnalyticsPlugin.cs) and [component code](GoogleAnalyticsComponent.razor.cs) share [GoogleAnalyticsConfiguration](GoogleAnalyticsConfiguration.cs).

**Configuration example, relocated from PRD v0.5:**

```json
{
  "Plugins": [
    { "Id": "google-analytics", "Options": { "MeasurementId": "G-EXAMPLE" } }
  ]
}
```

The shared reader accepts nullable `IReadOnlyDictionary<string, object?>` and uses `TryGetValue` and a string type check, without mutating the upstream snapshot or nested caller values. An enabled plugin requires a string `MeasurementId` matching the whole value `G-` plus one or more uppercase ASCII letters/digits (`\AG-[A-Z0-9]+\z`), enforced by a culture-invariant generated regex. It does not trim malformed input into acceptance. `G-EXAMPLE` remains an explicitly supported synthetic value, not proof of an active Google property.

Null options, missing/null/non-string values, empty/blank, whitespace-padded and malformed IDs throw `InvalidOperationException`. This is an invalid configured operation, not a new public exception/API type. Both paths produce the same fixed diagnostic naming `google-analytics` and `MeasurementId` and describing the allowed format. It never formats or includes the payload, attaches it as an inner exception, or silently emits an empty ID. Hook validation applies even to empty/unmarked HTML; component validation applies only when a selected manifest exists.

Disabling still means removing the manifest; no `Options.Enabled` switch is introduced. Valid loader/config output is preserved with separate contexts: `Uri.EscapeDataString` constructs the ID query value; the raw hook applies `HtmlEncoder.Default` to the loader URL, while Razor encodes its URL attribute. `JavaScriptEncoder.Default` encodes single-quoted JavaScript string content on both paths. Razor inserts that already-JavaScript-encoded value as `MarkupString`, avoiding an incorrect HTML-encoding pass. The strict alphabet itself excludes HTML/JavaScript delimiters. No blanket sanitizer or provider request is involved.

**Verification (GA-Q-002/GA-Q-004):** [hook](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests/GoogleAnalyticsPluginTests.cs) and [component](../../test/ScissorHands.Plugin.GoogleAnalytics.Tests/GoogleAnalyticsComponentTests.cs) run the same 38-scenario failure matrix, including nested/non-string values whose `ToString` must not be called, quotes, script terminators, markup, delimiters, Unicode and trailing newlines. Fixed-message assertions establish non-leaking diagnostics. Parsed script/attribute assertions cover five synthetic valid IDs including `G-EXAMPLE`, `G-A` and `G-0`. Snapshot cases cover successful and failing reads plus upstream defensive-copy behavior; they explicitly retain nested object/list identities and values. Obsolete empty-ID expectations are replaced, not preserved.

## GA-TR-002: Hook and component integration

**State / source:** Confirmed baseline; GA-FR-001 and shared P-FR-001/P-FR-004; T-001/T-002/T-003. Keep selection and insertion predictable.

Retain `Id="google-analytics"` and non-empty implementation display name. The hook must check cancellation before work and replace every exact paired marker `<plugin:google-analytics></plugin:google-analytics>` case-insensitively. Paired markers are the only documented hook syntax; no self-closing support is added. Valid unmarked HTML is unchanged; multiple markers insert multiple scripts. It does not validate host enablement or deduplicate existing scripts.

The alternative component syntax is `<GoogleAnalyticsComponent Id="google-analytics" />` within a layout supplying the upstream cascade. Use one path per intended insertion unless duplicate output is intended. The hook marker and self-closing Razor component syntax are distinct contracts.

The component calls `base.OnParametersSet()` before accessing `Plugin`, clears `MeasurementId` and derived URL/JavaScript state, then reads the newly resolved manifest. Its [Razor markup](GoogleAnalyticsComponent.razor) emits nothing when `Plugin` is null, without validating unrelated manifest options. Do not supply `Plugin` directly or use `Name` as a selector. Upstream owns the cascade and validates the selector and all supplied manifest IDs, rejecting duplicate IDs before selection; malformed IDs fail there rather than being normalized. Optional/repeated manifest display names do not affect exact selection. Absent-selection behavior does not bypass upstream identity validation.

**Verification (GA-Q-001/GA-Q-003):** passing tests cover case-insensitive repeated paired markers, existing scripts without deduplication, empty/unmarked HTML and unsupported self-closing/nonempty markers. Cancellation assertions are awaited and check the original token; cancelled calls with null arguments/invalid options demonstrate entry cancellation before input access. bUnit covers missing cascades, absent/null/empty manifest collections, exact-ID/display-name independence, configuration/site/document updates, reselection, removal/re-addition, newly invalid options, and clearing the protected measurement state before absence or validation failure. No mid-operation cancellation or host rollback guarantee is added.

## GA-TR-003: Preview and privacy

**State / source:** Accepted policy retaining current behavior; P-NFR-004 and shared T-006. Distinguish generated markup from remote behavior.

Neither path checks `Site.IsPreview`; enabled preview and production output include the script. Generation does not send analytics events. A browser loading the output can request Google's script and run its measurement behavior.

Retain enabled-when-configured behavior in preview and production, including the fake-ID sample. No suppression option is added in this scope. Consent integration belongs to the consuming site; provider retention/deletion remains external. The plugin must not claim consent enforcement, preview isolation, zero network, provider delivery or compliance. Future changes to that settled boundary require a new product decision.

**Verification:** both paths pass synthetic preview/production cases at root and `/blog` with a non-default locale; the loader remains Google's external URL, not a site-prefixed URL. bUnit and local HTML parsing do not fetch resources or execute browser JavaScript. No real Google request, credential or browser was used. Provider/consent acceptance is outside this evidence.

## Traceability and verification

| Product baseline | Technical coverage | Evidence / limits |
| --- | --- | --- |
| P-FR-002 | GA-TR-001 | Shared strict reader, failure matrix, intact output and immutable-input regressions passed |
| GA-FR-001 | GA-TR-002 | Paired-marker, lifecycle/removal/re-addition and awaited entry-cancellation regressions passed |
| P-NFR-004 | GA-TR-003 | Preview/production markup regressions passed; not provider acceptance |
| Shared P-FR-001/P-FR-004 | GA-TR-002, T-001/T-002/T-003 | Exact identity, supported insertion and disabled component behavior |
| Shared P-FR-006 | T-009 and this PRD/TRD pair | Gateway links, stable IDs, status and evidence mapping |
| Shared P-NFR-001 | T-007/T-008 | Resolved graph and targeted evidence below; full-solution/sample/package checks passed in the [shared evidence](../../TRD.md#implementation-evidence-2026-09-15) |
| Shared P-NFR-002 | T-002/T-004, GA-TR-001/002 | Option ownership/failure behavior and awaited cancellation assertions passed |
| Shared P-NFR-003 | T-006, GA-TR-001/003 | Context-specific output/privacy review; not a comprehensive audit |
| Shared P-NFR-005 | GA-TR-001/003 | External loader URL; no local route/locale/UI generation |

Shared commands and package conventions remain in [AGENTS.md](../../AGENTS.md). Shared [Q-005](../../PRD.md#shared-release-question) and [T-008](../../TRD.md#t-008-package-and-consumer-documentation) govern accepted release/support/recovery and optional-only deferral policies. Packaging and policy agreement are not publication authorization.

### Targeted evidence (2026-09-15)

Windows .NET SDK `10.0.401`, `net10.0`, Release; existing restored dependencies were used without upgrade or reinstall. Resolved Core/Plugin: `1.0.0-preview.20260914.1`; bUnit `2.11.3`, xUnit v3 `4.0.1`, MTP `2.4.0`, Shouldly `4.3.0`, NSubstitute `6.2.0`. This is evidence for this graph, not every version in the floating ranges.

The installed ScissorHands.Core and ScissorHands.Plugin `1.0.0-preview.20260914.1` packages' `.nuspec` repository metadata identifies **exactly** upstream commit `7b5db6e1f27327cd8be50c08e4163e72e0a28425`, matching the pinned API documentation URLs; this is a package-to-source association, not merely a historical documentation snapshot. The [catalog's release-matched source verification](../../TRD.md#1-shared-boundaries) owns the shared evidence. At that commit, [PluginManifest](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Core/Manifests/PluginManifest.cs) shallow-copies options into an ordinal dictionary wrapped in `ReadOnlyDictionary`; nested values are not deep-copied. [PluginComponentBase](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Plugin/PluginComponentBase.cs) provides parameter-time exact-ID selection and validation. The regressions above exercise those released contracts.

Commands run from the repository root (Windows path separators normalized to forward slashes for documentation):

```powershell
dotnet build ./test/ScissorHands.Plugin.GoogleAnalytics.Tests/ScissorHands.Plugin.GoogleAnalytics.Tests.csproj -c Release --no-restore -warnaserror
dotnet test --project ./test/ScissorHands.Plugin.GoogleAnalytics.Tests/ScissorHands.Plugin.GoogleAnalytics.Tests.csproj -c Release --no-build --verbosity normal
```

Final result: build succeeded with **0 warnings, 0 errors**; MTP discovered **135 tests, 135 passed, 0 failed, 0 skipped**.

**Shared validation completed:** the [catalog implementation evidence](../../TRD.md#implementation-evidence-2026-09-15) records a full Release build with 0 warnings/errors and 347 passing tests (GA 135, Open Graph 186, sample 26), eight isolated static builds (64 pages), both preview modes, and local package inspection at `1.0.0-preview.implementation`. GA output passed; sample checks used loopback HTML/local theme assets without JavaScript execution. Assembly, project README, license, icon, dependency and symbol contents passed inspection. The catalog owns exact shared commands, contexts and integration limits.

## Gaps and readiness

The [PRD decision records](PRD.md#plugin-questions-and-acceptance-limits) retain product choices and routing IDs. GA-Q-001 through GA-Q-004's scoped implementation/regression follow-ups are delivered as described above. Preview/consent policy is retained. No accepted local runtime behavior remains deferred.

**Breaking migration:** the earlier hooks/components accepted arbitrary strings and emitted an empty ID for missing or non-string values. Both now fail for those configurations. Configure a `MeasurementId` accepted by GA-TR-001, or remove the plugin manifest, before upgrading. Public entry points are unchanged; the shared helper is internal. The sample's synthetic `G-EXAMPLE` and paired markers remain compatible without a sample change.

**Remaining evidence/authorization:** full-solution, scoped sample-host and package-content checks are completed above, not pending. External publication, real provider/consent behavior and a comprehensive output/privacy audit remain unverified. Evidence is limited to the recorded environment and does not establish browser JavaScript behavior or broader cross-platform compatibility. Existing publishing-setup observations do not prove OIDC/registry delivery. No release is authorized by these checks; @justinyoo owns release selection and authorization.

No visible controls, local storage, authentication, remote-generation API or performance SLA is in scope. Privacy/client implications are explicitly applicable because browser code contacts Google. Other capabilities require their own product change, not inherited sibling requirements.

**v0.5 confirmation (2026-09-14):** the user explicitly confirmed validation, output handling and regression coverage. GA-TR-001's syntax/error/output requirements and GA-TR-002's awaited cancellation/removal evidence are confirmed without changing scope, migration effects or IDs.

**v0.6 separation (2026-09-14):** consolidates package/source metadata, JSON/component examples, validation contracts and technical delivery/migration records here. GA-Q-003 is technically owned by GA-TR-002, with its PRD entry retained as a redirect. Product acceptance and all existing IDs remain unchanged.

**v0.7 reference policy (2026-09-14):** makes local requirement authority and release-matched API references explicit. All validation, lifecycle, privacy, migration and verification obligations remain unchanged.

**Readiness:** scoped implementation and targeted regression validation completed against local PRD v0.7 and shared v0.8. The 2026-09-15 sign-off at `02fbfc029c7560e2dc24543ee99b6d3fdce2b669` remains historical requirements approval, not approval of this runtime. Shared T-008's remaining release evidence and authorization still apply; local success is not a completed audit or release approval.
