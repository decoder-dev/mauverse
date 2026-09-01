# MAUverce 1.12.5 release record

Built: 2026-09-01

Classification: engineering release published as GitHub Release `v1.12.5`.
Not production-approved until remaining `docs/RELEASE_CHECKLIST.md` gates pass
on the exact promoted signed artifacts.

## Targets

| Client | Path | Version | Build |
| --- | --- | --- | --- |
| Android (MAUI) | `mauverse_mobile` | `1.12.5` | `versionCode` 37 |
| iPhone (SwiftUI) | `MauverseIOS` | `1.12.5` | `CURRENT_PROJECT_VERSION` 37 |

- Package: `com.pmi4freal.mauverse3`
- Release commit: `e36523b3e4cb9a7c3c3563c129b3946c898085d8` (`main`)
- Tag / GitHub Release: https://github.com/decoder-dev/mauverse/releases/tag/v1.12.5
- Pull request: https://github.com/decoder-dev/mauverse/pull/34 (group hints)
- CI on release artifacts: https://github.com/decoder-dev/mauverse/actions/runs/33458186006

## Published artifacts (unsigned)

| Artifact | SHA-256 |
| --- | --- |
| `mauverse-android-arm64-unsigned.aab` | `35051fe96f2885485346ff0bf9d9ef1c046105c0a0b829b486516217b29eaf07` |
| `mauverse-native-ios-arm64-unsigned.ipa` | `9a8db20d40792b1315832ace81b03d490afa2389c0a55f3eada528f93fc65ee3` |

## Product changes

- Group input hints in profile (Android + iOS): autocomplete from schedule while typing.
- API `get_subgroups` exact-match fix; null-safe subgroup handling in profile/login flows.
- Unified tab screen spacing on iOS and Android (28pt side margins, 12pt gutter, 108pt tab clearance).
- Visual alignment fixes: profile action rows, schedule date picker, news/notification card padding.
- History/authorship consolidated under `decoder-dev`.

## Verification

- [x] All four CI jobs green on release commit / run `33458186006`.
- [x] GitHub Release `v1.12.5` published with unsigned AAB/IPA and checksums.
- [ ] Protected production Android keystore signing.
- [ ] Apple distribution signing of the native IPA on an approved Mac.
- [ ] Device smoke from `docs/RELEASE_CHECKLIST.md`.
- [ ] Production API TLS, readiness, Moodle/mail monitoring, and rollback owner.
