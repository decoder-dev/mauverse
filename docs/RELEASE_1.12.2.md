# MAUverce 1.12.2 release record

Built: 2026-08-02

Classification: engineering release published as GitHub Release `v1.12.2`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.2` | `versionCode` 31 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.2` | `CURRENT_PROJECT_VERSION` 31 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `aa04ea534c3ab729b4031af4bf0dd7b4881a2137` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.2
- Pull request: https://github.com/decoder-dev/mauverse/pull/21 (merged)
- CI on release commit: _pending — GitHub Actions blocked by account spending limit / payment_

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | _pending CI_ |
| `mauverse-native-ios-arm64-unsigned.ipa` | _pending CI_ |

## Product changes

- News titles: decode HTML entities (`&thinsp;`, `&nbsp;`, numeric entities) so amounts render as `100 000` instead of literal `&thinsp;`.
- API RSS parser: same thin-space cleanup after `html.unescape`.
- Phones: Murmansk local 6-digit PBX numbers dial as `+7 8152 …`; trailing `(3045)` / `доб. 3045` become `;ext=` extensions instead of being concatenated into a bogus long number.
- Android department contacts: tap-to-call uses the same formatter.

## Verification

- [ ] All four CI jobs green on `aa04ea5` (`backend`, `api-image`,
  `android-release`, `Native SwiftUI iPhone Release (unsigned)`).
- [ ] GitHub Release `v1.12.2` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing (not `CN=Android Debug`).
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke test from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
