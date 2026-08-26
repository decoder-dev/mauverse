# MAUverce 1.12.4 release record

Built: 2026-08-03

Classification: engineering release published as GitHub Release `v1.12.4`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.4` | `versionCode` 33 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.4` | `CURRENT_PROJECT_VERSION` 33 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `ae2174210065df46c9ff0dbb6bc330c786eac6f8` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.4
- Pull request: https://github.com/decoder-dev/mauverse/pull/26 (merged)
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/30843250564

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `05a88513468f20beea83c0b96789e00f34380ab927408dc1cc58327088c4b22a` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `7f0d246e66c8eb56de4081408754e5b50d7a7361a74112600ffb10795072ce3f` |

## Product changes

- iOS Liquid Glass production pass (`docs/IOS_LIQUID_GLASS.md`).
- DesignSystem: `mauGlass` / thin `mauSurface`, motion, press, a11y, glass stack.
- Home, Schedule, News, Services, Login, Campus on glass; NewsCard opaque-under-glass fixed.
- Chip/tab animations; calendar HTML cleanup.

## Verification

- [x] All four CI jobs green on PR #26 / run `30843250564`.
- [x] GitHub Release `v1.12.4` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing.
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md` + Liquid Glass checklist.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
