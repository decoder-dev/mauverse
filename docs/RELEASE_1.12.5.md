# MAUverce 1.12.5 release record

Built: 2026-09-01

Classification: engineering release published as GitHub Release `v1.12.5`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.5` | `versionCode` 39 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.5` | `CURRENT_PROJECT_VERSION` 39 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `025f969fcb4ef4292af3b89d2ffda8f2ec758943` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/33491913305

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `b7a5e1497969ee8d50f38dc2cbb73f2b27992f03c7fdb772c6a544d4689c039a` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `032d1402f082595d7415569445cf098f4a341372ff14f9092c91cc2eeec831f3` |

## Product changes

- Group input hints in profile (Android + iOS): autocomplete from schedule while typing.
- API `get_subgroups` exact-match fix; null-safe subgroup handling in profile/login flows.
- Unified tab screen spacing on iOS and Android (28pt side margins, 12pt gutter, 108pt tab clearance).
- Visual alignment fixes: profile action rows, schedule date picker, news/notification card padding.
- iOS: correct night greeting (04:00 shows «Доброй ночи»).
- iOS: long-text rows no longer overflow screen (profile, schedule, news, home, debts).
- In-app browser: external pages no longer clip flush-left content (iOS + Android).
- iOS: notification cards grow with Dynamic Type instead of clipping.
- History/authorship consolidated under `decoder-dev`.

## Verification

- [x] All four CI jobs green on release commit / run `33491913305`.
- [x] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing.
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
