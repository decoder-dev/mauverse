# MAUverce 1.12.5 release record

Built: 2026-09-01

Classification: engineering release published as GitHub Release `v1.12.5`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.5` | `versionCode` 38 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.5` | `CURRENT_PROJECT_VERSION` 38 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `c965c520cd360e3a561107b6ef67a4cc099c3dc7` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/33487968909

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `8f5f2b93edd3d6d39389f316920be294bd19440c5ddb79a2a8f2c7a121f74309` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `eb10607cb261091e49e5e0bd21db72d307151387a13ecdc0e3e7db180f99b0be` |

## Product changes

- Group input hints in profile (Android + iOS): autocomplete from schedule while typing.
- API `get_subgroups` exact-match fix; null-safe subgroup handling in profile/login flows.
- Unified tab screen spacing on iOS and Android (28pt side margins, 12pt gutter, 108pt tab clearance).
- Visual alignment fixes: profile action rows, schedule date picker, news/notification card padding.
- iOS: correct night greeting (04:00 shows «Доброй ночи»).
- iOS: long-text rows no longer overflow screen (profile, schedule, news, home, debts).
- History/authorship consolidated under `decoder-dev`.

## Verification

- [x] All four CI jobs green on release commit / run `33487968909`.
- [x] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing.
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
