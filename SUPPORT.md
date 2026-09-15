# Support

Support for the official ScissorHands.NET plugins is best-effort through
[GitHub Issues](https://github.com/getscissorhands/plugins/issues), triaged by
@justinyoo. There is no guaranteed response time or commitment to maintain every
historical preview.

## Documentation

- [Plugin catalog and usage guides](README.md#list-of-plugins)
- [Local preview and static-build sample](sample/README.md)
- [Contributor setup and workflow](CONTRIBUTING.md)
- [Product requirements](PRD.md) and [technical compatibility](TRD.md)

Each plugin's README describes its own configuration, supported integration
surfaces, and preview behavior. The sample uses a synthetic analytics ID, but
browsing it can still make Google requests; follow its guide for static or
isolated validation.

## Bugs and Feature Requests

Search existing issues before opening a
[bug report](https://github.com/getscissorhands/plugins/issues/new?template=bug_report.yml)
or [feature request](https://github.com/getscissorhands/plugins/issues/new?template=feature_request.yml).
Include the plugin ID, exact plugin/engine versions, integration mode, and a
minimal reproduction or description of the desired outcome. Remove credentials
and private site content from examples and logs.

These packages consume the upstream engine. If generation, serving, or another
upstream dependency appears involved, describe that evidence rather than
assuming the plugins implement those services.

## Private Reports

Report suspected vulnerabilities privately using [SECURITY.md](SECURITY.md).
For conduct incidents, use the enforcement contact in
[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). Do not open public issues for either.

## Preview Releases and Recovery

Plugins are versioned independently from the engine and currently remain preview.
Compatibility is limited to verified combinations. Code fixes use a new preview
version; published packages are not replaced. Consumers may temporarily pin a
previously verified combination, but no automatic rollback is promised.

The [release and recovery guide](README.md#support-and-recovery) remains the
source for publication recovery and approval-gated deprecation or unlisting.
This support guide does not authorize a release or change those policies.
