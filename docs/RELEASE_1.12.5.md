# MAUverce 1.12.5 release record

Built: 2026-09-01

Classification: engineering release published as GitHub Release `v1.12.5`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.5` | `versionCode` 34 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.5` | `CURRENT_PROJECT_VERSION` 34 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `17f6f98fc0949cc4d30b092a2024c70b9569373f` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- Pull request: https://github.com/decoder-dev/mauverse/pull/34 (group hints)
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/33455483313

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `5f14b84f8dd0e2e423ef2db26f5ae444f2015cf7d5e5c5b4b27aedd8921af387` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `199a8dfcab881eace5fc8871d5a45a714e06071c0b7c9ff9463442881850659d` |

## Product changes

- Group input hints in profile (Android + iOS): autocomplete from schedule while typing.
- API `get_subgroups` exact-match fix; null-safe subgroup handling in profile/login flows.
- History/authorship consolidated under `decoder-dev`.

## Verification

- [x] All four CI jobs green on release commit / run `33455483313`.
- [ ] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing.
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
