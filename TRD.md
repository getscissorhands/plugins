# ScissorHands Plugins - Technical requirements gateway

## Baseline and authority

| Field | Value |
| --- | --- |
| Version / status | 0.5 / Review-ready |
| Last updated / PRD consulted | 2026-09-14 |
| Product baseline | [Catalog PRD](PRD.md) v0.5, Review-ready with accepted plugin/release policies and ownership; shared requirements and delegated plugin baselines |
| Scope | Shared authoring/compatibility obligations and an index of per-plugin technical requirements |
| Sources | Catalog source baseline `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`, current configuration and plugin-relevant upstream contracts |
| Sign-off / owner | @justinyoo owns implementation, verification, support and release authorization; plugin/support/recovery/deferral policies confirmed on 2026-09-14, not implementation or release acceptance |

Read this gateway plus the owning plugin's PRD/TRD. Shared requirements apply where the plugin uses that surface; local documents must explicitly state applicability or justified exclusions, not silently weaken shared rules. Product changes belong in the owning PRD before its TRD. Upstream Plugin/Core contracts remain authoritative; engine implementation and approval history are excluded.

## Plugin technical catalog

| Plugin ID | Product baseline | Technical requirements | Evidence location |
| --- | --- | --- | --- |
| `google-analytics` | [PRD v0.4](src/ScissorHands.Plugin.GoogleAnalytics/PRD.md) | [Google Analytics TRD v0.4](src/ScissorHands.Plugin.GoogleAnalytics/TRD.md) | [Tests](test/ScissorHands.Plugin.GoogleAnalytics.Tests) |
| `open-graph` | [PRD v0.4](src/ScissorHands.Plugin.OpenGraph/PRD.md) | [Open Graph TRD v0.4](src/ScissorHands.Plugin.OpenGraph/TRD.md) | [Tests](test/ScissorHands.Plugin.OpenGraph.Tests) |

## 1. Shared boundaries

Plugins consume ScissorHands.Plugin and transitive Core contracts; the host owns installation, ID/dependency validation, hook scheduling, resolved routes, generation and serving. Components, where provided, receive context from a theme's cascade. Do not reproduce Web-engine services or assume arbitrary assemblies are sandboxed.

Do not infer catalog-wide implementation rules from the first two plugins. Supported hooks, components, options, dependencies, side effects, network/storage access and output types belong in each plugin pair. New plugins need explicit requirements for new risks; head-only current plugins do not establish a universal no-UI/no-network policy.

Configuration sources are [root props](Directory.Build.props), [source props](src/Directory.Build.props), [test props](test/Directory.Build.props), [central packages](Directory.Packages.props) and [global.json](global.json). These select .NET 10, nullable/implicit usings, the language version, central major floats, and MTP/xUnit v3 test executables. Warnings-as-errors is currently a validation flag, not a root property.

## 2. Shared technical requirements

Shared T-records retain the existing authoring baseline, with accepted release-policy changes in T-007/T-008. Local v0.4 TRDs distinguish accepted target behavior from current code and pending implementation. Each record states its source, obligation and verification; stable IDs and redirects remain intact.

### T-001: Identity and dependency integration

**Source / rationale:** P-FR-001, P-NFR-001; [upstream plugin contract][upstream-plugin]. Preserve stable selection independent of display labels.

Implementations must use unique lowercase ASCII kebab-case IDs and non-empty display names; manifests/selectors/dependency targets use exact IDs with no normalization or name fallback. Manifest presence enables host hooks; components must omit output for an absent manifest. There is no universal options-based enablement switch.

Declare actual stage prerequisites through `DependsOn`/`PluginDependency(PluginId, Stage)`, never registration/manifest order. Upstream validates declarations and schedules stages; local plugins must not implement a parallel registry/resolver.

**Verification:** Check IDs, display-name independence and disabled surfaces locally; exercise host validation/order when dependencies are introduced. Each plugin TRD records its actual declarations and coverage.

### T-002: Hook transformation and cancellation

**Source / rationale:** P-FR-004, P-NFR-002; upstream hook signatures. Keep transformations composable.

Override only supported stages, inherit unused pass-through hooks and return the transformed value. Preserve non-null hook parameter contracts and pass supplied cancellation through supported operations. Observed cancellation and errors must propagate rather than become success-shaped output. A direct hook call does not enforce host enablement.

Each plugin defines its own insertion/replacement contract; paired markers and replace-all semantics are not required for every future plugin. Preserve the two existing plugins' guarantees in their local TRDs. No mid-string-operation interruptibility, global deduplication or host rollback guarantee is introduced.

