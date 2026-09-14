# Open Graph - Technical requirements

[Catalog PRD](../../PRD.md) | [Catalog TRD](../../TRD.md) | [Plugin PRD](PRD.md) | [Usage](README.md)

## Baseline and inheritance

| Field | Value |
| --- | --- |
| Version / status | 0.7 / Implementation-ready |
| Last updated / PRD consulted | 2026-09-15 |
| Product baseline | Open Graph PRD v0.7, Implementation-ready with unchanged product behavior and acceptance |
| Shared baseline | Catalog PRD/TRD v0.8; apply shared obligations without silently overriding them |
| Historical source baseline | Commit `283eb0fa228ce005b63c05ca6e726e5c08b4bd13`; retained as pre-delivery evidence, not the delivered implementation |
| Delivery state | Implemented in the 2026-09-15 delivery follow-up; targeted local evidence below, not release approval |
| Package / plugin ID | `ScissorHands.Plugin.OpenGraph` / `open-graph` |
| Owner | @justinyoo owns implementation, verification, support and release authorization |
| Sign-off | @justinyoo signed off v0.7 at commit `02fbfc029c7560e2dc24543ee99b6d3fdce2b669` on 2026-09-15 (UTC+09:00). Approval covers requirements and acceptance criteria, not completed implementation or publication authorization |
| Release stage | Preview, with versioning/releases independent from the upstream engine |

This TRD owns Open Graph's technical behavior and evidence expectations. Shared T-001 through T-004 and T-006 through T-009 apply; original T-005 is relocated here as the authoritative social-URL requirement, with a gateway redirect. New local records use `OG-TR-*`.

