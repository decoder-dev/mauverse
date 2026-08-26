# MAUverce 1.12.3 release record

Built: 2026-08-03

Classification: engineering release published as GitHub Release `v1.12.3`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.3` | `versionCode` 32 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.3` | `CURRENT_PROJECT_VERSION` 32 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `40ac91bb5349f8883a7131c8e6de10dce35bd240` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.3
- Pull request: https://github.com/decoder-dev/mauverse/pull/24 (merged)
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/30841992477

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `eca26b3387d0f281bef8d4633ff759f3f49e8c02c650f24b51a345838a735b6c` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `080c5f3f8526294624c4414692dd263e456851cedae11c0d91ee671f1fb18092` |

## Product changes

- Dumb-user UX/bug audit schemes in `docs/UX_AUDIT_1.12.3.md`.
- Android: StateContainer loading states, news filter/error copy, HTML decode in news details, subgroup cancel path, teacher tap-to-search, schedule tab rename, friendlier login labels, certificate DOB, null guards.
- iOS: stop Moodle group id as schedule UID, native certificate form, configurable API base URL, tab-switching quick actions, retry cards, Russian date chips, softer jargon, phone/mailto contacts.
- Android engineering signing docs: `docs/ANDROID_SIGNING.md`.

## Verification

- [x] All four CI jobs green on PR #24 / run `30841992477`.
- [x] GitHub Release `v1.12.3` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing (not engineering / debug cert).
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke test from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
