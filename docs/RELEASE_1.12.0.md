# MAUverce 1.12.0 release record

Built: 2026-08-02

Classification: engineering release candidate, not production-approved until
`docs/RELEASE_CHECKLIST.md` gates pass on the exact promoted commit and artifact.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.0` | `versionCode` 29 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.0` | `CURRENT_PROJECT_VERSION` 29 |

- Package: `com.pmi4freal.mauverse3`
- Release branch: `cursor/university-portal-parity-010d`
- Pull request: https://github.com/decoder-dev/mauverse/pull/16

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

- Local `mauverse_mobile.Tests` and `mauverse-api` unit tests on the RC branch.
- Hosted CI must be green for `backend`, `api-image`, `android-release`, and
  `Native SwiftUI iPhone Release (unsigned)` on the exact release commit.
- Production signing, device smoke tests, and API traffic promotion remain open
  gates from `docs/RELEASE_CHECKLIST.md`.

## Promotion gates still required

- [ ] All four CI jobs green on the exact release commit.
- [ ] Protected production Android keystore signing (not `CN=Android Debug`).
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke test from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
- [ ] Privacy / forms retention approval if distributing forms more widely.
