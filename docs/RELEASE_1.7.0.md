# MAUverce 1.7.0 release record

Built: 2026-07-16 13:42:59 +03:00

Classification: engineering release candidate, not production-approved.

## Android artifact

- Application name: `MAUverce`
- Package: `com.pmi4freal.mauverse3`
- Version: `1.7.0` (`versionCode` 8)
- Archived file: `.artifacts/MAUverce-1.7.0-android-engineering.apk`
- Build output: `mauverse_mobile/bin/Release/net9.0-android/com.pmi4freal.mauverse3-Signed.apk`
- Size: 34,413,040 bytes
- SHA-256: `05D2D83E17BA2A2C974BF94851954C8805EF2136B4CFCBE24E987710C329616C`
- Signer: `CN=Android Debug, O=Android, C=US`
- Signing status: debug-signed; a protected production keystore was not used.

## Delivered

- Modernized light/dark UI, accessibility metadata, safe-area handling, responsive popups, and system-bar theming.
- Faster tab reuse with cancellation of hidden-page work and cached data paths.
- Hardened mobile HTTP, Moodle requests, secure token storage, cache concurrency, and student forms.
- Hardened API authentication, authorization, validation, rate limits, dependency timeouts, and readiness.
- Reproducible CI checks, Docker non-root health smoke, pinned Actions, and Dependabot configuration.

## Verification

- Android Release: 0 warnings, 0 errors.
- APK manifest: package, `versionName` 1.7.0, and `versionCode` 8 verified with Android build tools.
- XAML XML validation: passing.
- Backend: 25 tests passing; Ruff, compileall, and `pip check` passing.
- Source scan: no plaintext production credentials, merge markers, obsolete skeleton paths, or fake loading rows.
- Previous package id is preserved for an in-place upgrade.
- Device installation is pending because no authorized USB device is currently visible to ADB.

## Promotion gates

- Run the corrected workflow on the exact release commit in hosted CI.
- Sign an archived artifact with the protected production keystore and verify its certificate and checksum.
- Complete device smoke, TalkBack/dynamic-font, privacy, crash-reporting, and Apple-platform approval gates.
