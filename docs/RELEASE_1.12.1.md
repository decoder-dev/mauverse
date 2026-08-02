# MAUverce 1.12.1 release record

Built: 2026-08-02

Classification: engineering release published as GitHub Release `v1.12.1`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.0` | `versionCode` 29 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.1` | `CURRENT_PROJECT_VERSION` 30 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `23749894a01e295b4ac1260c7d5d336e161cb43a` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.1
- Pull request: https://github.com/decoder-dev/mauverse/pull/18 (merged)
- CI on release commit: https://github.com/decoder-dev/mauverse/actions/runs/30764355781

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `7af8a3de23cae9cd0e5eba956af0ac7c95723458209716d44e84c6885814f833` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `1315cf660d53e409d279b9bbbe5ce31d299b189154a5a0435cf05d3c85b37afc` |

## iPhone changes

- Portal guides: Студенту, Абитуриенту, Наука, International
- Curated digital services hub
- Native events calendar (`/press/calendar/rss/`)
- News filters Абитуриент / Календарь
- Contacts and payment requisites with call/copy
- Campus branches, transport tips, 2GIS route, Yandex panorama
- Settings university links (site, PDN, sveden)
- Schedule teacher/room filter chips (exclusive selection)

## Verification

- [x] All four CI jobs green on `2374989`.
- [x] GitHub Release `v1.12.1` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing (not `CN=Android Debug`).
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke test from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
