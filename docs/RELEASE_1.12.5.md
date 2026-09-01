# MAUverce 1.12.5 release record

Built: 2026-09-01

Classification: engineering release published as GitHub Release `v1.12.5`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.5` | `versionCode` 35 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.5` | `CURRENT_PROJECT_VERSION` 35 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `097505fc1cd4f5d4cf6b1dd7c70fa1b4e5f643a8` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- Pull request: https://github.com/decoder-dev/mauverse/pull/34 (group hints)
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/33456756216

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `ae4767be4a0814df5cf4064fbaaedd87fc344f9fed01ccf649731a3f49b15f48` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `49cd2d936e86db7ab333f6225bd2d6e75b84c52891f6c629380a3459dd7043be` |

## Product changes

- Group input hints in profile (Android + iOS): autocomplete from schedule while typing.
- API `get_subgroups` exact-match fix; null-safe subgroup handling in profile/login flows.
- Unified tab screen spacing on iOS and Android (22pt side margins, 12pt gutter, 108pt tab clearance); Services cards no longer flush with screen edges.
- History/authorship consolidated under `decoder-dev`.

## Verification

- [x] All four CI jobs green on release commit / run `33456756216`.
- [x] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing.
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
