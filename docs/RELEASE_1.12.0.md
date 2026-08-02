# MAUverce 1.12.0 release record

Built: 2026-08-02

Classification: engineering release published as GitHub Release `v1.12.0`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.0` | `versionCode` 29 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.0` | `CURRENT_PROJECT_VERSION` 29 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `c60be64d972be930ad68194340944d10a0dc4925` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.0
- Pull request: https://github.com/decoder-dev/mauverse/pull/16 (merged)
- CI on release commit: https://github.com/decoder-dev/mauverse/actions/runs/30763288979

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `6e6b78a22621a520912f487004fafe7b00cc0c351f8d60b14686a961656eb030` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `561cc033124e1bda04380a62a52453516a000895ca11eb4e5ce816c1f0276837` |

## Product changes

### Android (MAUI)

- Portal guides from mauniver.ru: Студенту, Абитуриенту, Наука, International.
- Digital services hub: ЭИОС, библиотека, webmail, dorm IT ticket, Intra, PROMT.
- Native events calendar from `/press/calendar/rss/` plus site calendar link.
- News filters: Абитуриент (`/abit/news/rss/`) and Календарь.
- Contacts and payment requisites screen with call/copy actions.
- Campus navigator: branch campuses, transport tips, route and panorama actions.
- Schedule filters: room and teacher selection no longer block each other.
- Settings links: official site, personal-data policy, sveden.

### iPhone (SwiftUI)

- Ships the native SwiftUI client from `MauverseIOS` (not MAUI iOS).
- Existing feature set: login/session, home, schedule, services, news, profile,
  in-app browser, campus/digital service entry points.
- CI builds an unsigned `mauverse-native-ios-arm64-unsigned.ipa` via XcodeGen +
  Xcode 26.5 with code signing disabled.

## API changes

- `ParserType.APPLICANT` (`news_type=8`) → `/abit/news/rss/`
- `ParserType.CALENDAR` (`news_type=9`) → `/press/calendar/rss/`

## Verification

- [x] All four CI jobs green on `c60be64` (`backend`, `api-image`,
  `android-release`, `Native SwiftUI iPhone Release (unsigned)`).
- [x] GitHub Release `v1.12.0` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing (not `CN=Android Debug`).
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke test from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
- [ ] Privacy / forms retention approval if distributing forms more widely.