The local PRD, this TRD and catalog requirements define the accepted obligations. Engine identity, hook and component API references are centralized in the [catalog TRD](../../TRD.md#1-shared-boundaries). That release-matching policy also applies to the Core URL-helper reference below. Engine planning/contributor documents are not prerequisites or sources of additional policy.

The plugin is an independent Razor class library consuming Plugin/Core. It overrides only `PostHtmlAsync`, has no `DependsOn` override, and performs no file/network operation in generation. It does not implement route loading, navigation, preview hosting or image fetching. Source links and verification below own the implementation details formerly repeated in the PRD.

**Release-matched API record (2026-09-15):** restored project assets resolve ScissorHands.Core and ScissorHands.Plugin `1.0.0-preview.20260914.1`. Both cached NuGet manifests identify repository commit `7b5db6e1f27327cd8be50c08e4163e72e0a28425`, matching the existing pinned reference, not the moving `vnext` branch. Relevant source references are [Core ContentUrlHelper](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Core/Urls/ContentUrlHelper.cs), [PluginComponentBase](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Plugin/PluginComponentBase.cs), [ContentPlugin](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Plugin/ContentPlugin.cs) and [SiteManifest](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Core/Manifests/SiteManifest.cs). Existing API calls are retained; the implementation adds local policy and shared metadata resolution, not a replacement engine API. Cached helper probes and the regression suite exercise this resolved release. No dependency upgrade or broader floating-range compatibility claim is made.

**Source verification for that release:** the fetched `PluginComponentBase` source confirms that `OnParametersSet` calls its base, validates the component `Id`, clears `Plugin`, validates configured IDs and duplicates with exact ordinal identity, then selects the matching manifest. The [PluginManifest source at the same commit](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Core/Manifests/PluginManifest.cs) confirms a shallow input copy into an ordinal `Dictionary` wrapped in `ReadOnlyDictionary`. This establishes the package-to-source association for the lifecycle/identity and option-snapshot contracts used here; it does not claim deep immutability of nested values. No upstream API change is required.

The verified `ContentUrlHelper` source at that same release commit confirms that `GetContentUrl` trims outer whitespace, splits on `/` and `\` with empty segments removed, rejects literal `.`/`..` segments, applies `Uri.EscapeDataString` per segment, and returns `.` for the root. `GetImageUrl` only checks for null and returns `path.TrimStart('/')`: it does not trim trailing slashes/whitespace, normalize backslashes, escape image segments or validate schemes. The local image classification/validation therefore precedes this narrow helper call; neither method is treated as a sanitizer.

Preview context belongs to the cascaded `SiteManifest.IsPreview` Boolean; the released `PluginComponentBase` has no `IsPreview` property. Preview regressions set the site flag directly. Publication-root composition was explicitly clarified for this delivery: reject query/fragment-bearing `SiteUrl` values and query/fragment/absolute/network `BaseUrl` forms, retain a supported `SiteUrl` path plus `BaseUrl`, and preserve supported image queries/fragments. These local publication-context rules are not global URL sanitization or an altered historical sign-off.

## OG-TR-001: Option and metadata behavior

**State / source:** Confirmed requirement (user, 2026-09-14); P-FR-003 and shared T-004. Delivered through the [shared metadata resolver](OpenGraphMetadata.cs), [hook](OpenGraphPlugin.cs), [component code](OpenGraphComponent.razor.cs) and [helper](OpenGraphPluginHelper.cs).

**Configuration example, relocated from PRD v0.5:**

```json
{
  "Plugins": [
    {
      "Id": "open-graph",
      "Options": { "TwitterSiteId": "@example", "TwitterCreatorId": "@author" }
    }
  ]
}
```

Read nullable options by typed `TryGetValue` without mutation. Equivalent input contexts must produce equivalent metadata values and optional-tag presence across both paths. Retain the following defaults except for the explicit creator change:

| Input / context | Accepted target |
| --- | --- |
| `TwitterSiteId`, `TwitterCreatorId` | Missing/null/non-string values are ignored; empty/whitespace values omit optional tags |
| `Document.Metadata.TwitterHandle` | Non-whitespace metadata overrides the creator option |
| Title/description | `UseContentMetadata` requires a document, no collection and a non-blank `SourcePath`. Use document title plus site title, and document description with null fallback; otherwise use site title/description |
| Creator scope | Both paths retain creator only for an individual source-backed `ContentKind.Post` for which content metadata is used. Suppress it for pages, collections and source-less posts |
| Site metadata | Use supplied `Site.Locale` and `Site.Title`; do not recompose engine locale/date routes |
| Fixed tags | `og:type` is `website`; `twitter:card` is `summary_large_image`, including when image tags are omitted |

The hook has no collection parameter and uses the host-provided document; synthetic collection documents have no source path. The component additionally considers its `Documents` cascade. Compare equivalent contexts rather than inventing a new hook parameter or engine pass. HTML serialization/whitespace need not be identical.

**Delivery / verification:** `OpenGraphMetadata.Create` resolves both paths' values and creator eligibility. The hook supplies its document; the component additionally supplies its `Documents` cascade. Both suppress source-less-post creators. Comparable source-backed/source-less posts, pages, synthetic collections and missing documents are verified in [cross-surface contract tests](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphContractTests.cs), alongside typed/default options, defensive/read-only snapshots, nested input preservation, author overrides and null-only description fallback. Assertions compare parsed metadata values and tag keys rather than byte serialization. Existing [hook](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphPluginTests.cs), [helper](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphPluginHelperTests.cs) and [bUnit](../../test/ScissorHands.Plugin.OpenGraph.Tests/OpenGraphComponentTests.cs) tests remain part of the suite.

## OG-TR-002: Hook and component integration

**State / source:** Confirmed requirement (user, 2026-09-14); OG-FR-001 and shared P-FR-001/P-FR-004; T-001/T-002/T-003. Preserve host selection/insertion while replacing the missing-context fallback with an explicit failure.

Retain `Id="open-graph"` and non-empty implementation name. The hook checks cancellation, replaces all exact paired markers `<plugin:open-graph></plugin:open-graph>` using `StringComparison.OrdinalIgnoreCase` and leaves unmarked HTML unchanged for otherwise valid inputs. Paired markers are the supported hook syntax; self-closing support is not added. It does not enforce host enablement or deduplicate tags.

The alternative component syntax is `<OpenGraphComponent Id="open-graph" />` within the upstream cascade. Select one path per intended insertion to avoid duplicates. The hook marker and self-closing Razor component syntax are distinct contracts.

The component must call `base.OnParametersSet()`, clear derived fields and recompute from the selected manifest/context. An absent manifest emits nothing; a matched manifest with missing required site/origin information must fail clearly rather than render default tags. Missing document context is valid for site-level pages when site configuration is valid.

**Delivery / verification:** the component calls its base lifecycle, clears all existing protected derived fields, returns only for an absent selected manifest, then resolves required context and new values. Missing site/invalid origins throw `ArgumentException`; they no longer render default tags. Tests cover ID changes with unrelated/duplicate labels, upstream rejection of invalid ID syntax, option and context changes, collection/source-less/missing-document transitions, disabling/removal and re-addition, plus invalid context on enablement/update. An absent manifest bypasses local site/image validation entirely. The hook's entry-cancellation assertion is now awaited and checks the observed token with otherwise invalid context. Mixed-case repeated markers, self-closing/near-match non-markers, no-marker identity and preservation of existing metadata/unrelated HTML are covered. There is no global deduplication.

Blazor notifies separate cascading values independently. A host that removes both a manifest and required site context must disable the manifest first; on re-enabling, provide the site first. Tests exercise real cascade notifications, not a fictitious atomic update. A matched manifest observed with missing required context remains an explicit failure; the plugin does not swallow it as a transient success.

## T-005: Content and image URL boundaries

**State / source:** Confirmed requirement (user, 2026-09-14); P-FR-005, shared P-NFR-005; [local helper](OpenGraphPluginHelper.cs) and [Core URL-helper API reference](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/docs/website-documentation.md#shared-url-helpers). Retain the original catalog ID and locally agreed validation/omission rules; the helper reference describes dependency semantics, not product policy.

For non-blank slugs, use `ContentUrlHelper.GetContentUrl`: trim outer whitespace, normalize both slash separators, escape segments and reject literal `.`/`..` segments. Convert the helper's root result `.` to the site root. Null/blank local slugs also return the site root; this local fallback does not change upstream null-argument behavior.

When emitting metadata, require a supplied site and an absolute HTTP(S) `SiteUrl` with a non-empty host. Missing/blank/malformed or unsupported-scheme site URLs must fail with actionable context, not return relative social URLs. Compose site-local URLs with `SiteUrl` and `BaseUrl`; preserve subpaths rather than independently adding routes, dates or locales. The publication root is a URL without a query/fragment; a `SiteUrl` path prefix is retained. `BaseUrl` is a local subpath without a query/fragment or authority/scheme, with optional outer slashes and local separator normalization. This avoids appending content paths inside an unrelated query or fragment.

Hero images select a non-whitespace document image, otherwise the site image. If both are absent, omit both `og:image` and `twitter:image`; the helper may retain an empty-string absence result, without changing its public return type. Other optional-tag defaults and the card type do not change as an incidental consequence.

The released `SiteManifest` initializes `HeroImage` to an external `hero.jpg`. An omitted site-image setting therefore inherits a nonblank fallback; it is not image absence. Consumers suppress that default with `Site.HeroImage = ""` and no document image. Absent-image test fixtures must explicitly clear the inherited value.

Classify images before normalization: accept site-local paths (including a leading single `/`) and absolute HTTP(S) URLs with a host. Other absolute schemes, malformed absolute references and network-path references such as `//host/image.png` must not be silently reinterpreted as local assets; use an explicit HTTP(S) form for an external image. Preserve supported query strings, fragments, existing percent encoding and significant trailing slashes, without applying content-slug escaping to images. Use Core's image helper for its narrow formatting semantics only, after local input policy is enforced.

**Delivery design:** both public URL helpers validate the required publication context, including when the image result is absent. `GetHeroImageUrl` retains its public `string` return type and returns `string.Empty` only for optional image absence with valid site context. Both renderers conditionally omit image tags for that sentinel; `summary_large_image` is unchanged.

The selected image is validated before normalization. A nonblank reference containing a control character or malformed `%` escape fails. Outer whitespace is trimmed; local path separators normalize before network-path classification so `//`, `\\`, `/\` and `\/` prefixes cannot become local assets. A colon in the first path segment requires a well-formed absolute HTTP(S) reference, including an explicit `://` and host; unsupported schemes and malformed web references cannot be silently localized. Absolute references containing backslashes or unescaped whitespace fail rather than relying on permissive `Uri` repair.

For local images, split the path from the first query/fragment delimiter, normalize only path separators, then apply Core's narrow `GetImageUrl` formatting (leading `/` removal). Do not use `GetContentUrl` on images or round-trip through `Uri.ToString`, which could change escaping. Preserve the image suffix, existing percent-escape spelling, ordinary local path text and meaningful trailing slash. For external HTTP(S) images, retain the supplied origin/reference rather than prefixing the site. No filesystem, network or image-fetch API is used. Diagnostics name the context/field without reflecting arbitrary image/slug payloads.

**Verification:** cross-surface cases cover root/subpath and `SiteUrl` prefixes, HTTP/HTTPS, required-site failures, malformed/disallowed origins/images, disguised network references, control characters, malformed and existing percent escapes, query/fragment data, trailing slashes, local separator/Unicode/space behavior, selected-document precedence, site fallback and absent-image omission. Obsolete null-site-helper and relative-origin assertions were changed to explicit failures; content helper escaping, dot traversal and root `.` mapping remain covered. Host mounting and provider acceptance remain separate checks.

## OG-TR-003: Output and preview boundaries

**State / source:** Confirmed requirement (user, 2026-09-14); OG-NFR-001, shared P-NFR-003 and T-006. Keep metadata context and external behavior explicit.

Normal metadata must remain text in both renderers: escape the hook's attribute values correctly while preserving Razor encoding and avoiding double encoding. Metadata containing quotes, markup-like or placeholder-like text must not become elements, attributes or further template substitutions. Do not blanket-sanitize unrelated document HTML. Apply T-005's accepted URL policy separately from HTML encoding.

Retain Open Graph output in preview and production under the same validation rules. Neither path fetches external images or calls social providers during generation; consumers can subsequently request image URLs. No crawler-acceptance, privacy, offline or delivery guarantee is claimed. OG-Q-004 is settled without a suppression feature.

**Delivery / verification (OG-Q-002):** the hook appends each metadata attribute once using `HtmlEncoder.Default`, then performs one ordinal-case-insensitive replacement over the original HTML. There is no chained metadata substitution or subsequent interpretation of inserted metadata. Razor retains its normal attribute encoding; no `MarkupString`/raw output is introduced. Parsed-value tests cover quotes, apostrophes, ampersands, script-like text, already-entity-looking input, old template tokens and paired-marker-shaped text in titles/descriptions/site names/locale/account handles. They assert original values, matching keys, no injected scripts/attributes, preserved unrelated HTML and image query encoding. Preview and production tests exercise the same output and invalid-origin paths, without provider requests.

## Traceability and verification

| Product baseline | Technical coverage | Evidence / limits |
| --- | --- | --- |
| P-FR-003 | OG-TR-001/003 | Shared resolver and equivalent-input parsed-value parity/encoding regressions delivered; unequal host tag context tracked in OG-Q-005 |
| P-FR-005 | T-005 | Required-origin, image-omission and URL policy implemented; generated component tag-route gap tracked in OG-Q-005 |
| OG-FR-001 | OG-TR-002 | Paired markers, explicit missing-context failures, awaited cancellation and lifecycle transitions verified locally |
| OG-NFR-001 | OG-TR-003 | Preview/production output and validation regressions; no provider acceptance |
| Shared P-FR-001/P-FR-004 | OG-TR-002, T-001/T-002/T-003 | Exact identity, supported insertion and absent-manifest behavior |
| Shared P-FR-006 | T-009 and this pair | Gateway links, versioned baseline and evidence mapping |
| Shared P-NFR-001 | T-007/T-008 | Resolved release, targeted tests and completed [shared build/sample/package validation](../../TRD.md#implementation-evidence-2026-09-15); actual publishing verification remains separate |
| Shared P-NFR-002 | T-002/T-004, OG-TR-001/002 | Read-only snapshots/nested preservation, observable errors and awaited entry cancellation |
| Shared P-NFR-003 | OG-TR-003, T-006 | Context-specific validation review; no comprehensive audit claim |
| Shared P-NFR-005 | T-005, OG-TR-001/003 | Resolved URLs/site locale; head-only output, no visible UI or measured performance contract |

Use [AGENTS.md](../../AGENTS.md) for commands. Package assembly, README, license, icon, symbols and dependency metadata were inspected in the [completed shared validation](../../TRD.md#implementation-evidence-2026-09-15). Shared [T-008](../../TRD.md#t-008-package-and-consumer-documentation) and [Q-005](../../PRD.md#shared-release-question) govern release/support/recovery and optional-only deferral policies. Implementation, regressions and scoped sample/package checks are delivered; actual publishing verification and authorization remain separate release gates. Package creation and policy agreement are not authorization to publish.

## Delivery evidence

**Delivery follow-up (2026-09-15, UTC+09:00):** the signed-off plugin PRD/TRD v0.7 and catalog v0.8 baseline are unchanged. This is an implementation/evidence update, not a new requirements sign-off.

- .NET SDK `10.0.401`, .NET 10, restored Core/Plugin `1.0.0-preview.20260914.1`; existing xUnit v3/Microsoft.Testing.Platform, bUnit, Shouldly and NSubstitute configuration retained. No dependencies upgraded.
- Targeted Release build succeeded with **0 warnings and 0 errors**. Microsoft.Testing.Platform discovered and passed **186 tests**, with **0 failed and 0 skipped**, using the commands below from the repository root (Windows path separators were used in the executed commands). `git diff --check` also passed for the owned source/test directories. Zero-test runs are not acceptance.
- The suite is isolated: no browser, external image fetch or social-provider request, and no new filesystem/network dependency.

```powershell
dotnet build ./test/ScissorHands.Plugin.OpenGraph.Tests/ScissorHands.Plugin.OpenGraph.Tests.csproj -c Release --no-restore -warnaserror
dotnet test --project ./test/ScissorHands.Plugin.OpenGraph.Tests/ScissorHands.Plugin.OpenGraph.Tests.csproj -c Release --no-build --verbosity normal
```

**Completed shared validation:** the [catalog implementation evidence](../../TRD.md#implementation-evidence-2026-09-15) records a full Release build with zero warnings/errors and **347 passing tests** (Google Analytics 135, Open Graph 186, sample 26). Eight isolated static builds inspected 64 pages across root/subpath and site-image presence/absence; both preview modes were checked over local HTTP with assets, without JavaScript execution or provider requests. The `1.0.0-preview.implementation` packages were inspected for assemblies, READMEs, licenses, icons, dependencies and symbols, then cleaned up. These checks are complete, not pending.

Ordinary-page/equivalent-context parity passed. The tagged-route `og:url` discrepancy was verified exactly as OG-Q-005, not normalized away. Production deployment/mounting, browser JavaScript behavior, provider/crawler acceptance and actual publishing verification are not established by this evidence. There is no completed security audit or release approval claim.

## Gaps and readiness

The [PRD decision records](PRD.md#plugin-questions-and-acceptance-limits) retain product choices and routing IDs. OG-Q-001's paired-marker documentation is aligned; OG-Q-004's preview behavior is retained and verified. OG-Q-002's image/URI/encoding work and OG-Q-003's equivalent-input parity, explicit missing-context failures and awaited cancellation/removal regressions are implemented. No accepted plugin-runtime behavior is intentionally deferred, but OG-Q-005 is an unresolved host-integration limitation, not full-host parity or an approved deferral. Scoped shared sample/package validation is complete; @justinyoo owns tracking the remaining host limitation, actual publishing verification and release authorization.

### OG-Q-005: Generated tag pages receive unequal host context

**Confirmed integration finding (2026-09-15, UTC+09:00):** isolated generation with ScissorHands.Web `1.0.0-preview.20260914.1` exposed different tag-page `og:url` values. The [released StaticSiteGenerator source](https://github.com/getscissorhands/ScissorHands.NET/blob/7b5db6e1f27327cd8be50c08e4163e72e0a28425/src/ScissorHands.Web/Generators/StaticSiteGenerator.cs), matching the installed package's repository commit, explains the difference:

- `RenderTagListPageAsync` supplies `TaggedDocuments` and renders the layout before constructing `tagListDocument` with `Slug = "tags"`. That resolved synthetic document is supplied only afterward to `WriteRenderedHtmlAsync` and the hooks.
- `RenderTagPageAsync` similarly supplies `Tag`, `TaggedPosts` and `TaggedPages`, renders without a `Document`, then constructs the synthetic tag document with its resolved slug for the hook path.
- The released `MainLayoutBase` and `PluginComponentBase` expose no current-route URL that the component could use instead.

Consequently, components receive no document and correctly apply the documented site-root fallback, while hooks retain the supplied actual tag route (observed examples: `tags`, `tags/plugins`, `tags/preview`). Titles, creator suppression and image values matched in the scoped generated-output comparison, but `og:url` did not. These upstream inputs are **not equivalent**: passing equivalent-input plugin tests does not demonstrate full-host parity.

**Workaround / closure:** use paired-marker hook mode where accurate generated tag-page canonical URLs are required. Upstream must provide a resolved document or route cascade before component rendering; then revalidate the consuming layout and both paths against that released contract. Do not infer tag routes from labels/collections, construct engine routes in the plugin or normalize all source-less document slugs to the root. @justinyoo owns tracking and integration revalidation; shared sample records own the released-input-shape regression evidence. Preview/output validation rules are unchanged. This finding is not provider/crawler acceptance and does not alter historical sign-off.

### Breaking migration and release readiness

**Breaking migration:** the earlier baseline permitted hook creators for source-less posts, missing-site component defaults, relative URL fallback, broad absolute image schemes and raw hook metadata substitution. The delivered implementation removes those behaviors. Configure valid site context and an absolute HTTP(S) publication root `SiteUrl`; use a site-local `BaseUrl` and T-005's supported image forms; provide normal metadata as unencoded text; account for creator/image tags that may no longer appear. Optional absent images are omitted, not a reason to disable the plugin. Public URL helper return types and component protected properties are retained, but missing/invalid site context now throws `ArgumentException` even for a direct helper call. An absent component manifest remains silent. See the [README migration steps](README.md#breaking-migration-from-the-earlier-permissive-behavior).

No analytics/consent service, database, account system, remote-generation API, navigation subsystem or accessibility/browser-conformance program is part of this plugin baseline. External image and locale/output semantics remain applicable; future features require a separate applicability review.

**v0.5 confirmation (2026-09-14):** the user explicitly confirmed metadata consistency, required versus optional metadata, URL/output handling and regression coverage. OG-TR-001/002/003 and T-005 now record confirmed requirements without changing scope, migration effects or IDs.

**v0.6 separation (2026-09-14):** consolidates package/source metadata, JSON/component examples, exact field/URL contracts and technical delivery/migration records here. PRD question IDs retain their product decisions and link to this evidence; no behavior, ID or acceptance obligation changes.

**v0.7 reference policy (2026-09-14):** makes local requirement authority and release-matched API references explicit. All metadata, URL, lifecycle, privacy, migration and verification obligations remain unchanged.

**Delivery follow-up (2026-09-15, UTC+09:00):** implements the accepted behavior and local regression evidence without changing reviewed document versions or the historical sign-off at `02fbfc029c7560e2dc24543ee99b6d3fdce2b669`.

**Readiness:** implementation delivered against plugin PRD v0.7 and shared v0.8 baselines with targeted equivalent-input tests and completed shared build/sample/package validation. OG-Q-005 remains an explicit generated-tag component integration gap; full-host parity is not claimed. Actual publishing verification and release authorization remain required gates, not approved deferrals. Shared T-008 records reported publishing setup and limits of independent verification. The completed validation does not establish production/crawler acceptance, a completed audit or release approval.
