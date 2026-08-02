# MAUverce 1.12.1 release record

Built: 2026-08-02

Classification: engineering release candidate for native iOS portal parity with
Android 1.12.0 surfaces.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.0` | `versionCode` 29 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.1` | `CURRENT_PROJECT_VERSION` 30 |

- Package: `com.pmi4freal.mauverse3`
- Branch: `cursor/ios-portal-parity-010d`

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

- Hosted `Native SwiftUI iPhone Release (unsigned)` must be green on the release commit.
- Production Apple signing and device smoke tests remain open gates.