**Verification:** Test the owning plugin's unchanged/transformed/failure cases and await asynchronous cancellation assertions. Coverage gaps stay in its PRD, not hidden behind a passing suite.

### T-003: Component lifecycle and cascading input

**Source / rationale:** P-FR-001/P-FR-004, P-NFR-002; `PluginComponentBase`. Applies to plugins that provide a Razor component.

Call `base.OnParametersSet()` before using the selected manifest; recompute/clear derived state when context changes. Use `Id` for selection and upstream cascading parameters for site/document/manifests; `Plugin` is not a direct component parameter. Define missing-context behavior locally without promising universal surface parity.

**Verification:** bUnit selection, absence, updates and relevant context transitions. A hook-only plugin records this requirement as not applicable with a reason.

### T-004: Option and metadata behavior

**Source / rationale:** P-NFR-002; plugin-owned product requirements. Preserve immutable configuration ownership and explicit defaults.

Treat nullable `PluginManifest.Options` as read-only input; use typed access rather than writable-dictionary casts or mutation of nested caller objects. Defensive copying is not deep immutability. A plugin must define its keys, types, defaults, metadata precedence and invalid-value/error behavior in its own pair.

The former combined Google Analytics/Open Graph option table now belongs to [GA-TR-001](src/ScissorHands.Plugin.GoogleAnalytics/TRD.md#ga-tr-001-measurement-configuration) and [OG-TR-001](src/ScissorHands.Plugin.OpenGraph/TRD.md#og-tr-001-option-and-metadata-behavior). Neither plugin's permissive defaults govern a new plugin.

**Verification:** Local option snapshots, null/missing/wrong-type cases and documented precedence. Stricter validation has now been explicitly accepted for the existing plugins in their PRDs; it is not a universal catalog default, nor is it already implemented.

### T-005: Content and image URL boundaries

**Delegated record:** the authoritative Open Graph composition requirement retains ID T-005 in its [plugin TRD](src/ScissorHands.Plugin.OpenGraph/TRD.md#t-005-content-and-image-url-boundaries). This root heading remains a navigation anchor, not a duplicate requirement.

The applicable catalog-wide rule is P-NFR-005: use upstream [ContentUrlHelper semantics][upstream-urls] for relevant content/image references, honor site subpaths, and document each plugin's actual URL contexts. Do not impose Open Graph's absolute-social-URL composition on all future plugins or escape images as content slugs.

### T-006: Output integrity, privacy and failure policy

**Source / rationale:** P-NFR-003/P-NFR-005; upstream output/trust boundary. Assess the contexts each plugin emits.

Preserve ordinary Razor metadata encoding and review raw HTML, attributes, JavaScript strings and URL schemes separately. Formatting helpers are not sanitizers or scheme allowlists. Use synthetic inputs; do not expose secrets or treat content as commands. Do not introduce blanket sanitization as a compatibility shortcut.

Each plugin must document preview behavior and relevant external requests, storage, consent/privacy, UI/accessibility and locale concerns. No catalog-wide preview suppression, analytics policy, offline guarantee, retention service or compliance certification is implied. Local PRDs own changes to those policies.

**Verification:** Inspect and test the affected contexts and side effects; distinguish untested or proposed validation from proven behavior. The existing plugins' output/preview gaps are owned by their local questions.

### T-007: Shared build and compatibility configuration

**Source / rationale:** P-NFR-001; current props and user-selected floats. Avoid per-project drift.

Keep versions in `Directory.Packages.props` with central-floating opt-in. Source/test props import root defaults and own common dependencies/settings; projects retain unique metadata/references. Preserve .NET 10, executable xUnit v3 tests, Shouldly/NSubstitute/bUnit and Microsoft.Testing.Platform. Await async assertions and use the xUnit cancellation token where appropriate.

**Verification / accepted compatibility policy:** re-evaluate floating packages during upgrades, record the resolved plugin/upstream version combinations and validate affected behavior. Compatibility claims cover that verified graph, not every version in the floating ranges or an indefinite backward-support window. Build Release and run applicable regressions with nonzero discovery; breaking changes require migration guidance and new acceptance tests. Commands live in [AGENTS.md](AGENTS.md).

Plugin versions and release timing must not be coupled to upstream engine version numbers or releases. @justinyoo selects independent preview releases; compatibility is established by dependency declarations and evidence, not matching version labels. No pipeline version value or package dependency range is changed by this documentation clarification.

### T-008: Package and consumer documentation

**Source / rationale:** P-NFR-001, Q-005 and the catalog's accepted release/support/recovery/deferral policies; source props/workflow. Deliver independently usable packages with explicit recovery boundaries.

Keep plugin-specific IDs/descriptions/tags and repository metadata. Normal builds do not pack; explicit packing includes the assembly, project README with root fallback, license, icon and symbols. Version appropriately for prerelease dependencies rather than suppressing NU5104 or silently changing release policy.

The tag-only release workflow publishes to NuGet.org using `NuGet/login@v1`, the `nuget-release` environment and release-scoped `id-token: write`; `NUGET_USER` selects the NuGet account. The temporary key authenticates package/symbol pushes. GitHub Packages publishing is retained with `GITHUB_TOKEN` and `--no-symbols`, followed by GitHub release creation. Both registries use `--skip-duplicate`; partial publication is not an atomic transaction.

Build/pack receive `-p:Version` without editing project files. [Environment, secret and trusted-publisher setup](README.md#publishing-packages) must match this repository and workflow before a release. Local validation does not exercise OIDC token exchange or grant publication permission.

**Accepted release gate:** @justinyoo owns implementation, verification, support and release authorization. Before publishing, record their explicit authorization for that release, the actual dependency graph, applicable build/regression results, package-content checks and scoped consumer-integration evidence. Implement agreed plugin behavior rather than defer it; preview status does not waive requirements or evidence. Only optional extras may be deferred, with an issue identifying the item, nonblocking rationale and @justinyoo as owner. No specific deferral is approved by this revision.

**Accepted support/recovery contract:** use best-effort GitHub Issue triage, without a response-time SLA or maintenance commitment for every historical preview. Code fixes must use a new preview version, not replacement of a published package. A partial release must reuse the same verified artifacts for missing destinations and skip existing duplicates; do not rebuild different bytes under the same version. Deprecation or unlisting requires @justinyoo's explicit approval for the affected release and must point to its replacement where supported. Neither action removes installed copies; no automatic rollback is provided. Consumers may pin a previously verified plugin/engine combination. Operational guidance lives in [README](README.md#support-and-recovery).

**Verification:** inspect actual package contents/dependencies, verify paired-marker examples, and exercise changed option/context/URL rules in both integration paths. For an actual recovery, record artifact identity, per-destination results and any required deprecation/unlisting approval before claiming completion. Review any optional-deferral issue against the accepted gates. Existing tests for obsolete defaults must not stand in for new target acceptance. PRD/TRD files are repository authoring documents, not automatically bundled into NuGet packages; a written recovery policy is not evidence of a performed recovery.

### T-009: Per-plugin documentation and onboarding

**Source / rationale:** P-FR-006; user's 2026-09-14 catalog-growth request. Keep ownership scalable and traceable.

Each plugin project must have adjacent `PRD.md` and `TRD.md`. The PRD defines scope, behavior, acceptance and questions; its TRD cites that PRD's version/status plus these shared baselines and maps requirements to evidence. Both link back to the gateways and each other. Register the pair in both root catalogs.

Use new plugin-prefixed IDs; preserve existing IDs or provide explicit relocation mappings. Record applicable shared requirements, exceptions needing decisions, and justified exclusions. A documentation entry cannot approve a sibling plugin or inherit upstream approval. Create a TDD only when design depth warrants one.

**Verification:** Check gateway/back links, local traceability, source fidelity, statuses and absence of duplicated authoritative behavior. Only explicitly agreed scope can change shared requirements.

### T-010: Local preview integration

**Source / rationale:** P-FR-007; user's local sample request and [theme-template reference](https://github.com/getscissorhands/theme-template/tree/main/sample). Exercise local plugins without package publication.

The non-packable `sample` web project must consume centrally versioned ScissorHands.Web and reference local plugin projects. It reuses six built-in views with a small layout that forwards upstream cascading context and selects either components or paired markers, never both. Only configured plugins produce markers/output. Both Open Graph and Google Analytics are enabled in the sample; analytics uses fake measurement ID `G-EXAMPLE` at the user's request. The fake ID is for markup inspection and does not prevent browser requests to Google. Removing the analytics manifest disables its output; no plugin-level preview suppression or consent handling is introduced.

Copy the Web package's linked theme content into build/publish output without version-specific paths. Include the default manifest and third-party notice, not just CSS/JS: the engine resolves the bundled theme below the application output and copies its assets into generated output. The sample layout renders manifest URLs through `GetThemeUrl` and supplies the theme's header/navigation/footer classes and `theme-toggle` control. The packaged script persists the color preference in browser localStorage. No separate CSS/JS implementation or full theme/navigation fork is introduced.

Run from the sample directory so upstream working-directory roots resolve correctly. Provide local content, a hero-image fixture, a single `http` launch profile at `http://localhost:5000` and documented preview/build commands. `Site.SiteUrl` uses the same URL for static metadata; no-profile runs use the host's default address unless overridden. The sample bootstrap consumes `--use-placeholders` and prepends `--Sample:UsePlaceholders=true` to the forwarded host arguments so the switch works before or after the engine's bare mode flag. Other arguments retain their order; no `Sample` JSON block or separate hook profile is needed. Ignore and exclude generated `preview`/`dist` inputs. The sample consumes upstream serving and does not implement missing subpath-mount behavior or claim provider acceptance.

**Verification:** Build the sample with the solution; exercise switch translation before/after preview/build flags, absent/repeated switches and preservation of other arguments. Exercise its layout with the real plugin runner in both modes, with and without analytics/manifests. Inspect generated root/post/page/tag/404 metadata, copied default-theme files, stylesheet/script/favicon HTTP responses, and browser styling/color-toggle behavior. Generation-only analytics checks use synthetic values without fetching provider resources.

## 3. Product-to-technical routing

| Catalog PRD ID | Technical coverage / owner |
| --- | --- |
| P-FR-001 | T-001, T-003 plus each plugin's integration record |
| P-FR-002 | [Google Analytics TRD](src/ScissorHands.Plugin.GoogleAnalytics/TRD.md#traceability-and-verification) |
| P-FR-003 | [Open Graph TRD](src/ScissorHands.Plugin.OpenGraph/TRD.md#traceability-and-verification) |
| P-FR-004 | T-002, T-003 plus each plugin's insertion contract |
| P-FR-005 | [Open Graph T-005](src/ScissorHands.Plugin.OpenGraph/TRD.md#t-005-content-and-image-url-boundaries) |
| P-FR-006 | T-009 |
| P-FR-007 | T-010 |
| P-NFR-001 | T-001, T-007, T-008 |
| P-NFR-002 | T-002, T-003, T-004 |
| P-NFR-003 | T-006 |
| P-NFR-004 | [Google Analytics privacy](src/ScissorHands.Plugin.GoogleAnalytics/TRD.md#ga-tr-003-preview-and-privacy); former Open Graph portion in [OG-TR-003](src/ScissorHands.Plugin.OpenGraph/TRD.md#og-tr-003-output-and-preview-boundaries) |
| P-NFR-005 | T-006 plus each plugin's URL/locale/client applicability; Open Graph specializes this in T-005 |

Plugin TRDs own local test/evidence mappings and the decision/delivery state of Q-001 through Q-004. Shared Q-005 maps to T-007/T-008. Evidence existence, successful generation, user acceptance of policy and document status are not release sign-off.

## 4. Coverage and change record

Identity, contracts, input ownership, failure behavior, build, compatibility and packaging apply catalog-wide. Rendering/URLs, UI, accessibility, localization, performance, storage, networking and privacy require per-plugin applicability review. Engine I/O containment, navigation, serving and deployment are external; no new database, payment, account or AI system is introduced by this catalog structure.

**v0.2:** retains T-001 through T-008; generalizes shared obligations, relocates option details to plugin-prefixed records and T-005 to Open Graph, and adds T-009. Retained headings/mappings preserve earlier references. No runtime change or question resolution is implied.

**Sample addition (2026-09-14):** T-010 implements the separately requested P-FR-007 consumer sample. It does not change plugin runtime contracts or convert local checks into release/provider approval.

**v0.3 decisions (2026-09-14):** align with the accepted recommendations and plugin PRDs v0.2. Release evidence policy is accepted; local TRDs specify the stricter validation/parity/omission targets and preserved preview boundaries. Runtime changes remain pending, while paired-marker documentation is corrected. No existing requirement ID is dropped.

**v0.4 clarification (2026-09-14):** incorporated @justinyoo's ownership and engine-independent preview release policy without authorizing a release or amending version ranges. Recovery/support and deferral details remained open at that revision.

**v0.5 decisions (2026-09-14):** aligns T-008 with the user's accepted support/recovery and optional-only deferral policies in catalog PRD v0.5. Q-005's policy choices are settled; implementation, external setup, evidence and per-action authorization remain execution responsibilities. No specific item is deferred or recovery claimed complete.

**Readiness:** Review-ready with accepted policies, named ownership and technical elaboration, not full-document sign-off or completed implementation. Local runtime/evidence work and external release setup remain outstanding, not deferred. The [catalog source record](PRD.md#sources-and-review-status) retains provenance without importing engine approval or asserting release readiness.

[upstream-plugin]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/docs/website-documentation.md#plugin-authoring
[upstream-urls]: https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/docs/website-documentation.md#shared-url-helpers
