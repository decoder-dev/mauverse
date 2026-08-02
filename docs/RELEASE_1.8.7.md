# MAUverce 1.8.7 release record

Built: 2026-08-02

Classification: engineering release candidate, not production-approved until
`docs/RELEASE_CHECKLIST.md` gates pass on the exact promoted commit and artifact.

## Android / iPhone targets

- Application name: `MAUverse`
- Package: `com.pmi4freal.mauverse3`
- Version: `1.8.7` (`versionCode` 22)
- Release branch: `cursor/university-portal-parity-010d`
- Pull request: https://github.com/decoder-dev/mauverse/pull/16

## Product changes

- Portal guides from mauniver.ru: Студенту, Абитуриенту, Наука, International.
- Digital services hub: ЭИОС, библиотека, webmail, dorm IT ticket, Intra, PROMT.
- Native events calendar from `/press/calendar/rss/` plus site calendar link.
- News filters: Абитуриент (`/abit/news/rss/`) and Календарь.
- Contacts and payment requisites screen with call/copy actions.
- Campus navigator: branch campuses, transport tips, route and panorama actions.
- Schedule filters: room and teacher selection no longer block each other.
- Settings links: official site, personal-data policy, sveden.

## API changes

- `ParserType.APPLICANT` (`news_type=8`) → `/abit/news/rss/`
- `ParserType.CALENDAR` (`news_type=9`) → `/press/calendar/rss/`

## Verification

- Local `mauverse_mobile.Tests`: 31 passed.
- Local `mauverse-api` unittest: 59 passed.
- Hosted CI on the release commit must show green `backend`, `api-image`,
  `android-release`, and `ios-release-unsigned` before promotion.
- Production signing, device smoke tests, and API traffic promotion remain open
  gates from `docs/RELEASE_CHECKLIST.md`.

## Promotion gates still required

- [ ] All four CI jobs green on the exact release commit.
- [ ] Protected production Android keystore signing (not `CN=Android Debug`).
- [ ] Device smoke test from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
- [ ] Privacy / forms retention approval if distributing forms more widely.
