# MAUverce 1.12.5 release record

Built: 2026-09-01

Classification: engineering release published as GitHub Release `v1.12.5`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.5` | `versionCode` 36 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.5` | `CURRENT_PROJECT_VERSION` 36 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `54e8aa28c936d02d41c9454c1624b1d0ab4d729f` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- Pull request: https://github.com/decoder-dev/mauverse/pull/34 (group hints)
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/33457466714

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `9630fdff6f8259b5bafdef4f52987f241916ed0ea3c40d690fb3d4819e19ab7e` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `0bec3cd3b1b3716701da373b20647c997b04b66628ab0e63876fa18ceee0c3de` |

## Product changes

- Group input hints in profile (Android + iOS): autocomplete from schedule while typing.
- API `get_subgroups` exact-match fix; null-safe subgroup handling in profile/login flows.
- Unified tab screen spacing on iOS and Android (28pt side margins, 12pt gutter, 108pt tab clearance); all tab screens use shared layout constants.
- History/authorship consolidated under `decoder-dev`.

## Verification

- [x] All four CI jobs green on release commit / run `33457466714`.
- [x] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing.
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
