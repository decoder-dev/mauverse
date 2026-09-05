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
- Release commit: `d585bd2d075466ca8b83bcbb8114e4548c34380a` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/33982522548

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `8f5950f26aac2a08f9c113b44c7bb4bba68bde8dc875fa67bd0ae866e3f1ed96` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `e6e9d38dce5dde4050a7f849d390bf9e128267fcbb5813ddae2de421591579ea` |

## Product changes

- Group input hints in profile (Android + iOS)
- Unified tab spacing; visual overflow and clipping fixes across tabs
- iOS: reduced double-counted tab bottom inset; hide empty nav bars; Dynamic Type IconTile
- iOS: schedule lesson rail/time column no longer clip; news filter no longer labels cards «ВСЕ»
- iOS: profile/group entry switches tabs instead of pushing a dead-end Profile screen
- Android: PagePaddingNoBottom for list tabs so 108pt clearance no longer shrinks the viewport
- Android: center MaxContentWidth columns; compact quick-access tiles; notification carousel gutter
- Android: CollectionView tab clearance via Footer (XamlC-safe) instead of Padding
- In-app browser flush-left clipping fix; notification cards grow with Dynamic Type
- History/authorship consolidated under `decoder-dev`

## Verification

- [x] All four CI jobs green on release commit / run `33982522548`
- [x] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums
- [ ] Protected production Android keystore signing
- [ ] Apple distribution signing of the native IPA on an approved Mac
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner
