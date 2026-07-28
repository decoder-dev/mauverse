# MAUverce production audit

Updated: 2026-07-16

Status values: `DONE`, `ACTIVE`, `BLOCKED`, `PLANNED`. `DONE` means the result is
verified in the current tree; historical claims and planned CI runs do not count.

## P0: release blockers

| Status | Finding | Required result |
| --- | --- | --- |
| DONE | Access tokens previously existed in the local user model | Tokens are stored in platform secure storage and removed from the SQLite model |
| DONE | Android application data could expose credentials through backup | `android:allowBackup="false"` is set |
| DONE | An experimental websocket broadcast messages without room isolation | The unused route and connection manager are absent |
| ACTIVE | Current APK is signed with `CN=Android Debug` | Configure protected production signing, verify the certificate, and archive a versioned artifact |
| ACTIVE | No hosted CI run is available in this workspace | Require all corrected workflow jobs on the exact release commit before promotion |
| DONE | Concurrent backend changes temporarily left tests out of sync | Current source passes all discovered tests, Ruff, `pip check`, and compileall |
| DONE | Android Release was blocked by unsupported accessibility XAML | Unsupported properties were removed; MAUverce 1.7.3 builds with warnings treated as errors |

A current-tree scan found no plaintext production passwords or private signing
keys. Git history could not be audited because this workspace has no valid Git
metadata; repository history still requires a secret scan before release.

## P1: high priority

| Status | Finding | Impact / action |
| --- | --- | --- |
| DONE | Backend verification omitted Ruff and compileall in CI | Workflow runs tests, `pip check`, Ruff, and compileall with pinned dev tooling |
| DONE | CI had no minimal permissions, bounded timeouts, Android artifact, or NuGet cache | Workflow now has `contents: read`, timeouts, cancellation, pip/NuGet caches, checksums, and an unsigned artifact |
| ACTIVE | Hosted Linux Android and Docker jobs have not run from this workspace | First hosted run must prove workload installation, container build, and health smoke |
| DONE | Docker runtime lacked a healthcheck and deterministic identity | Image runs as UID/GID 10001 and checks `/health`; CI asserts both properties |
| DONE | Liveness and database readiness were not separated | `/health` reports process liveness and `/ready` returns 503 when either database is unavailable |
| ACTIVE | Moodle and mail readiness are not represented by `/ready` | Define monitoring and incident policy without making readiness depend on fragile third-party probes |
| ACTIVE | Python transitive dependencies are not hash-locked | Introduce an approved lock/update process without mixing runtime and dev dependencies |
| DONE | Direct imports relied on undeclared transitive packages | `starlette` and `urllib3` are explicit, pinned runtime dependencies |
| DONE | Naming and comment rules were informal | Root `.editorconfig`, build-time code-style analysis, strict Ruff, and `docs/CODE_STYLE.md` enforce platform conventions |
| ACTIVE | Android NuGet restore has no committed `packages.lock.json` | Generate and validate the lock when MAUI restore is no longer contending with concurrent builds, then use `--locked-mode` in CI |
| ACTIVE | Mobile client has no automated tests | Add testable domain services and a mobile unit-test project |
| DONE | Data-loading pages continued work after navigation | Main data screens cancel their commands and HTTP work on `Disappearing`, then retry safely on `Appearing` |
| DONE | API base URL was hard-coded in source | HTTPS base URL is build-configurable; production route migration still needs deployment coordination |

## P2: product quality

| Status | Finding | Impact / action |
| --- | --- | --- |
| DONE | Branding mixed legacy names | Visible identity is MAUverce while the package id is preserved |
| DONE | Theme choice and settings metadata were incomplete | Persisted system/light/dark selection and operational settings are present |
| ACTIVE | Accessibility coverage is partial | Complete a full TalkBack focus-order and dynamic-text audit |
| ACTIVE | State handling varies across older pages | Standardize loading, content, empty, offline, and failure states |
| ACTIVE | Local database uses `EnsureCreated` rather than migrations | Introduce migrations before schema evolution; page contexts are now transient to prevent cross-page concurrency |
| DONE | Backend documentation endpoints were always enabled | `ENABLE_DOCS` controls docs and defaults to false |
| ACTIVE | No crash reporting or approved privacy-policy link exists | Select approved infrastructure and document ownership |
| ACTIVE | iOS and Mac Catalyst are not verified | Build and smoke-test on supported macOS hardware |

## Functional gaps requiring product/API ownership

| Status | Gap | Dependency |
| --- | --- | --- |
| BLOCKED | Student group messaging | Membership, moderation, retention, notification, and abuse-reporting API |
| BLOCKED | Full indoor building navigation | Floor plans, graph data, accessibility routes, and content owner |
| BLOCKED | Schedule migration to the prospective API | Stable documented endpoint and compatibility contract |
| PLANNED | Remaining native student forms | Approved schemas, recipients, retention policy, and API contract |

## Current verification evidence

- Local backend on Python 3.12.10: all 33 current tests, strict Ruff format/lint, `pip check`, and compileall pass.
- CI declares Python 3.11, matching the Docker runtime; the hosted job is still required as exact-platform evidence.
- The current Android project targets MAUverce 1.7.3 (`versionCode` 11) with SDK `9.0.313` and JDK 17.
- Local Android Release succeeds with `--warnaserror`; full `dotnet format --verify-no-changes` and XAML XML validation pass.
- The archived APK SHA-256 is `5E72C9B7C2D02394FEA33166C90E6DE3B3F994CB9BAB723C692183D0F6A896D1`.
- The current `*-Signed.apk` signer is `CN=Android Debug`; it is an installable engineering artifact, not a production-signed artifact.
- Mobile integration removes dead code and skeleton data, cancels hidden-page loads, uses per-page EF contexts, and preserves the package id for upgrades.
- Workflow syntax passes actionlint 1.7.12; all eight action references are immutable 40-character SHAs.
- Docker cannot be built locally because Docker CLI is unavailable; `api-image` now performs the build, non-root assertion, and health smoke in CI.
- Previously recorded device and frame measurements remain historical evidence and were not rerun during this release-engineering audit.

## External release gates

- Obtain a valid Git repository and a green hosted CI run for the release commit.
- Provision and approve Android production signing outside source control.
- Confirm production API hostname/path, TLS, readiness, logging, alerting, and rate limiting.
- Approve privacy text and retention for forms and messages.
- Run Android release smoke tests and Apple-platform validation where supported.

MAUverce is not production-approved while any P0 item remains `ACTIVE`.
