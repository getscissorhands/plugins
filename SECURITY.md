# Security Policy

## Reporting a Vulnerability

Please do **not** open a public GitHub issue for security vulnerabilities.

Send a private report to [ask@getscissorhands.app](mailto:ask@getscissorhands.app).
This policy covers the plugins and local sample maintained in this repository;
identify upstream dependencies involved so maintainers can coordinate with the
appropriate project.

Include the following in your report:

- Affected plugin IDs, exact plugin and engine package versions, and integration mode
- Description, potential impact, and a minimal reproduction using synthetic data
- Relevant configuration or generated output, with credentials and private content removed
- Suggested fix, if available

Do not include live credentials, personal data, or an exploit in a public issue
or pull request. Report conduct concerns through
[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md), not this security contact.

## Supported Versions

Packages are currently preview releases, versioned independently from the engine.
Compatibility covers verified plugin/engine combinations, not every version
allowed by a floating dependency range. See the
[catalog compatibility record](TRD.md#t-007-shared-build-and-compatibility-configuration).

@justinyoo triages reports on a best-effort basis. There is no guaranteed response
or remediation time and no commitment to maintain every historical preview.
Please report the affected version even if it is not the latest preview.

## Disclosure Policy

Keep vulnerability details private while maintainers investigate and coordinate
remediation and disclosure with you. Discuss any public advisory and reporter
credit with the maintainer; do not assume a disclosure date or permission to
identify the reporter.

Code fixes ship in a new preview version rather than replacing an already
published package. Release authorization, required evidence, and approval for
deprecation or unlisting remain governed by the
[existing release and recovery policy](README.md#support-and-recovery).
This policy is a reporting process, not a claim of a completed security audit.
