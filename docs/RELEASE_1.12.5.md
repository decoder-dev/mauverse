# MAUverce 1.12.5 release record

Built: 2026-09-05

Classification: engineering release published as GitHub Release `v1.12.5`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.5` | `versionCode` 40 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.5` | `CURRENT_PROJECT_VERSION` 40 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `TBD` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- CI on release artifacts: `TBD`

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `TBD` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `TBD` |

## Product changes

- Group input hints in profile (Android + iOS)
- Unified tab spacing; visual overflow and clipping fixes across tabs
- iOS: reduced double-counted tab bottom inset; hide empty nav bars; Dynamic Type IconTile
- iOS: schedule lesson rail/time column no longer clip; news filter no longer labels cards «ВСЕ»
- iOS: profile/group entry switches tabs instead of pushing a dead-end Profile screen
- Android: PagePaddingNoBottom for list tabs so 108pt clearance no longer shrinks the viewport
- Android: center MaxContentWidth columns; compact quick-access tiles; notification carousel gutter
- In-app browser flush-left clipping fix; notification cards grow with Dynamic Type
- History/authorship consolidated under `decoder-dev`

## Verification

- [ ] All four CI jobs green on release commit
- [x] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums
- [ ] Protected production Android keystore signing
- [ ] Apple distribution signing of the native IPA on an approved Mac
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner
