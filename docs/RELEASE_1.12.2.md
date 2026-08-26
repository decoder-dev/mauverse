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
- Release commit: `50e190935e27ef9b44c680bb3cf542a8490b350b` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.2
- Pull requests: https://github.com/decoder-dev/mauverse/pull/21 (product), https://github.com/decoder-dev/mauverse/pull/22 (release)
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/30767400489

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `7b11c5dcce5fe11ccad1da8e0e8eeb89a0264528e4001cf3da4ecaab3206b244` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `0327900a2446474a247e03e8b46afdbfe610cebb9fd5f16a7749b0ad7290eb16` |

## Product changes

- News titles: decode HTML entities (`&thinsp;`, `&nbsp;`, numeric entities) so amounts render as `100 000` instead of literal `&thinsp;`.
- API RSS parser: same thin-space cleanup after `html.unescape`.
- Phones: Murmansk local 6-digit PBX numbers dial as `+7 8152 …`; trailing `(3045)` / `доб. 3045` become `;ext=` extensions instead of being concatenated into a bogus long number.
- Android department contacts: tap-to-call uses the same formatter.
- Android Release: suppress CA1822 on `DetailTelephoneViewModel.CallAsync` for `warnaserror` CI.

## Verification

- [x] All four CI jobs green on PR #22 / run `30767400489`.
- [x] GitHub Release `v1.12.2` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing (not `CN=Android Debug`).
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke test from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
